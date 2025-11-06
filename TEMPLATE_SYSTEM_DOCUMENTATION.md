# Export Template System - Complete Documentation

## Overview

The Template System allows users to save their export configurations (field mappings, sheet configurations, etc.) and reuse them for future exports. This eliminates the need to manually reconfigure exports every time and ensures consistency across repeated export operations.

---

## 🎯 Key Features

### For End Users
- **Save Configurations**: Save any single-sheet or multi-sheet export configuration as a named template
- **Quick Load**: Load saved templates with one click
- **One-Click Export**: Export directly from a saved template without manual configuration
- **Template Management**: View, search, duplicate, and delete templates
- **Preserved Settings**: All field selections, column names, and sheet configurations are saved

### For Administrators
- **File-Based Storage**: Templates stored as JSON files (no database required)
- **Easy Backup**: Simply copy the Data/ExportTemplates directory
- **Template Sharing**: Share templates by copying JSON files between environments
- **Version Control**: Template files can be committed to source control if needed

---

## 📋 Architecture

### Data Models

#### ExportTemplate
```csharp
public class ExportTemplate
{
    public string Id { get; set; }                    // Unique identifier (GUID)
    public string Name { get; set; }                  // User-friendly name
    public string Description { get; set; }           // Optional description
    public ExportTemplateType Type { get; set; }      // SingleSheet or MultiSheet
    public DateTime CreatedAt { get; set; }           // Creation timestamp
    public DateTime UpdatedAt { get; set; }           // Last update timestamp
    public string CreatedBy { get; set; }             // User who created it

    // Configuration data
    public ExportRequest? SingleSheetConfig { get; set; }
    public MultiSheetExportRequest? MultiSheetConfig { get; set; }
}
```

#### ExportTemplateType
```csharp
public enum ExportTemplateType
{
    SingleSheet = 0,  // Single entity type export
    MultiSheet = 1    // Multiple sheets export
}
```

### Storage Service

**ExportTemplateService** (`HuvrWebApp/Services/ExportTemplateService.cs`)

**Features:**
- File-based JSON storage in `Data/ExportTemplates/templates.json`
- In-memory caching using `ConcurrentDictionary` for fast access
- Thread-safe operations with `SemaphoreSlim` for file writes
- Automatic directory creation on startup
- CRUD operations: Create, Read, Update, Delete, Search, Duplicate

**Storage Location:**
```
HuvrWebApp/
  Data/
    ExportTemplates/
      templates.json    <- All templates stored here
```

---

## 🔧 API Endpoints

### Template Controller (`/Template`)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/Template/GetAllTemplates` | GET | Get all templates with summary information |
| `/Template/GetTemplate?id={id}` | GET | Get a specific template by ID |
| `/Template/GetTemplatesByType?type={type}` | GET | Get templates filtered by type (0=Single, 1=Multi) |
| `/Template/SaveTemplate` | POST | Save a new template |
| `/Template/UpdateTemplate` | POST | Update an existing template |
| `/Template/DeleteTemplate?id={id}` | POST | Delete a template |
| `/Template/DuplicateTemplate?id={id}` | POST | Create a copy of a template |
| `/Template/SearchTemplates?searchTerm={term}` | GET | Search templates by name/description |

### Excel Controller (`/Excel`)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/Excel/ExportFromTemplate?templateId={id}` | POST | Generate Excel file directly from template |

---

## 💻 User Interface

### 1. Single-Sheet Export (`/Excel/FieldMapping`)

**New Elements:**
- **Template Selector Dropdown**: Load saved single-sheet templates
- **Save as Template Button**: Opens modal to save current configuration
- **Manage Templates Link**: Navigate to template management page

**Workflow:**
1. Select entity type and configure fields
2. Click "Save as Template"
3. Enter template name and description
4. Template is saved and appears in selector
5. Load template anytime from dropdown

### 2. Multi-Sheet Export (`/Excel/MultiSheetMapping`)

**New Elements:**
- **Template Selector Dropdown**: Load saved multi-sheet templates
- **Save as Template Button**: Opens modal to save current configuration
- **Manage Templates Link**: Navigate to template management page

**Workflow:**
1. Configure multiple sheets with fields
2. Click "Save as Template"
3. Enter template name and description
4. All sheets and configurations are saved
5. Load template to recreate entire multi-sheet setup

### 3. Template Management Page (`/Template/Index`)

**Features:**
- **Card-Based View**: Each template displayed as a card
- **Template Information**:
  - Name and description
  - Type (Single/Multi-Sheet with sheet count)
  - Field count
  - Entity types
  - Last updated timestamp
- **Actions Per Template**:
  - **Export Now**: Generate Excel immediately
  - **Duplicate**: Create a copy
  - **Delete**: Remove template (with confirmation)
- **Search Box**: Filter templates by name, description, or entity type
- **Refresh Button**: Reload templates from storage

---

## 📝 Usage Examples

### Example 1: Save a Single-Sheet Template

```
User Action Flow:
1. Navigate to /Excel/FieldMapping
2. Select entity type "Projects"
3. Load fields including related fields (Asset.Name, Asset.Location)
4. Select desired fields and customize column names
5. Click "Save as Template"
6. Enter:
   - Name: "Project Export with Assets"
   - Description: "Standard project export including asset details"
7. Click "Save Template"
8. Template is now available in dropdown

Template Saved As:
{
  "id": "abc123...",
  "name": "Project Export with Assets",
  "description": "Standard project export including asset details",
  "type": 0,
  "singleSheetConfig": {
    "entityType": "projects",
    "mappings": [
      { "apiField": "Id", "excelColumn": "Project ID", "isSelected": true },
      { "apiField": "Name", "excelColumn": "Project Name", "isSelected": true },
      { "apiField": "Asset.Name", "excelColumn": "Asset", "isSelected": true },
      ...
    ]
  }
}
```

### Example 2: Save a Multi-Sheet Template

```
User Action Flow:
1. Navigate to /Excel/MultiSheetMapping
2. Add Sheet 1: Projects with Asset fields
3. Add Sheet 2: Defects with Project and User fields
4. Add Sheet 3: Measurements with Asset fields
5. Click "Save as Template"
6. Enter:
   - Name: "Comprehensive Project Report"
   - Description: "Complete report with projects, defects, and measurements"
7. Click "Save Template"

Template Saved As:
{
  "id": "def456...",
  "name": "Comprehensive Project Report",
  "description": "Complete report with projects, defects, and measurements",
  "type": 1,
  "multiSheetConfig": {
    "sheets": [
      {
        "sheetName": "Projects",
        "entityType": "projects",
        "startRow": 1,
        "mappings": [...]
      },
      {
        "sheetName": "Defects",
        "entityType": "defects",
        "startRow": 1,
        "mappings": [...]
      },
      {
        "sheetName": "Measurements",
        "entityType": "measurements",
        "startRow": 1,
        "mappings": [...]
      }
    ],
    "linkRelatedData": true
  }
}
```

### Example 3: Export Directly from Template

```
User Action Flow:
1. Navigate to /Template/Index
2. Find template "Comprehensive Project Report"
3. Click "Export Now"
4. Excel file is generated and downloaded immediately
5. No manual configuration needed
```

### Example 4: Duplicate and Modify Template

```
User Action Flow:
1. Navigate to /Template/Index
2. Find template "Project Export with Assets"
3. Click "Duplicate"
4. Template "Project Export with Assets (Copy)" is created
5. Load the copy in /Excel/FieldMapping
6. Modify fields (e.g., add more Asset fields)
7. Save as new template with different name
```

---

## 🔄 Template Lifecycle

```
┌─────────────┐
│   Create    │
│  Configure  │ → User sets up export on FieldMapping or MultiSheetMapping page
│   Export    │
└──────┬──────┘
       │
       v
┌──────────────┐
│ Save Template│ → User clicks "Save as Template" and provides name/description
│  (Optional)  │
└──────┬───────┘
       │
       v
┌──────────────┐
│   Stored     │ → Template saved to JSON file and cached in memory
│ templates.   │
│   json       │
└──────┬───────┘
       │
       ├─→ Load Later → User selects template from dropdown
       │                 ↓
       │              Fields/sheets are configured automatically
       │
       ├─→ Export Now → Direct export from Template Management page
       │                 ↓
       │              Excel file generated immediately
       │
       ├─→ Duplicate  → Creates copy for modification
       │
       └─→ Delete     → Removes from storage (with confirmation)
```

---

## 🗂️ Template JSON Structure

### Single-Sheet Template
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Project Export with Assets",
  "description": "Standard project export including asset details",
  "type": 0,
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-15T10:30:00Z",
  "createdBy": "session-abc123",
  "singleSheetConfig": {
    "entityType": "projects",
    "mappings": [
      {
        "apiField": "Id",
        "excelColumn": "Project ID",
        "isSelected": true
      },
      {
        "apiField": "Name",
        "excelColumn": "Project Name",
        "isSelected": true
      },
      {
        "apiField": "Asset.Name",
        "excelColumn": "Asset Name",
        "isSelected": true
      },
      {
        "apiField": "Asset.Location",
        "excelColumn": "Asset Location",
        "isSelected": true
      }
    ]
  },
  "multiSheetConfig": null
}
```

### Multi-Sheet Template
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "name": "Comprehensive Project Report",
  "description": "Complete report with projects, defects, and measurements",
  "type": 1,
  "createdAt": "2025-01-15T11:00:00Z",
  "updatedAt": "2025-01-15T11:00:00Z",
  "createdBy": "session-xyz789",
  "singleSheetConfig": null,
  "multiSheetConfig": {
    "sheets": [
      {
        "sheetName": "Projects Summary",
        "entityType": "projects",
        "startRow": 1,
        "filterByParentId": null,
        "filterByParentType": null,
        "mappings": [
          {
            "apiField": "Id",
            "excelColumn": "Project ID",
            "isSelected": true
          },
          {
            "apiField": "Asset.Name",
            "excelColumn": "Asset",
            "isSelected": true
          }
        ]
      },
      {
        "sheetName": "Defects Report",
        "entityType": "defects",
        "startRow": 3,
        "filterByParentId": null,
        "filterByParentType": null,
        "mappings": [
          {
            "apiField": "Id",
            "excelColumn": "Defect ID",
            "isSelected": true
          },
          {
            "apiField": "Project.Name",
            "excelColumn": "Project",
            "isSelected": true
          },
          {
            "apiField": "User.Email",
            "excelColumn": "Identified By",
            "isSelected": true
          }
        ]
      }
    ],
    "linkRelatedData": true
  }
}
```

---

## 🔒 Security & Permissions

### Current Implementation
- Templates are stored per application instance (not per user)
- All users with access to the application can view/use all templates
- CreatedBy field tracks the session ID but doesn't enforce permissions

### Future Enhancements
Consider implementing:
1. **User-specific templates**: Filter templates by user
2. **Shared vs Private templates**: Allow users to mark templates as private or shared
3. **Role-based access**: Admin-only templates vs user templates
4. **Template permissions**: View, Edit, Delete permissions per template

---

## 📊 Performance Considerations

### Optimization Strategies Implemented

1. **In-Memory Caching**:
   - All templates loaded into `ConcurrentDictionary` on startup
   - Read operations are O(1) from memory
   - No disk I/O for template retrieval

2. **Thread-Safe Writes**:
   - `SemaphoreSlim` ensures only one write at a time
   - Prevents file corruption from concurrent writes

3. **Efficient JSON Serialization**:
   - Newtonsoft.Json used for fast serialization/deserialization
   - Indented formatting for human readability (can be changed for size)

4. **Lazy Loading**:
   - Template content only loaded when actually needed
   - Summary views use minimal data (ExportTemplateSummary)

### Scalability Considerations

**Current Setup (File-Based)**:
- ✅ Good for: Small to medium deployments (< 1000 templates)
- ✅ Zero database overhead
- ✅ Easy backup and restore
- ⚠️ Not ideal for: Distributed/load-balanced environments (shared storage needed)

**Future Database Migration** (if needed):
- Replace `ExportTemplateService` with database implementation
- Keep same interface/API endpoints
- Add Entity Framework Core
- Use SQL/NoSQL based on requirements

---

## 🛠️ Maintenance & Administration

### Backup Templates
```bash
# Backup all templates
cp -r HuvrWebApp/Data/ExportTemplates /backup/location/

# Restore templates
cp -r /backup/location/ExportTemplates HuvrWebApp/Data/
```

### Share Templates Between Environments
```bash
# Export from Dev
cp HuvrWebApp/Data/ExportTemplates/templates.json /shared/location/

# Import to Prod
cp /shared/location/templates.json HuvrWebApp/Data/ExportTemplates/
# Restart application to reload
```

### Cleanup Old Templates
```csharp
// Future enhancement: Add cleanup API
// DELETE /Template/CleanupOldTemplates?daysOld=90
```

### Template Migration
If data model changes:
1. Create migration script to update JSON structure
2. Load all templates
3. Transform to new format
4. Save back to storage

---

## 🐛 Troubleshooting

### Problem: Templates not loading
**Solution:**
- Check if `Data/ExportTemplates` directory exists
- Verify `templates.json` file is valid JSON
- Check application logs for deserialization errors
- Restart application to clear cache

### Problem: Template save fails
**Solution:**
- Ensure application has write permissions to `Data/ExportTemplates`
- Check disk space availability
- Verify JSON serialization succeeds
- Check for special characters in template name

### Problem: Duplicate template names
**Solution:**
- Currently allowed (IDs are unique)
- Consider adding name uniqueness validation if desired

### Problem: Template not found after save
**Solution:**
- Refresh the template list
- Check if file write succeeded
- Verify template ID is correct

---

## 📚 API Usage Examples

### JavaScript: Save Template
```javascript
var templateData = {
    Name: "My Custom Export",
    Description: "Description here",
    Type: 0, // SingleSheet
    SingleSheetConfig: {
        EntityType: "projects",
        Mappings: [
            { ApiField: "Id", ExcelColumn: "ID", IsSelected: true },
            { ApiField: "Name", ExcelColumn: "Name", IsSelected: true }
        ]
    }
};

$.ajax({
    url: '/Template/SaveTemplate',
    type: 'POST',
    contentType: 'application/json',
    data: JSON.stringify(templateData),
    success: function(response) {
        if (response.success) {
            console.log('Template saved:', response.template.id);
        }
    }
});
```

### JavaScript: Load and Export
```javascript
// Load template
$.get('/Template/GetTemplate', { id: 'template-id-here' }, function(response) {
    if (response.success) {
        var config = response.template.singleSheetConfig;
        // Use config to populate UI
    }
});

// Export from template
$.post('/Excel/ExportFromTemplate', { templateId: 'template-id-here' }, {
    xhrFields: { responseType: 'blob' }
}, function(blob) {
    // Download blob as Excel file
    var url = window.URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = 'export.xlsx';
    a.click();
});
```

### C#: Programmatic Template Creation
```csharp
var templateService = serviceProvider.GetRequiredService<ExportTemplateService>();

var template = await templateService.SaveTemplateAsync(new SaveTemplateRequest
{
    Name = "Automated Report",
    Description = "Generated by scheduled task",
    Type = ExportTemplateType.SingleSheet,
    SingleSheetConfig = new ExportRequest
    {
        EntityType = "projects",
        Mappings = new List<FieldMapping>
        {
            new() { ApiField = "Id", ExcelColumn = "ID", IsSelected = true },
            new() { ApiField = "Name", ExcelColumn = "Name", IsSelected = true }
        }
    }
}, userId: "system");
```

---

## ✨ Future Enhancements

### Priority 1: Essential Features
1. **Template Versioning**: Track template history and allow rollback
2. **Template Validation**: Validate templates before save
3. **Bulk Operations**: Export/import templates in bulk
4. **Template Categories**: Organize templates by category/tag

### Priority 2: Nice to Have
5. **Template Preview**: Preview what the Excel will look like
6. **Scheduled Exports**: Schedule template-based exports
7. **Email Integration**: Email exports directly from templates
8. **Template Marketplace**: Share templates with community
9. **Template Analytics**: Track which templates are most used
10. **Template Permissions**: Fine-grained access control

### Priority 3: Advanced Features
11. **Template Inheritance**: Base templates with overrides
12. **Dynamic Templates**: Templates with runtime parameters
13. **Template Composition**: Combine multiple templates
14. **API Access**: REST API for template management
15. **Webhook Integration**: Trigger exports via webhooks

---

## 📖 Summary

The Export Template System provides a robust, user-friendly way to save and reuse export configurations. With file-based storage, comprehensive CRUD operations, and intuitive UI, users can streamline their export workflows significantly.

**Key Benefits:**
- ⏱️ **Time Savings**: Configure once, reuse many times
- 🎯 **Consistency**: Ensure same fields/formats every time
- 🚀 **Efficiency**: One-click exports from saved templates
- 📦 **Portability**: Easy backup and sharing via JSON files
- 🔧 **Maintainability**: Simple architecture, easy to extend

**Production Ready Features:**
- ✅ Complete CRUD operations
- ✅ Thread-safe storage
- ✅ In-memory caching
- ✅ Comprehensive error handling
- ✅ User-friendly UI
- ✅ Search and filter capabilities
- ✅ Template duplication
- ✅ Direct export from templates

The system is ready for production use and can be enhanced based on user feedback and requirements.
