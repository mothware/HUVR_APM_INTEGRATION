# Multi-Sheet Excel Export with Model Linking - Implementation Guide

## Overview

This implementation adds powerful new capabilities to the HUVR APM Integration system:
1. **Model Linking** based on `<model>Id` fields (foreign key relationships)
2. **Multi-sheet Excel exports** with configurable starting rows
3. **Related entity field access** - access fields from linked models (e.g., `Asset.Name` from a Project)

## Architecture

### 1. Model Relationship Configuration (`/Models/ModelRelationshipConfiguration.cs`)

Defines all relationships between models based on their ID fields:

```csharp
// Example relationships
Project → Asset (via AssetId)
Project → Defect (one-to-many via ProjectId)
Defect → User (via IdentifiedBy)
Asset → Library (via LibraryId)
```

**Key Features:**
- Centralized relationship definitions
- Support for one-to-one and one-to-many relationships
- Automatic field discovery for related entities
- Metadata including relationship descriptions

**Usage:**
```csharp
// Get all relationships for a model
var relationships = ModelRelationshipConfiguration.GetRelationships("Project");

// Check if two models are related
bool hasRelation = ModelRelationshipConfiguration.HasRelationship("Project", "Asset");

// Get available fields including related entities
var fields = ModelRelationshipConfiguration.GetAvailableFieldsWithRelationships("Project");
```

### 2. Related Entity Field Resolver (`/Services/RelatedEntityFieldResolver.cs`)

Resolves field values across related entities using an efficient caching mechanism:

**Key Features:**
- In-memory entity cache for fast lookups
- Support for nested field paths (e.g., `Asset.Location`)
- Automatic foreign key resolution
- Type-safe value extraction from JObjects

**Usage:**
```csharp
// Build resolver with entity data
var resolver = RelatedEntityFieldResolver.BuildCache(entityCollections);

// Resolve a related field
var value = resolver.ResolveFieldValue(projectObject, "Asset.Name", "Project");
```

### 3. Multi-Sheet Export Models (`/Models/FieldMappingViewModel.cs`)

#### SheetConfiguration
Defines configuration for a single sheet:
```csharp
public class SheetConfiguration
{
    public string SheetName { get; set; }           // Custom sheet name
    public string EntityType { get; set; }          // Entity type to export
    public List<FieldMapping> Mappings { get; set; } // Field mappings
    public int StartRow { get; set; } = 1;          // Starting row for data
    public string? FilterByParentId { get; set; }   // Optional filtering
    public string? FilterByParentType { get; set; } // Optional filtering
}
```

#### MultiSheetExportRequest
Container for multiple sheet configurations:
```csharp
public class MultiSheetExportRequest
{
    public List<SheetConfiguration> Sheets { get; set; }
    public bool LinkRelatedData { get; set; } = true; // Enable/disable linking
}
```

### 4. Excel Controller Enhancements (`/Controllers/ExcelController.cs`)

#### New Endpoints

**GET: `/Excel/GetAvailableFieldsWithRelationships`**
- Returns fields including related entity fields
- Example response includes fields like `Id`, `Name`, `Asset.Name`, `Asset.Location`

**POST: `/Excel/ExportToExcelMultiSheet`**
- Accepts `MultiSheetExportRequest`
- Generates multi-sheet Excel workbook
- Resolves related entity fields automatically
- Supports custom start rows per sheet

**GET: `/Excel/MultiSheetMapping`**
- Serves the new multi-sheet mapping UI

#### Key Implementation Details

1. **Efficient Data Fetching**: Fetches all required data upfront to minimize API calls
2. **Relationship Resolution**: Uses `RelatedEntityFieldResolver` for linked data
3. **Filtering Support**: Can filter child entities by parent ID
4. **Error Handling**: Comprehensive error handling with user-friendly messages

### 5. User Interface (`/Views/Excel/MultiSheetMapping.cshtml`)

**Features:**
- Dynamic sheet addition/removal
- Per-sheet configuration (name, entity type, start row)
- Visual distinction between direct and related fields
- Real-time field loading with relationship information
- Batch export of all configured sheets

**UI Components:**
- Sheet configuration cards (collapsible)
- Entity type selector
- Field mapping tables with checkboxes
- Related field badges (shows source entity)
- Start row input (numeric, min 1)
- Export button (generates multi-sheet Excel)

## Usage Examples

### Example 1: Projects with Related Asset Data

```json
{
  "Sheets": [
    {
      "SheetName": "Projects with Assets",
      "EntityType": "projects",
      "StartRow": 1,
      "Mappings": [
        { "ApiField": "Id", "ExcelColumn": "Project ID", "IsSelected": true },
        { "ApiField": "Name", "ExcelColumn": "Project Name", "IsSelected": true },
        { "ApiField": "Asset.Name", "ExcelColumn": "Asset Name", "IsSelected": true },
        { "ApiField": "Asset.Location", "ExcelColumn": "Asset Location", "IsSelected": true }
      ]
    }
  ],
  "LinkRelatedData": true
}
```

**Result**: Single sheet with project data plus related asset information

### Example 2: Multi-Sheet Export with Projects and Defects

```json
{
  "Sheets": [
    {
      "SheetName": "All Projects",
      "EntityType": "projects",
      "StartRow": 1,
      "Mappings": [
        { "ApiField": "Id", "ExcelColumn": "ID", "IsSelected": true },
        { "ApiField": "Name", "ExcelColumn": "Name", "IsSelected": true },
        { "ApiField": "Asset.Name", "ExcelColumn": "Asset", "IsSelected": true }
      ]
    },
    {
      "SheetName": "All Defects",
      "EntityType": "defects",
      "StartRow": 3,
      "Mappings": [
        { "ApiField": "Id", "ExcelColumn": "ID", "IsSelected": true },
        { "ApiField": "Title", "ExcelColumn": "Title", "IsSelected": true },
        { "ApiField": "Project.Name", "ExcelColumn": "Project", "IsSelected": true },
        { "ApiField": "Asset.Name", "ExcelColumn": "Asset", "IsSelected": true },
        { "ApiField": "User.Email", "ExcelColumn": "Identified By", "IsSelected": true }
      ]
    }
  ],
  "LinkRelatedData": true
}
```

**Result**: Two-sheet workbook:
- Sheet 1: Projects starting at row 1
- Sheet 2: Defects starting at row 3 (allows for header rows)

### Example 3: Custom Start Rows for Template Compliance

```json
{
  "Sheets": [
    {
      "SheetName": "Inspection Data",
      "EntityType": "measurements",
      "StartRow": 5,
      "Mappings": [
        { "ApiField": "MeasurementType", "ExcelColumn": "Type", "IsSelected": true },
        { "ApiField": "Value", "ExcelColumn": "Value", "IsSelected": true },
        { "ApiField": "Asset.Name", "ExcelColumn": "Asset", "IsSelected": true },
        { "ApiField": "Project.Name", "ExcelColumn": "Project", "IsSelected": true }
      ]
    }
  ],
  "LinkRelatedData": true
}
```

**Result**: Data starts at row 5, leaving rows 1-4 for company logos, titles, etc.

## Supported Relationships

| Source Entity | Related Entity | Foreign Key | Relationship Type | Example Field Access |
|---------------|----------------|-------------|-------------------|---------------------|
| Project | Asset | AssetId | One-to-One | `Asset.Name`, `Asset.Location` |
| Project | Defect | Id → ProjectId | One-to-Many | N/A (use Defect sheet) |
| Defect | Project | ProjectId | One-to-One | `Project.Name`, `Project.Status` |
| Defect | Asset | AssetId | One-to-One | `Asset.Name` |
| Defect | User | IdentifiedBy | One-to-One | `User.Email`, `User.FirstName` |
| Asset | Library | LibraryId | One-to-One | `Library.Name` |
| Measurement | Project | ProjectId | One-to-One | `Project.Name` |
| Measurement | Asset | AssetId | One-to-One | `Asset.Name` |
| DefectOverlay | Defect | DefectId | One-to-One | `Defect.Title` |
| DefectOverlay | User | CreatedBy | One-to-One | `User.Email` |

## API Endpoints

### GET `/Excel/MultiSheetMapping`
Serves the multi-sheet mapping UI

### GET `/Excel/GetAvailableFieldsWithRelationships?entityType={type}`
Returns available fields including related entities

**Response:**
```json
{
  "success": true,
  "fields": [
    {
      "fieldPath": "Id",
      "displayName": "Id",
      "entityType": "Project",
      "isRelated": false
    },
    {
      "fieldPath": "Asset.Name",
      "displayName": "Asset.Name",
      "entityType": "Project",
      "isRelated": true,
      "relatedEntity": "Asset",
      "description": "Parent asset information"
    }
  ]
}
```

### POST `/Excel/ExportToExcelMultiSheet`
Generates multi-sheet Excel workbook

**Request Body:** `MultiSheetExportRequest` (see above)

**Response:** Excel file (XLSX) as binary stream

## Performance Considerations

1. **Caching Strategy**: All required entity data is fetched once and cached in memory
2. **Efficient Lookups**: Uses Dictionary-based lookups for O(1) entity resolution
3. **Lazy Loading**: Only fetches entity types that are actually needed
4. **Parallel Fetching**: API calls can be made in parallel for different entity types

## Error Handling

The implementation includes comprehensive error handling:
- Invalid entity types
- Missing relationships
- API failures
- Empty datasets
- Invalid field paths

All errors are returned as JSON with descriptive messages:
```json
{
  "success": false,
  "error": "Descriptive error message"
}
```

## Testing Recommendations

1. **Unit Tests**:
   - Test relationship lookups
   - Test field resolution with valid/invalid paths
   - Test entity caching mechanism

2. **Integration Tests**:
   - Test multi-sheet export with sample data
   - Test relationship resolution across entities
   - Test start row configuration
   - Test filtering by parent ID

3. **UI Tests**:
   - Test sheet addition/removal
   - Test field loading
   - Test export functionality
   - Test error handling

## Future Enhancements

Potential improvements for future iterations:

1. **Advanced Filtering**: Filter child entities per sheet (e.g., only Defects with Severity = "High")
2. **Sorting Options**: Specify sort order per sheet
3. **Aggregations**: Support for calculated fields and aggregations
4. **Templates**: Save/load sheet configurations as templates
5. **Scheduled Exports**: Automated periodic exports
6. **Email Delivery**: Email exports to specified recipients
7. **More Relationship Types**: Support for many-to-many relationships
8. **Nested Relationships**: Support for chained relationships (e.g., `Project.Asset.Library.Name`)

## Files Modified/Created

### Created
- `/HuvrWebApp/Models/ModelRelationshipConfiguration.cs`
- `/HuvrWebApp/Services/RelatedEntityFieldResolver.cs`
- `/HuvrWebApp/Views/Excel/MultiSheetMapping.cshtml`
- `/MULTI_SHEET_EXPORT_IMPLEMENTATION.md` (this file)

### Modified
- `/HuvrWebApp/Models/FieldMappingViewModel.cs` - Added `SheetConfiguration` and `MultiSheetExportRequest`
- `/HuvrWebApp/Controllers/ExcelController.cs` - Added multi-sheet export endpoints

## Conclusion

This implementation provides a robust, extensible foundation for multi-sheet Excel exports with model linking capabilities. The architecture is designed for maintainability and future enhancements while providing excellent performance for typical use cases.
