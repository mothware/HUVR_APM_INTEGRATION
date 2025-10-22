using System.Text.Json.Serialization;

namespace GeApmClient.Models
{
    /// <summary>
    /// Thickness Measurement Location (TML) entity
    /// </summary>
    public class ThicknessMeasurementLocation
    {
        [JsonPropertyName("EntityKey")]
        public long EntityKey { get; set; }

        [JsonPropertyName("EntityId")]
        public string? EntityId { get; set; }

        [JsonPropertyName("TML_ID")]
        public string? TmlId { get; set; }

        [JsonPropertyName("TML_Description")]
        public string? Description { get; set; }

        [JsonPropertyName("Equipment_ID")]
        public string? EquipmentId { get; set; }

        [JsonPropertyName("Location")]
        public string? Location { get; set; }

        [JsonPropertyName("Nominal_Thickness")]
        public decimal? NominalThickness { get; set; }

        [JsonPropertyName("Minimum_Thickness")]
        public decimal? MinimumThickness { get; set; }

        [JsonPropertyName("Retirement_Thickness")]
        public decimal? RetirementThickness { get; set; }

        [JsonPropertyName("Corrosion_Rate")]
        public decimal? CorrosionRate { get; set; }

        [JsonPropertyName("Status")]
        public string? Status { get; set; }

        [JsonPropertyName("Component_Type")]
        public string? ComponentType { get; set; }

        [JsonPropertyName("Last_Inspection_Date")]
        public DateTime? LastInspectionDate { get; set; }

        [JsonPropertyName("Next_Inspection_Date")]
        public DateTime? NextInspectionDate { get; set; }

        /// <summary>
        /// Additional properties not explicitly mapped
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object?>? AdditionalProperties { get; set; }
    }

    /// <summary>
    /// TML Measurement (Thickness Reading) entity
    /// </summary>
    public class TmlMeasurement
    {
        [JsonPropertyName("EntityKey")]
        public long EntityKey { get; set; }

        [JsonPropertyName("EntityId")]
        public string? EntityId { get; set; }

        [JsonPropertyName("TML_ID")]
        public string? TmlId { get; set; }

        [JsonPropertyName("Measurement_Date")]
        public DateTime? MeasurementDate { get; set; }

        [JsonPropertyName("Measured_Thickness")]
        public decimal? MeasuredThickness { get; set; }

        [JsonPropertyName("Measurement_Taken_By")]
        public string? MeasurementTakenBy { get; set; }

        [JsonPropertyName("Inspection_ID")]
        public string? InspectionId { get; set; }

        [JsonPropertyName("Reading_Number")]
        public int? ReadingNumber { get; set; }

        [JsonPropertyName("Temperature")]
        public decimal? Temperature { get; set; }

        [JsonPropertyName("Comments")]
        public string? Comments { get; set; }

        [JsonPropertyName("Status")]
        public string? Status { get; set; }

        [JsonPropertyName("Created_Date")]
        public DateTime? CreatedDate { get; set; }

        [JsonPropertyName("Modified_Date")]
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Additional properties not explicitly mapped
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object?>? AdditionalProperties { get; set; }
    }

    /// <summary>
    /// Request model for creating/updating TML measurements
    /// </summary>
    public class TmlMeasurementRequest
    {
        public string TmlId { get; set; } = string.Empty;
        public DateTime MeasurementDate { get; set; }
        public decimal MeasuredThickness { get; set; }
        public string? MeasurementTakenBy { get; set; }
        public string? InspectionId { get; set; }
        public int? ReadingNumber { get; set; }
        public decimal? Temperature { get; set; }
        public string? Comments { get; set; }
        public string Status { get; set; } = "Active";
    }

    /// <summary>
    /// Request model for bulk TML measurement upload
    /// </summary>
    public class BulkTmlMeasurementRequest
    {
        public List<TmlMeasurementRequest> Measurements { get; set; } = new();
    }
}
