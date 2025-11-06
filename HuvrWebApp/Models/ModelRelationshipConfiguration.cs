namespace HuvrWebApp.Models;

/// <summary>
/// Defines relationships between models based on their Id fields
/// </summary>
public class ModelRelationshipConfiguration
{
    private static readonly Dictionary<string, List<RelationshipDefinition>> _relationships = new()
    {
        ["Project"] = new List<RelationshipDefinition>
        {
            new("Asset", "AssetId", "Id", "Parent asset information"),
            new("Defect", "Id", "ProjectId", "Related defects", isCollection: true),
            new("Checklist", "Id", "ProjectId", "Related checklists", isCollection: true),
            new("Measurement", "Id", "ProjectId", "Related measurements", isCollection: true),
            new("InspectionMedia", "Id", "ProjectId", "Related inspection media", isCollection: true)
        },
        ["Asset"] = new List<RelationshipDefinition>
        {
            new("Project", "Id", "AssetId", "Projects using this asset", isCollection: true),
            new("Library", "LibraryId", "Id", "Associated library"),
            new("Defect", "Id", "AssetId", "Related defects", isCollection: true),
            new("Measurement", "Id", "AssetId", "Related measurements", isCollection: true)
        },
        ["Defect"] = new List<RelationshipDefinition>
        {
            new("Project", "ProjectId", "Id", "Parent project"),
            new("Asset", "AssetId", "Id", "Related asset"),
            new("User", "IdentifiedBy", "Id", "User who identified the defect"),
            new("DefectOverlay", "Id", "DefectId", "Defect overlays", isCollection: true)
        },
        ["DefectOverlay"] = new List<RelationshipDefinition>
        {
            new("Defect", "DefectId", "Id", "Parent defect"),
            new("InspectionMedia", "MediaId", "Id", "Associated media"),
            new("User", "CreatedBy", "Id", "User who created the overlay")
        },
        ["Checklist"] = new List<RelationshipDefinition>
        {
            new("Project", "ProjectId", "Id", "Parent project")
        },
        ["Measurement"] = new List<RelationshipDefinition>
        {
            new("Project", "ProjectId", "Id", "Parent project"),
            new("Asset", "AssetId", "Id", "Related asset")
        },
        ["InspectionMedia"] = new List<RelationshipDefinition>
        {
            new("Project", "ProjectId", "Id", "Parent project")
        },
        ["Library"] = new List<RelationshipDefinition>
        {
            new("Asset", "Id", "LibraryId", "Assets using this library", isCollection: true),
            new("LibraryMedia", "Id", "LibraryId", "Library media items", isCollection: true)
        },
        ["LibraryMedia"] = new List<RelationshipDefinition>
        {
            new("Library", "LibraryId", "Id", "Parent library")
        },
        ["User"] = new List<RelationshipDefinition>
        {
            new("Defect", "Id", "IdentifiedBy", "Defects identified by this user", isCollection: true),
            new("DefectOverlay", "Id", "CreatedBy", "Overlays created by this user", isCollection: true)
        }
    };

    /// <summary>
    /// Get all relationships for a given entity type
    /// </summary>
    public static List<RelationshipDefinition> GetRelationships(string entityType)
    {
        return _relationships.TryGetValue(entityType, out var relationships)
            ? relationships
            : new List<RelationshipDefinition>();
    }

    /// <summary>
    /// Get a specific relationship between two entity types
    /// </summary>
    public static RelationshipDefinition? GetRelationship(string sourceEntity, string targetEntity)
    {
        return GetRelationships(sourceEntity)
            .FirstOrDefault(r => r.TargetEntity.Equals(targetEntity, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if two entities have a relationship
    /// </summary>
    public static bool HasRelationship(string sourceEntity, string targetEntity)
    {
        return GetRelationship(sourceEntity, targetEntity) != null;
    }

    /// <summary>
    /// Get all available fields for an entity including related entity fields
    /// </summary>
    public static List<AvailableField> GetAvailableFieldsWithRelationships(string entityType)
    {
        var fields = new List<AvailableField>();

        // Add direct fields from the entity
        var directFields = GetDirectFieldsForEntity(entityType);
        fields.AddRange(directFields.Select(f => new AvailableField
        {
            FieldPath = f,
            DisplayName = f,
            EntityType = entityType,
            IsRelated = false
        }));

        // Add related entity fields
        var relationships = GetRelationships(entityType);
        foreach (var relationship in relationships.Where(r => !r.IsCollection))
        {
            var relatedFields = GetDirectFieldsForEntity(relationship.TargetEntity);
            fields.AddRange(relatedFields.Select(f => new AvailableField
            {
                FieldPath = $"{relationship.TargetEntity}.{f}",
                DisplayName = $"{relationship.TargetEntity}.{f}",
                EntityType = entityType,
                IsRelated = true,
                RelatedEntity = relationship.TargetEntity,
                Description = relationship.Description
            }));
        }

        return fields;
    }

    private static List<string> GetDirectFieldsForEntity(string entityType)
    {
        return entityType switch
        {
            "Asset" => new List<string> { "Id", "Name", "Description", "AssetType", "Location", "Status", "CreatedAt", "UpdatedAt", "ExternalId" },
            "Project" => new List<string> { "Id", "Name", "Description", "AssetId", "ProjectTypeId", "Status", "StartDate", "EndDate", "CreatedAt", "UpdatedAt" },
            "Defect" => new List<string> { "Id", "ProjectId", "AssetId", "Title", "Description", "Severity", "Status", "DefectType", "Location", "IdentifiedBy", "IdentifiedAt" },
            "Measurement" => new List<string> { "Id", "ProjectId", "AssetId", "MeasurementType", "Value", "Unit", "Location", "MeasuredBy", "MeasuredAt" },
            "InspectionMedia" => new List<string> { "Id", "ProjectId", "FileName", "FileType", "FileSize", "Status", "DownloadUrl", "ThumbnailUrl", "UploadedAt" },
            "Checklist" => new List<string> { "Id", "ProjectId", "Name", "TemplateId", "Status", "CompletedBy", "CompletedAt" },
            "User" => new List<string> { "Id", "Email", "FirstName", "LastName", "Role" },
            "Workspace" => new List<string> { "Id", "Name", "Description" },
            "Library" => new List<string> { "Id", "Name", "Description", "LibraryType" },
            "LibraryMedia" => new List<string> { "Id", "LibraryId", "FileName", "FileType", "FileSize" },
            "DefectOverlay" => new List<string> { "Id", "DefectId", "MediaId", "CreatedBy", "CreatedAt" },
            _ => new List<string>()
        };
    }
}

/// <summary>
/// Represents a relationship between two entities
/// </summary>
public class RelationshipDefinition
{
    public string TargetEntity { get; set; }
    public string SourceKey { get; set; }
    public string TargetKey { get; set; }
    public string Description { get; set; }
    public bool IsCollection { get; set; }

    public RelationshipDefinition(string targetEntity, string sourceKey, string targetKey, string description, bool isCollection = false)
    {
        TargetEntity = targetEntity;
        SourceKey = sourceKey;
        TargetKey = targetKey;
        Description = description;
        IsCollection = isCollection;
    }
}

/// <summary>
/// Represents an available field for mapping
/// </summary>
public class AvailableField
{
    public string FieldPath { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public bool IsRelated { get; set; }
    public string? RelatedEntity { get; set; }
    public string? Description { get; set; }
}
