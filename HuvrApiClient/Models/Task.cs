using System.Text.Json.Serialization;
using HuvrApiClient.JsonConverters;

namespace HuvrApiClient.Models
{
    /// <summary>
    /// Represents a task assigned to a user within a project
    /// </summary>
    public class Task
    {
        [JsonPropertyName("id")]
        [JsonConverter(typeof(FlexibleIdConverter))]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("priority")]
        public string? Priority { get; set; }

        [JsonPropertyName("assigned_to")]
        public string? AssignedToUserId { get; set; }

        [JsonPropertyName("assigned_to_user")]
        public User? AssignedToUser { get; set; }

        [JsonPropertyName("assigned_by")]
        public string? AssignedByUserId { get; set; }

        [JsonPropertyName("assigned_by_user")]
        public User? AssignedByUser { get; set; }

        [JsonPropertyName("project_id")]
        public string? ProjectId { get; set; }

        [JsonPropertyName("project")]
        public Project? Project { get; set; }

        [JsonPropertyName("due_date")]
        public DateTime? DueDate { get; set; }

        [JsonPropertyName("completed_date")]
        public DateTime? CompletedDate { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Request model for creating or updating a task
    /// </summary>
    public class TaskRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("priority")]
        public string? Priority { get; set; }

        [JsonPropertyName("assigned_to")]
        public string? AssignedToUserId { get; set; }

        [JsonPropertyName("project_id")]
        public string? ProjectId { get; set; }

        [JsonPropertyName("due_date")]
        public DateTime? DueDate { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Response model for paginated task list
    /// </summary>
    public class TaskListResponse
    {
        [JsonPropertyName("results")]
        public List<Task> Results { get; set; } = new();

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }
    }

    /// <summary>
    /// Contains a task with all its associated project data (assets, findings, library items, CMLs)
    /// </summary>
    public class TaskWithProjectData
    {
        public Task Task { get; set; } = new();
        public Project? Project { get; set; }
        public List<Asset> Assets { get; set; } = new();
        public List<Defect> Findings { get; set; } = new();
        public List<LibraryMedia> LibraryItems { get; set; } = new();
        public List<Measurement> Measurements { get; set; } = new();
        public List<InspectionMedia> InspectionMedia { get; set; } = new();
    }
}
