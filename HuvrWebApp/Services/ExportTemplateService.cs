using HuvrWebApp.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace HuvrWebApp.Services;

/// <summary>
/// Service for managing export templates with file-based storage
/// </summary>
public class ExportTemplateService
{
    private readonly string _templatesDirectory;
    private readonly string _templatesFilePath;
    private readonly ConcurrentDictionary<string, ExportTemplate> _templatesCache;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public ExportTemplateService(IWebHostEnvironment environment)
    {
        _templatesDirectory = Path.Combine(environment.ContentRootPath, "Data", "ExportTemplates");
        _templatesFilePath = Path.Combine(_templatesDirectory, "templates.json");
        _templatesCache = new ConcurrentDictionary<string, ExportTemplate>();

        // Ensure directory exists
        Directory.CreateDirectory(_templatesDirectory);

        // Load existing templates
        LoadTemplatesFromFile();
    }

    /// <summary>
    /// Get all templates
    /// </summary>
    public async Task<List<ExportTemplate>> GetAllTemplatesAsync()
    {
        await EnsureLoadedAsync();
        return _templatesCache.Values.OrderByDescending(t => t.UpdatedAt).ToList();
    }

    /// <summary>
    /// Get all template summaries
    /// </summary>
    public async Task<List<ExportTemplateSummary>> GetAllTemplateSummariesAsync()
    {
        var templates = await GetAllTemplatesAsync();
        return templates.Select(t => new ExportTemplateSummary
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            Type = t.Type,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            CreatedBy = t.CreatedBy,
            EntityTypes = GetEntityTypesSummary(t),
            FieldCount = GetFieldCount(t),
            SheetCount = GetSheetCount(t)
        }).ToList();
    }

    /// <summary>
    /// Get a template by ID
    /// </summary>
    public async Task<ExportTemplate?> GetTemplateByIdAsync(string id)
    {
        await EnsureLoadedAsync();
        _templatesCache.TryGetValue(id, out var template);
        return template;
    }

    /// <summary>
    /// Save a new template
    /// </summary>
    public async Task<ExportTemplate> SaveTemplateAsync(SaveTemplateRequest request, string userId)
    {
        var template = new ExportTemplate
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            SingleSheetConfig = request.SingleSheetConfig,
            MultiSheetConfig = request.MultiSheetConfig
        };

        _templatesCache.TryAdd(template.Id, template);
        await SaveTemplatesToFileAsync();

        return template;
    }

    /// <summary>
    /// Update an existing template
    /// </summary>
    public async Task<ExportTemplate?> UpdateTemplateAsync(UpdateTemplateRequest request, string userId)
    {
        if (!_templatesCache.TryGetValue(request.Id, out var template))
        {
            return null;
        }

        template.Name = request.Name;
        template.Description = request.Description;
        template.UpdatedAt = DateTime.UtcNow;

        if (request.SingleSheetConfig != null)
        {
            template.SingleSheetConfig = request.SingleSheetConfig;
        }

        if (request.MultiSheetConfig != null)
        {
            template.MultiSheetConfig = request.MultiSheetConfig;
        }

        await SaveTemplatesToFileAsync();

        return template;
    }

    /// <summary>
    /// Delete a template
    /// </summary>
    public async Task<bool> DeleteTemplateAsync(string id)
    {
        var removed = _templatesCache.TryRemove(id, out _);
        if (removed)
        {
            await SaveTemplatesToFileAsync();
        }
        return removed;
    }

    /// <summary>
    /// Search templates by name
    /// </summary>
    public async Task<List<ExportTemplate>> SearchTemplatesAsync(string searchTerm)
    {
        await EnsureLoadedAsync();
        var lowerSearch = searchTerm.ToLower();
        return _templatesCache.Values
            .Where(t => t.Name.ToLower().Contains(lowerSearch) ||
                       t.Description.ToLower().Contains(lowerSearch))
            .OrderByDescending(t => t.UpdatedAt)
            .ToList();
    }

    /// <summary>
    /// Get templates by type
    /// </summary>
    public async Task<List<ExportTemplate>> GetTemplatesByTypeAsync(ExportTemplateType type)
    {
        await EnsureLoadedAsync();
        return _templatesCache.Values
            .Where(t => t.Type == type)
            .OrderByDescending(t => t.UpdatedAt)
            .ToList();
    }

    /// <summary>
    /// Duplicate a template
    /// </summary>
    public async Task<ExportTemplate> DuplicateTemplateAsync(string id, string userId)
    {
        var original = await GetTemplateByIdAsync(id);
        if (original == null)
        {
            throw new ArgumentException($"Template with ID {id} not found");
        }

        var duplicate = new ExportTemplate
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"{original.Name} (Copy)",
            Description = original.Description,
            Type = original.Type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            SingleSheetConfig = original.SingleSheetConfig,
            MultiSheetConfig = original.MultiSheetConfig
        };

        _templatesCache.TryAdd(duplicate.Id, duplicate);
        await SaveTemplatesToFileAsync();

        return duplicate;
    }

    // Private helper methods

    private void LoadTemplatesFromFile()
    {
        if (!File.Exists(_templatesFilePath))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(_templatesFilePath);
            var templates = JsonConvert.DeserializeObject<List<ExportTemplate>>(json);

            if (templates != null)
            {
                foreach (var template in templates)
                {
                    _templatesCache.TryAdd(template.Id, template);
                }
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - start with empty cache
            Console.WriteLine($"Error loading templates: {ex.Message}");
        }
    }

    private async Task SaveTemplatesToFileAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            var templates = _templatesCache.Values.ToList();
            var json = JsonConvert.SerializeObject(templates, Formatting.Indented);
            await File.WriteAllTextAsync(_templatesFilePath, json);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task EnsureLoadedAsync()
    {
        // Templates are loaded in constructor, this is just for consistency
        await Task.CompletedTask;
    }

    private string GetEntityTypesSummary(ExportTemplate template)
    {
        if (template.Type == ExportTemplateType.SingleSheet && template.SingleSheetConfig != null)
        {
            return template.SingleSheetConfig.EntityType;
        }

        if (template.Type == ExportTemplateType.MultiSheet && template.MultiSheetConfig != null)
        {
            var entityTypes = template.MultiSheetConfig.Sheets
                .Select(s => s.EntityType)
                .Distinct()
                .ToList();
            return string.Join(", ", entityTypes);
        }

        return "Unknown";
    }

    private int GetFieldCount(ExportTemplate template)
    {
        if (template.Type == ExportTemplateType.SingleSheet && template.SingleSheetConfig != null)
        {
            return template.SingleSheetConfig.Mappings.Count(m => m.IsSelected);
        }

        if (template.Type == ExportTemplateType.MultiSheet && template.MultiSheetConfig != null)
        {
            return template.MultiSheetConfig.Sheets
                .Sum(s => s.Mappings.Count(m => m.IsSelected));
        }

        return 0;
    }

    private int GetSheetCount(ExportTemplate template)
    {
        if (template.Type == ExportTemplateType.MultiSheet && template.MultiSheetConfig != null)
        {
            return template.MultiSheetConfig.Sheets.Count;
        }

        return 1;
    }
}
