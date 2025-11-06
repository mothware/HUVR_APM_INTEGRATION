namespace HuvrWebApp.Models
{
    public class FieldMappingViewModel
    {
        public string EntityType { get; set; } = string.Empty;
        public List<FieldMapping> Mappings { get; set; } = new();
        public List<string> AvailableApiFields { get; set; } = new();
    }

    public class FieldMapping
    {
        public string ApiField { get; set; } = string.Empty;
        public string ExcelColumn { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = true;
    }

    public class ExportRequest
    {
        public string EntityType { get; set; } = string.Empty;
        public List<FieldMapping> Mappings { get; set; } = new();
    }

    /// <summary>
    /// Configuration for a single sheet in a multi-sheet export
    /// </summary>
    public class SheetConfiguration
    {
        public string SheetName { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public List<FieldMapping> Mappings { get; set; } = new();
        public int StartRow { get; set; } = 1;
        public string? FilterByParentId { get; set; }
        public string? FilterByParentType { get; set; }
    }

    /// <summary>
    /// Request model for multi-sheet Excel export
    /// </summary>
    public class MultiSheetExportRequest
    {
        public List<SheetConfiguration> Sheets { get; set; } = new();
        public bool LinkRelatedData { get; set; } = true;
    }
}
