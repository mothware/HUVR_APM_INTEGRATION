using System.Text.Json.Serialization;
using HuvrApiClient.JsonConverters;

namespace HuvrApiClient.Models
{
    /// <summary>
    /// Represents detailed measurement data (e.g., Ultrasonic Testing)
    /// </summary>
    public class Measurement
    {
        [JsonPropertyName("id")]
        [JsonConverter(typeof(FlexibleIdConverter))]
        public string? Id { get; set; }

        [JsonPropertyName("project_id")]
        public string? ProjectId { get; set; }

        [JsonPropertyName("asset_id")]
        public string? AssetId { get; set; }

        [JsonPropertyName("measurement_type")]
        public string? MeasurementType { get; set; }

        [JsonPropertyName("value")]
        public decimal? Value { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("measured_by")]
        public string? MeasuredBy { get; set; }

        [JsonPropertyName("measured_at")]
        public DateTime? MeasuredAt { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Request model for creating or updating a measurement
    /// </summary>
    public class MeasurementRequest
    {
        [JsonPropertyName("project_id")]
        public string ProjectId { get; set; } = string.Empty;

        [JsonPropertyName("asset_id")]
        public string? AssetId { get; set; }

        [JsonPropertyName("measurement_type")]
        public string? MeasurementType { get; set; }

        [JsonPropertyName("value")]
        public decimal? Value { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Response model for paginated measurement list
    /// </summary>
    public class MeasurementListResponse
    {
        [JsonPropertyName("results")]
        public List<Measurement> Results { get; set; } = new();

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }
    }
}
