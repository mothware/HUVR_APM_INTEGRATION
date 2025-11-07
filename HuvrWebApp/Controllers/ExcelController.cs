using Microsoft.AspNetCore.Mvc;
using HuvrWebApp.Services;
using HuvrWebApp.Models;
using ClosedXML.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HuvrWebApp.Controllers
{
    public class ExcelController : Controller
    {
        private readonly IHuvrService _huvrService;
        private readonly ExportTemplateService _templateService;

        public ExcelController(IHuvrService huvrService, ExportTemplateService templateService)
        {
            _huvrService = huvrService;
            _templateService = templateService;
        }

        private HuvrApiClient.HuvrApiClient? GetClientFromSession()
        {
            var sessionId = HttpContext.Session.GetString("SessionId");
            if (sessionId == null)
            {
                return null;
            }
            return _huvrService.GetClient(sessionId);
        }

        public IActionResult FieldMapping()
        {
            var client = GetClientFromSession();
            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        public IActionResult MultiSheetMapping()
        {
            var client = GetClientFromSession();
            if (client == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            return View();
        }

        [HttpGet]
        public IActionResult GetAvailableFields(string entityType)
        {
            var fields = GetFieldsForEntityType(entityType);
            return Json(new { success = true, fields });
        }

        [HttpGet]
        public IActionResult GetAvailableFieldsWithRelationships(string entityType)
        {
            // Normalize entity type to match model names (singular, PascalCase)
            var normalizedEntityType = NormalizeEntityType(entityType);
            var fields = ModelRelationshipConfiguration.GetAvailableFieldsWithRelationships(normalizedEntityType);
            return Json(new { success = true, fields });
        }

        private List<string> GetFieldsForEntityType(string entityType)
        {
            return entityType.ToLower() switch
            {
                "assets" => new List<string> { "Id", "Name", "Description", "AssetType", "Location", "Status", "CreatedAt", "UpdatedAt" },
                "projects" => new List<string> { "Id", "Name", "Description", "AssetId", "ProjectTypeId", "Status", "StartDate", "EndDate" },
                "defects" => new List<string> { "Id", "ProjectId", "AssetId", "Title", "Description", "Severity", "Status", "DefectType", "Location", "IdentifiedBy", "IdentifiedAt" },
                "measurements" => new List<string> { "Id", "ProjectId", "AssetId", "MeasurementType", "Value", "Unit", "Location", "MeasuredBy", "MeasuredAt" },
                "inspectionmedia" => new List<string> { "Id", "ProjectId", "FileName", "FileType", "FileSize", "Status", "DownloadUrl", "ThumbnailUrl" },
                "checklists" => new List<string> { "Id", "ProjectId", "Name", "TemplateId", "Status", "CompletedBy", "CompletedAt" },
                "users" => new List<string> { "Id", "Email", "FirstName", "LastName", "Role" },
                "workspaces" => new List<string> { "Id", "Name", "Description" },
                _ => new List<string>()
            };
        }

        [HttpPost]
        public async Task<IActionResult> ExportToExcel([FromBody] ExportRequest request)
        {
            var client = GetClientFromSession();
            if (client == null)
            {
                return Json(new { success = false, error = "Not authenticated" });
            }

            try
            {
                // Normalize entity type
                var normalizedEntityType = NormalizeEntityType(request.EntityType);

                // Determine if we need related entity data
                var selectedMappings = request.Mappings.Where(m => m.IsSelected).ToList();
                var hasRelatedFields = selectedMappings.Any(m => m.ApiField.Contains('.'));

                // Fetch data from API
                var data = await FetchDataForEntityType(client, request.EntityType);

                if (data == null || !data.Any())
                {
                    return Json(new { success = false, error = "No data available" });
                }

                // Convert to JObjects for processing
                var jsonData = data.Select(d => JObject.FromObject(d)).ToList();

                // If we have related fields, fetch and cache related entity data
                Services.RelatedEntityFieldResolver? resolver = null;
                if (hasRelatedFields)
                {
                    var entityCache = new Dictionary<string, List<JObject>>();
                    entityCache[normalizedEntityType] = jsonData;

                    // Determine which related entities we need
                    var requiredRelatedEntities = Services.RelatedEntityFieldResolver.GetRequiredRelatedEntities(
                        normalizedEntityType, selectedMappings);

                    // Fetch all required related entities
                    foreach (var relatedEntityType in requiredRelatedEntities)
                    {
                        var relatedData = await FetchDataForEntityType(client, relatedEntityType);
                        if (relatedData != null && relatedData.Any())
                        {
                            entityCache[relatedEntityType] = relatedData.Select(d => JObject.FromObject(d)).ToList();
                        }
                    }

                    resolver = Services.RelatedEntityFieldResolver.BuildCache(entityCache);
                }

                // Create Excel file
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(request.EntityType);

                // Add headers
                for (int i = 0; i < selectedMappings.Count; i++)
                {
                    var mapping = selectedMappings[i];
                    var columnName = string.IsNullOrWhiteSpace(mapping.ExcelColumn)
                        ? mapping.ApiField
                        : mapping.ExcelColumn;
                    worksheet.Cell(1, i + 1).Value = columnName;
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                // Add data rows
                int row = 2;
                foreach (var jsonObject in jsonData)
                {
                    for (int i = 0; i < selectedMappings.Count; i++)
                    {
                        var mapping = selectedMappings[i];
                        object? value;

                        // Use resolver for related entity fields
                        if (hasRelatedFields && mapping.ApiField.Contains('.') && resolver != null)
                        {
                            value = resolver.ResolveFieldValue(jsonObject, mapping.ApiField, normalizedEntityType);
                        }
                        else
                        {
                            value = GetNestedValue(jsonObject, mapping.ApiField);
                        }

                        worksheet.Cell(row, i + 1).Value = value?.ToString() ?? "";
                    }
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Save to memory stream
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"{request.EntityType}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportToExcelMultiSheet([FromBody] MultiSheetExportRequest request)
        {
            var client = GetClientFromSession();
            if (client == null)
            {
                return Json(new { success = false, error = "Not authenticated" });
            }

            try
            {
                if (request.Sheets == null || !request.Sheets.Any())
                {
                    return Json(new { success = false, error = "No sheet configurations provided" });
                }

                // Create Excel file
                using var workbook = new XLWorkbook();

                // Fetch all required data upfront if linking is enabled
                var entityCache = new Dictionary<string, List<JObject>>();
                if (request.LinkRelatedData)
                {
                    // Determine all entity types needed
                    var allEntityTypes = new HashSet<string>(request.Sheets.Select(s => s.EntityType));
                    var relatedEntities = Services.RelatedEntityFieldResolver.GetRequiredRelatedEntitiesForSheets(request.Sheets);
                    foreach (var entities in relatedEntities.Values)
                    {
                        allEntityTypes.UnionWith(entities);
                    }

                    // Fetch all required data
                    foreach (var entityType in allEntityTypes)
                    {
                        var data = await FetchDataForEntityType(client, entityType);
                        if (data != null && data.Any())
                        {
                            entityCache[entityType] = data.Select(d => JObject.FromObject(d)).ToList();
                        }
                    }
                }

                // Create resolver for related entity fields
                var resolver = Services.RelatedEntityFieldResolver.BuildCache(entityCache);

                // Process each sheet
                foreach (var sheetConfig in request.Sheets)
                {
                    // Fetch data for this sheet (or use from cache)
                    List<JObject> data;
                    if (entityCache.ContainsKey(sheetConfig.EntityType))
                    {
                        data = entityCache[sheetConfig.EntityType];
                    }
                    else
                    {
                        var rawData = await FetchDataForEntityType(client, sheetConfig.EntityType);
                        data = rawData?.Select(d => JObject.FromObject(d)).ToList() ?? new List<JObject>();
                    }

                    if (!data.Any())
                    {
                        continue; // Skip empty sheets
                    }

                    // Apply filtering if specified
                    if (!string.IsNullOrEmpty(sheetConfig.FilterByParentId) && !string.IsNullOrEmpty(sheetConfig.FilterByParentType))
                    {
                        var relationship = ModelRelationshipConfiguration.GetRelationship(sheetConfig.EntityType, sheetConfig.FilterByParentType);
                        if (relationship != null)
                        {
                            data = data.Where(d =>
                            {
                                var value = GetNestedValue(d, relationship.SourceKey)?.ToString();
                                return value == sheetConfig.FilterByParentId;
                            }).ToList();
                        }
                    }

                    if (!data.Any())
                    {
                        continue; // Skip if no data after filtering
                    }

                    // Create worksheet
                    var sheetName = string.IsNullOrWhiteSpace(sheetConfig.SheetName)
                        ? sheetConfig.EntityType
                        : sheetConfig.SheetName;
                    var worksheet = workbook.Worksheets.Add(sheetName);

                    // Add headers at the start row
                    var selectedMappings = sheetConfig.Mappings.Where(m => m.IsSelected).ToList();
                    var headerRow = Math.Max(1, sheetConfig.StartRow);
                    for (int i = 0; i < selectedMappings.Count; i++)
                    {
                        var mapping = selectedMappings[i];
                        var columnName = string.IsNullOrWhiteSpace(mapping.ExcelColumn)
                            ? mapping.ApiField
                            : mapping.ExcelColumn;
                        worksheet.Cell(headerRow, i + 1).Value = columnName;
                        worksheet.Cell(headerRow, i + 1).Style.Font.Bold = true;
                        worksheet.Cell(headerRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    }

                    // Add data rows
                    int row = headerRow + 1;
                    var normalizedEntityType = NormalizeEntityType(sheetConfig.EntityType);
                    foreach (var item in data)
                    {
                        for (int i = 0; i < selectedMappings.Count; i++)
                        {
                            var mapping = selectedMappings[i];
                            object? value;

                            // Use resolver for related entity fields if linking is enabled
                            if (request.LinkRelatedData && mapping.ApiField.Contains('.'))
                            {
                                value = resolver.ResolveFieldValue(item, mapping.ApiField, normalizedEntityType);
                            }
                            else
                            {
                                value = GetNestedValue(item, mapping.ApiField);
                            }

                            worksheet.Cell(row, i + 1).Value = value?.ToString() ?? "";
                        }
                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();
                }

                if (!workbook.Worksheets.Any())
                {
                    return Json(new { success = false, error = "No data available for any sheets" });
                }

                // Save to memory stream
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"MultiSheet_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportFromTemplate(string templateId)
        {
            var client = GetClientFromSession();
            if (client == null)
            {
                return Json(new { success = false, error = "Not authenticated" });
            }

            try
            {
                // Load the template
                var template = await _templateService.GetTemplateByIdAsync(templateId);
                if (template == null)
                {
                    return Json(new { success = false, error = "Template not found" });
                }

                // Export based on template type
                if (template.Type == ExportTemplateType.SingleSheet && template.SingleSheetConfig != null)
                {
                    return await ExportToExcel(template.SingleSheetConfig);
                }
                else if (template.Type == ExportTemplateType.MultiSheet && template.MultiSheetConfig != null)
                {
                    return await ExportToExcelMultiSheet(template.MultiSheetConfig);
                }

                return Json(new { success = false, error = "Invalid template configuration" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private async Task<List<object>> FetchDataForEntityType(HuvrApiClient.HuvrApiClient client, string entityType)
        {
            var result = new List<object>();

            switch (entityType.ToLower())
            {
                case "assets":
                    var assets = await client.GetAllAssetsAsync();
                    result.AddRange(assets.Cast<object>());
                    break;
                case "projects":
                    var projects = await client.GetAllProjectsAsync();
                    result.AddRange(projects.Cast<object>());
                    break;
                case "defects":
                    var defects = await client.GetAllDefectsAsync();
                    result.AddRange(defects.Cast<object>());
                    break;
                case "measurements":
                    var measurements = await client.GetAllMeasurementsAsync();
                    result.AddRange(measurements.Cast<object>());
                    break;
                case "inspectionmedia":
                    var media = await client.GetAllInspectionMediaAsync();
                    result.AddRange(media.Cast<object>());
                    break;
                case "checklists":
                    var checklists = await client.ListChecklistsAsync();
                    if (checklists?.Results != null)
                    {
                        result.AddRange(checklists.Results.Cast<object>());
                    }
                    break;
                case "users":
                    var users = await client.ListUsersAsync();
                    if (users?.Results != null)
                    {
                        result.AddRange(users.Results.Cast<object>());
                    }
                    break;
                case "workspaces":
                    var workspaces = await client.ListWorkspacesAsync();
                    if (workspaces?.Results != null)
                    {
                        result.AddRange(workspaces.Results.Cast<object>());
                    }
                    break;
            }

            return result;
        }

        private object? GetNestedValue(JObject obj, string path)
        {
            try
            {
                var token = obj.SelectToken(path);
                return token?.Type == JTokenType.Object || token?.Type == JTokenType.Array
                    ? token.ToString(Formatting.None)
                    : token?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private string NormalizeEntityType(string entityType)
        {
            return entityType.ToLower() switch
            {
                "assets" => "Asset",
                "projects" => "Project",
                "defects" => "Defect",
                "measurements" => "Measurement",
                "inspectionmedia" => "InspectionMedia",
                "checklists" => "Checklist",
                "users" => "User",
                "workspaces" => "Workspace",
                "libraries" => "Library",
                "librarymedia" => "LibraryMedia",
                "defectoverlays" => "DefectOverlay",
                _ => entityType
            };
        }
    }
}
