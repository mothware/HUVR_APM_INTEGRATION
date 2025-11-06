namespace HuvrWebApp.Models;

/// <summary>
/// Represents a saved export template that can be reused
/// </summary>
public class ExportTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ExportTemplateType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;

    // Single-sheet template data
    public ExportRequest? SingleSheetConfig { get; set; }

    // Multi-sheet template data
    public MultiSheetExportRequest? MultiSheetConfig { get; set; }
}

/// <summary>
/// Type of export template
/// </summary>
public enum ExportTemplateType
{
    SingleSheet,
    MultiSheet
}

/// <summary>
/// Request to save a new template
/// </summary>
public class SaveTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ExportTemplateType Type { get; set; }
    public ExportRequest? SingleSheetConfig { get; set; }
    public MultiSheetExportRequest? MultiSheetConfig { get; set; }
}

/// <summary>
/// Request to update an existing template
/// </summary>
public class UpdateTemplateRequest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ExportRequest? SingleSheetConfig { get; set; }
    public MultiSheetExportRequest? MultiSheetConfig { get; set; }
}

/// <summary>
/// Summary information about a template (for listing)
/// </summary>
public class ExportTemplateSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ExportTemplateType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // Summary information
    public string EntityTypes { get; set; } = string.Empty;
    public int FieldCount { get; set; }
    public int SheetCount { get; set; }
}
