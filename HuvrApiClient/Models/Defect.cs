using System.Text.Json.Serialization;
using HuvrApiClient.JsonConverters;

namespace HuvrApiClient.Models
{
    /// <summary>
    /// Represents inspection findings and defects
    /// </summary>
    public class Defect
    {
        [JsonPropertyName("id")]
        [JsonConverter(typeof(FlexibleIdConverter))]
        public string? Id { get; set; }

        [JsonPropertyName("asset")]
        public Asset? Asset { get; set; }

        [JsonPropertyName("overlays")]
        public List<DefectOverlay>? Overlays { get; set; }

        [JsonPropertyName("project_name")]
        public string? ProjectName { get; set; }

        [JsonPropertyName("created_by")]
        public User? CreatedBy { get; set; }

        [JsonPropertyName("state_note")]
        public string? StateNote { get; set; }

        [JsonPropertyName("created_on")]
        public DateTime? CreatedOn { get; set; }

        [JsonPropertyName("updated_on")]
        public DateTime? UpdatedOn { get; set; }

        [JsonPropertyName("labels")]
        public List<string>? Labels { get; set; }

        [JsonPropertyName("geo_point")]
        public object? GeoPoint { get; set; }

        [JsonPropertyName("heading")]
        public double? Heading { get; set; }

        [JsonPropertyName("pitch")]
        public double? Pitch { get; set; }

        [JsonPropertyName("local_coordinates")]
        public object? LocalCoordinates { get; set; }

        [JsonPropertyName("component")]
        public string? Component { get; set; }

        [JsonPropertyName("component_display")]
        public string? ComponentDisplay { get; set; }

        [JsonPropertyName("location_zone")]
        public string? LocationZone { get; set; }

        [JsonPropertyName("location_zone_display")]
        public string? LocationZoneDisplay { get; set; }

        [JsonPropertyName("location_code")]
        public string? LocationCode { get; set; }

        [JsonPropertyName("location_code_display")]
        public string? LocationCodeDisplay { get; set; }

        [JsonPropertyName("access")]
        public string? Access { get; set; }

        [JsonPropertyName("access_display")]
        public string? AccessDisplay { get; set; }

        [JsonPropertyName("client_id")]
        public string? ClientId { get; set; }

        [JsonPropertyName("external_id")]
        public string? ExternalId { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("severity")]
        public int? Severity { get; set; }

        [JsonPropertyName("severity_display")]
        public string? SeverityDisplay { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("type_display")]
        public string? TypeDisplay { get; set; }

        [JsonPropertyName("sub_type")]
        public string? SubType { get; set; }

        [JsonPropertyName("sub_type_display")]
        public string? SubTypeDisplay { get; set; }

        [JsonPropertyName("length")]
        public double? Length { get; set; }

        [JsonPropertyName("width")]
        public double? Width { get; set; }

        [JsonPropertyName("area")]
        public double? Area { get; set; }

        [JsonPropertyName("is_locked")]
        public bool IsLocked { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("checklist_line")]
        public string? ChecklistLine { get; set; }

        [JsonPropertyName("captured_on")]
        public DateTime? CapturedOn { get; set; }

        [JsonPropertyName("priority")]
        public string? Priority { get; set; }

        [JsonPropertyName("repair_by")]
        public DateTime? RepairBy { get; set; }

        [JsonPropertyName("resolved_on")]
        public DateTime? ResolvedOn { get; set; }

        [JsonPropertyName("resolution_duration")]
        public int? ResolutionDuration { get; set; }

        [JsonPropertyName("next_inspection_date")]
        public DateTime? NextInspectionDate { get; set; }

        [JsonPropertyName("layer")]
        public int? Layer { get; set; }

        [JsonPropertyName("profile")]
        public int? Profile { get; set; }

        [JsonPropertyName("checklist")]
        public int? Checklist { get; set; }

        [JsonPropertyName("project")]
        public int? Project { get; set; }

        [JsonPropertyName("cml")]
        public int? Cml { get; set; }

        [JsonPropertyName("newer_defect")]
        public int? NewerDefect { get; set; }
    }

    /// <summary>
    /// Represents an overlay on a defect (annotation on media)
    /// </summary>
    public class DefectOverlay
    {
        [JsonPropertyName("id")]
        [JsonConverter(typeof(FlexibleIdConverter))]
        public string? Id { get; set; }

        [JsonPropertyName("defect")]
        [JsonConverter(typeof(FlexibleIdConverter))]
        public string? DefectId { get; set; }

        [JsonPropertyName("media")]
        public InspectionMedia? Media { get; set; }

        [JsonPropertyName("geometry")]
        public object? Geometry { get; set; }

        [JsonPropertyName("geometry_extra")]
        public string? GeometryExtra { get; set; }

        [JsonPropertyName("note")]
        public string? Note { get; set; }

        [JsonPropertyName("created_by")]
        public User? CreatedBy { get; set; }

        [JsonPropertyName("created_on")]
        public DateTime? CreatedOn { get; set; }

        [JsonPropertyName("updated_on")]
        public DateTime? UpdatedOn { get; set; }

        [JsonPropertyName("display_url")]
        public string? DisplayUrl { get; set; }
    }

    /// <summary>
    /// Request model for creating or updating a defect
    /// </summary>
    public class DefectRequest
    {
        [JsonPropertyName("project_id")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("asset_id")]
        public string? AssetId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("severity")]
        public string? Severity { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("defect_type")]
        public string? DefectType { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Response model for paginated defect list
    /// </summary>
    public class DefectListResponse
    {
        [JsonPropertyName("results")]
        public List<Defect> Results { get; set; } = new();

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }
    }
}
