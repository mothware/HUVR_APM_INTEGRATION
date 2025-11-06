using HuvrWebApp.Models;
using Newtonsoft.Json.Linq;

namespace HuvrWebApp.Services;

/// <summary>
/// Resolves field values from related entities based on relationship configurations
/// </summary>
public class RelatedEntityFieldResolver
{
    private readonly Dictionary<string, List<JObject>> _entityCache = new();

    /// <summary>
    /// Add entities to the cache for relationship resolution
    /// </summary>
    public void AddEntitiesToCache(string entityType, List<JObject> entities)
    {
        _entityCache[entityType] = entities;
    }

    /// <summary>
    /// Clear the entity cache
    /// </summary>
    public void ClearCache()
    {
        _entityCache.Clear();
    }

    /// <summary>
    /// Resolve a field value, including related entity fields
    /// </summary>
    public object? ResolveFieldValue(JObject entity, string fieldPath, string entityType)
    {
        if (string.IsNullOrEmpty(fieldPath))
            return null;

        // Check if this is a related entity field (contains a dot)
        if (fieldPath.Contains('.'))
        {
            var parts = fieldPath.Split('.', 2);
            var relatedEntityType = parts[0];
            var relatedFieldPath = parts[1];

            return ResolveRelatedEntityField(entity, entityType, relatedEntityType, relatedFieldPath);
        }

        // Direct field access
        return GetNestedValue(entity, fieldPath);
    }

    /// <summary>
    /// Resolve a field from a related entity
    /// </summary>
    private object? ResolveRelatedEntityField(JObject entity, string sourceEntityType, string targetEntityType, string fieldPath)
    {
        // Get the relationship definition
        var relationship = ModelRelationshipConfiguration.GetRelationship(sourceEntityType, targetEntityType);
        if (relationship == null)
            return null;

        // Get the foreign key value from the source entity
        var foreignKeyValue = GetNestedValue(entity, relationship.SourceKey)?.ToString();
        if (string.IsNullOrEmpty(foreignKeyValue))
            return null;

        // Find the related entity in the cache
        if (!_entityCache.TryGetValue(targetEntityType, out var relatedEntities))
            return null;

        var relatedEntity = relatedEntities.FirstOrDefault(e =>
        {
            var targetKeyValue = GetNestedValue(e, relationship.TargetKey)?.ToString();
            return targetKeyValue == foreignKeyValue;
        });

        if (relatedEntity == null)
            return null;

        // Get the field value from the related entity
        return GetNestedValue(relatedEntity, fieldPath);
    }

    /// <summary>
    /// Get a nested value from a JObject using dot notation
    /// </summary>
    private object? GetNestedValue(JObject obj, string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        try
        {
            var token = obj.SelectToken(path);
            if (token == null)
                return null;

            return token.Type switch
            {
                JTokenType.String => token.Value<string>(),
                JTokenType.Integer => token.Value<long>(),
                JTokenType.Float => token.Value<double>(),
                JTokenType.Boolean => token.Value<bool>(),
                JTokenType.Date => token.Value<DateTime>(),
                JTokenType.Null => null,
                _ => token.ToString()
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Build a data cache from multiple entity collections for efficient relationship resolution
    /// </summary>
    public static RelatedEntityFieldResolver BuildCache(Dictionary<string, List<JObject>> entityCollections)
    {
        var resolver = new RelatedEntityFieldResolver();
        foreach (var kvp in entityCollections)
        {
            resolver.AddEntitiesToCache(kvp.Key, kvp.Value);
        }
        return resolver;
    }

    /// <summary>
    /// Get all unique related entity types needed for a set of field mappings
    /// </summary>
    public static HashSet<string> GetRequiredRelatedEntities(string sourceEntityType, List<FieldMapping> mappings)
    {
        var relatedEntities = new HashSet<string>();

        foreach (var mapping in mappings.Where(m => m.IsSelected && m.ApiField.Contains('.')))
        {
            var parts = mapping.ApiField.Split('.', 2);
            var relatedEntityType = parts[0];

            // Verify that this is a valid relationship
            if (ModelRelationshipConfiguration.HasRelationship(sourceEntityType, relatedEntityType))
            {
                relatedEntities.Add(relatedEntityType);
            }
        }

        return relatedEntities;
    }

    /// <summary>
    /// Get all unique related entity types needed for multiple sheet configurations
    /// </summary>
    public static Dictionary<string, HashSet<string>> GetRequiredRelatedEntitiesForSheets(List<SheetConfiguration> sheets)
    {
        var result = new Dictionary<string, HashSet<string>>();

        foreach (var sheet in sheets)
        {
            var relatedEntities = GetRequiredRelatedEntities(sheet.EntityType, sheet.Mappings);
            result[sheet.EntityType] = relatedEntities;
        }

        return result;
    }
}
