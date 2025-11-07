using Microsoft.AspNetCore.Mvc;
using HuvrWebApp.Models;
using HuvrWebApp.Services;

namespace HuvrWebApp.Controllers;

public class TemplateController : Controller
{
    private readonly ExportTemplateService _templateService;

    public TemplateController(ExportTemplateService templateService)
    {
        _templateService = templateService;
    }

    /// <summary>
    /// Template management page
    /// </summary>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Get all templates
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllTemplates()
    {
        try
        {
            var templates = await _templateService.GetAllTemplateSummariesAsync();
            return Json(new { success = true, templates });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific template by ID
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTemplate(string id)
    {
        try
        {
            var template = await _templateService.GetTemplateByIdAsync(id);
            if (template == null)
            {
                return Json(new { success = false, error = "Template not found" });
            }
            return Json(new { success = true, template });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get templates by type
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTemplatesByType(ExportTemplateType type)
    {
        try
        {
            var templates = await _templateService.GetTemplatesByTypeAsync(type);
            var summaries = templates.Select(t => new ExportTemplateSummary
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Type = t.Type,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CreatedBy = t.CreatedBy
            }).ToList();
            return Json(new { success = true, templates = summaries });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Save a new template
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SaveTemplate([FromBody] SaveTemplateRequest request)
    {
        try
        {
            // Get user ID from session (or default)
            var userId = HttpContext.Session.GetString("SessionId") ?? "anonymous";

            var template = await _templateService.SaveTemplateAsync(request, userId);
            return Json(new { success = true, template, message = "Template saved successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing template
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateTemplate([FromBody] UpdateTemplateRequest request)
    {
        try
        {
            var userId = HttpContext.Session.GetString("SessionId") ?? "anonymous";

            var template = await _templateService.UpdateTemplateAsync(request, userId);
            if (template == null)
            {
                return Json(new { success = false, error = "Template not found" });
            }
            return Json(new { success = true, template, message = "Template updated successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a template
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DeleteTemplate(string id)
    {
        try
        {
            var deleted = await _templateService.DeleteTemplateAsync(id);
            if (!deleted)
            {
                return Json(new { success = false, error = "Template not found" });
            }
            return Json(new { success = true, message = "Template deleted successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Duplicate a template
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DuplicateTemplate(string id)
    {
        try
        {
            var userId = HttpContext.Session.GetString("SessionId") ?? "anonymous";

            var template = await _templateService.DuplicateTemplateAsync(id, userId);
            return Json(new { success = true, template, message = "Template duplicated successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Search templates
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SearchTemplates(string searchTerm)
    {
        try
        {
            var templates = await _templateService.SearchTemplatesAsync(searchTerm);
            var summaries = templates.Select(t => new ExportTemplateSummary
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Type = t.Type,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CreatedBy = t.CreatedBy
            }).ToList();
            return Json(new { success = true, templates = summaries });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }
}
