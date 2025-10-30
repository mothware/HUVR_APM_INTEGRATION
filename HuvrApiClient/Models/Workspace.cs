using System.Text.Json.Serialization;
using HuvrApiClient.JsonConverters;

namespace HuvrApiClient.Models
{
    /// <summary>
    /// Represents a HUVR workspace
    /// </summary>
    public class Workspace
    {
        [JsonPropertyName("id")]
        [JsonConverter(typeof(FlexibleIdConverter))]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Response model for paginated workspace list
    /// </summary>
    public class WorkspaceListResponse
    {
        [JsonPropertyName("results")]
        public List<Workspace> Results { get; set; } = new();

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }
    }
}
