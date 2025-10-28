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

        public ExcelController(IHuvrService huvrService)
        {
            _huvrService = huvrService;
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

        [HttpGet]
        public IActionResult GetAvailableFields(string entityType)
        {
            var fields = GetFieldsForEntityType(entityType);
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
                // Fetch data from API
                var data = await FetchDataForEntityType(client, request.EntityType);

                if (data == null || !data.Any())
                {
                    return Json(new { success = false, error = "No data available" });
                }

                // Create Excel file
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(request.EntityType);

                // Add headers
                var selectedMappings = request.Mappings.Where(m => m.IsSelected).ToList();
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
                foreach (var item in data)
                {
                    var jsonObject = JObject.FromObject(item);

                    for (int i = 0; i < selectedMappings.Count; i++)
                    {
                        var mapping = selectedMappings[i];
                        var value = GetNestedValue(jsonObject, mapping.ApiField);
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
    }
}
