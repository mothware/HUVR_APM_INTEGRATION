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
}
