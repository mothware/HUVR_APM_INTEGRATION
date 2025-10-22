using System.Text.Json.Serialization;

namespace GeApmClient.Models
{
    /// <summary>
    /// Base Data Transfer Object for GE APM entities
    /// </summary>
    public class EntityDto
    {
        /// <summary>
        /// Entity Key - Unique identifier (Int64). Must be 0 for INSERT operations.
        /// </summary>
        [JsonPropertyName("EntityKey")]
        public long EntityKey { get; set; }

        /// <summary>
        /// Family Key - Required for all operations
        /// </summary>
        [JsonPropertyName("FamilyKey")]
        public string FamilyKey { get; set; } = string.Empty;

        /// <summary>
        /// Entity ID - User-defined identifier (not necessarily unique)
        /// </summary>
        [JsonPropertyName("EntityId")]
        public string? EntityId { get; set; }

        /// <summary>
        /// Properties dictionary containing all entity fields
        /// </summary>
        [JsonPropertyName("Properties")]
        public Dictionary<string, object?>? Properties { get; set; }
    }

    /// <summary>
    /// Response wrapper for OData queries
    /// </summary>
    /// <typeparam name="T">Type of entity being queried</typeparam>
    public class ODataResponse<T>
    {
        [JsonPropertyName("value")]
        public List<T> Value { get; set; } = new();

        [JsonPropertyName("@odata.count")]
        public int? Count { get; set; }

        [JsonPropertyName("@odata.nextLink")]
        public string? NextLink { get; set; }
    }

    /// <summary>
    /// Relationship Data Transfer Object
    /// </summary>
    public class RelationshipDto
    {
        /// <summary>
        /// Relationship Key - Must be 0 for INSERT operations
        /// </summary>
        [JsonPropertyName("RelationshipKey")]
        public long RelationshipKey { get; set; }

        /// <summary>
        /// Family Key for the relationship
        /// </summary>
        [JsonPropertyName("FamilyKey")]
        public string FamilyKey { get; set; } = string.Empty;

        /// <summary>
        /// Left entity key in the relationship
        /// </summary>
        [JsonPropertyName("LeftEntityKey")]
        public long LeftEntityKey { get; set; }

        /// <summary>
        /// Right entity key in the relationship
        /// </summary>
        [JsonPropertyName("RightEntityKey")]
        public long RightEntityKey { get; set; }

        /// <summary>
        /// Optional properties (legacy relationships only)
        /// </summary>
        [JsonPropertyName("Properties")]
        public Dictionary<string, object?>? Properties { get; set; }
    }
}
