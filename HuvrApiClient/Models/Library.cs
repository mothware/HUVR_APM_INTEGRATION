using System.Text.Json.Serialization;
using HuvrApiClient.JsonConverters;

namespace HuvrApiClient.Models
{
    /// <summary>
    /// Represents a library - collection of reference documents and media
    /// </summary>
    public class Library
    {
        [JsonPropertyName("id")]
        [JsonConverter(typeof(FlexibleIdConverter))]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("system")]
        public bool System { get; set; }

        [JsonPropertyName("sync_mode")]
        public string? SyncMode { get; set; }
    }

    /// <summary>
    /// Represents media/documents within a library
    /// </summary>
    public class LibraryMedia
    {
        [JsonPropertyName("id")]
        [JsonConverter(typeof(FlexibleIdConverter))]
        public string? Id { get; set; }

        [JsonPropertyName("library")]
        public Library? Library { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonPropertyName("preview")]
        public string? Preview { get; set; }

        [JsonPropertyName("file")]
        public string? File { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("media_type")]
        public string? MediaType { get; set; }

        [JsonPropertyName("document_category")]
        public string? DocumentCategory { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("generation")]
        public int Generation { get; set; }

        [JsonPropertyName("created_on")]
        public DateTime? CreatedOn { get; set; }

        [JsonPropertyName("updated_on")]
        public DateTime? UpdatedOn { get; set; }

        [JsonPropertyName("uploaded")]
        public bool Uploaded { get; set; }

        [JsonPropertyName("upload")]
        public string? Upload { get; set; }

        [JsonPropertyName("huvr_url")]
        public string? HuvrUrl { get; set; }
    }

    /// <summary>
    /// Response model for paginated library media list
    /// </summary>
    public class LibraryMediaListResponse
    {
        [JsonPropertyName("results")]
        public List<LibraryMedia> Results { get; set; } = new();

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }
    }
}
