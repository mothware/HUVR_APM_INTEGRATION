using System.Text.Json.Serialization;
using HuvrApiClient.JsonConverters;

namespace HuvrApiClient.Models
{
    /// <summary>
    /// Represents a project (work order) - inspections, repairs, surveys
    /// </summary>
    public class Project
    {
        [JsonPropertyName("id")]
        [JsonConverter(typeof(FlexibleIdConverter))]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("asset_id")]
        public string? AssetId { get; set; }

        [JsonPropertyName("parent")]
        public Asset? Parent { get; set; }

        [JsonPropertyName("project_type_id")]
        public string? ProjectTypeId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime? StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("assigned_users")]
        public List<string>? AssignedUsers { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("library")]
        public Library? Library { get; set; }

        [JsonPropertyName("defects")]
        public List<Defect>? Defects { get; set; }
    }

    /// <summary>
    /// Request model for creating or updating a project
    /// </summary>
    public class ProjectRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("asset_id")]
        public string AssetId { get; set; } = string.Empty;

        [JsonPropertyName("project_type_id")]
        public string ProjectTypeId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime? StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("assigned_users")]
        public List<string>? AssignedUsers { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Response model for paginated project list
    /// </summary>
    public class ProjectListResponse
    {
        [JsonPropertyName("results")]
        public List<Project> Results { get; set; } = new();

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }
    }
}
