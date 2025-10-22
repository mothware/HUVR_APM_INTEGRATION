# HUVR API Client for C#

A comprehensive C# client library for interacting with the HUVR Data API v3. This library provides strongly-typed models and async methods for all HUVR API endpoints.

## Features

- **Complete API Coverage**: All HUVR API v3 endpoints are implemented
- **Automatic Token Management**: Handles token expiration and refresh automatically
- **Strongly Typed Models**: Full C# models for all API entities
- **Async/Await Support**: Modern async patterns throughout
- **Easy Media Upload**: Simplified two-part media upload workflow
- **PATCH Support**: Partial updates for all resources
- **Query Parameters**: Flexible filtering and searching

## Installation

Add the HuvrApiClient files to your project:

```bash
HuvrApiClient/
├── HuvrApiClient.cs
├── HuvrApiClientConfig.cs
├── Models/
│   ├── AuthenticationModels.cs
│   ├── Asset.cs
│   ├── Project.cs
│   ├── InspectionMedia.cs
│   ├── Checklist.cs
│   ├── Defect.cs
│   ├── Measurement.cs
│   ├── User.cs
│   └── Workspace.cs
└── Examples/
    └── UsageExamples.cs
```

## Prerequisites

- .NET 6.0 or higher
- HUVR service account credentials (Client ID and Client Secret)

## Quick Start

### Basic Setup

```csharp
using HuvrApiClient;
using HuvrApiClient.Models;

// Initialize the client
var client = new HuvrApiClient(
    clientId: "[email protected]",
    clientSecret: "your-client-secret"
);

// Use the client (token is obtained automatically)
var assets = await client.ListAssetsAsync();
Console.WriteLine($"Found {assets.Count} assets");
```

## API Reference

### Authentication

The client automatically handles authentication and token refresh. Tokens are valid for 60 minutes and will be refreshed automatically 5 minutes before expiration.

```csharp
// Manual token acquisition (optional - done automatically)
var token = await client.ObtainAccessTokenAsync();
Console.WriteLine($"Token expires at: {token.ExpiresAt}");
```

### Assets

```csharp
// List assets
var assets = await client.ListAssetsAsync();

// List with filters
var filteredAssets = await client.ListAssetsAsync(new Dictionary<string, string>
{
    { "status", "Active" },
    { "location", "Building A" }
});

// Get asset by ID
var asset = await client.GetAssetAsync("asset-id");

// Create asset
var newAsset = await client.CreateAssetAsync(new AssetRequest
{
    Name = "Pump #123",
    Description = "Main cooling pump",
    AssetType = "Pump",
    Location = "Building A, Floor 2"
});

// Update asset (partial)
var updated = await client.UpdateAssetAsync(
    "asset-id",
    new { Status = "Maintenance" }
);

// Delete asset
await client.DeleteAssetAsync("asset-id");
```

### Projects (Work Orders)

```csharp
// List projects
var projects = await client.ListProjectsAsync();

// Filter by asset
var assetProjects = await client.ListProjectsAsync(new Dictionary<string, string>
{
    { "asset_search", "pump-123" }
});

// Get project
var project = await client.GetProjectAsync("project-id");

// Create project
var newProject = await client.CreateProjectAsync(new ProjectRequest
{
    Name = "Quarterly Inspection Q1 2025",
    AssetId = "asset-id",
    ProjectTypeId = "inspection-type-id",
    Status = "In Progress"
});

// Update project
await client.UpdateProjectAsync(
    "project-id",
    new { Status = "Completed", EndDate = DateTime.UtcNow }
);

// Delete project
await client.DeleteProjectAsync("project-id");
```

### Inspection Media

#### Simple Upload (Recommended)

```csharp
var fileBytes = await File.ReadAllBytesAsync("photo.jpg");

var media = await client.UploadInspectionMediaAsync(
    new InspectionMediaRequest
    {
        ProjectId = "project-id",
        FileName = "photo.jpg",
        FileType = "image/jpeg",
        FileSize = fileBytes.Length,
        Metadata = new Dictionary<string, object>
        {
            { "camera", "DJI Mavic 3" }
        }
    },
    fileBytes,
    "image/jpeg"
);
```

#### Manual Two-Part Upload

```csharp
// Part 1: Create media object
var media = await client.CreateInspectionMediaAsync(new InspectionMediaRequest
{
    ProjectId = "project-id",
    FileName = "photo.jpg",
    FileType = "image/jpeg",
    FileSize = fileBytes.Length
});

// Part 2: Upload file (must be done within 15 minutes)
await client.UploadInspectionMediaFileAsync(
    media.UploadUrl!,
    fileBytes,
    "image/jpeg"
);
```

#### Handle Expired Upload URL

```csharp
try
{
    await client.UploadInspectionMediaFileAsync(uploadUrl, fileBytes, "image/jpeg");
}
catch (HttpRequestException)
{
    // Get fresh upload URL
    media = await client.GetInspectionMediaAsync(mediaId);
    await client.UploadInspectionMediaFileAsync(media.UploadUrl!, fileBytes, "image/jpeg");
}
```

#### Update Media Metadata

```csharp
await client.UpdateInspectionMediaMetadataAsync(
    "media-id",
    new { Metadata = new { Notes = "Crack detected" } }
);
```

### Checklists

```csharp
// List checklists
var checklists = await client.ListChecklistsAsync();

// Get checklist
var checklist = await client.GetChecklistAsync("checklist-id");

// Create checklist
var newChecklist = await client.CreateChecklistAsync(new ChecklistRequest
{
    ProjectId = "project-id",
    Name = "Safety Inspection",
    TemplateId = "template-id",
    Responses = new Dictionary<string, object>
    {
        { "question1", "Yes" },
        { "question2", "No" }
    }
});

// Update checklist
await client.UpdateChecklistAsync(
    "checklist-id",
    new { Status = "Completed" }
);
```

### Defects (Findings)

```csharp
// List defects
var defects = await client.ListDefectsAsync();

// Filter by severity
var criticalDefects = await client.ListDefectsAsync(new Dictionary<string, string>
{
    { "severity", "Critical" }
});

// Create defect
var defect = await client.CreateDefectAsync(new DefectRequest
{
    ProjectId = "project-id",
    AssetId = "asset-id",
    Title = "Crack in foundation",
    Description = "15cm crack detected",
    Severity = "High",
    Status = "Open",
    DefectType = "Structural"
});

// Update defect
await client.UpdateDefectAsync(
    "defect-id",
    new { Status = "In Repair" }
);
```

### Measurements

```csharp
// List measurements
var measurements = await client.ListMeasurementsAsync();

// Create measurement
var measurement = await client.CreateMeasurementAsync(new MeasurementRequest
{
    ProjectId = "project-id",
    AssetId = "asset-id",
    MeasurementType = "Ultrasonic Thickness",
    Value = 12.5m,
    Unit = "mm",
    Location = "Point A1"
});

// Update measurement
await client.UpdateMeasurementAsync(
    "measurement-id",
    new { Value = 12.6m }
);
```

### Users

```csharp
// List users
var users = await client.ListUsersAsync();

// Get user
var user = await client.GetUserAsync("user-id");
```

### Workspaces

```csharp
// List workspaces
var workspaces = await client.ListWorkspacesAsync();

// Get workspace
var workspace = await client.GetWorkspaceAsync("workspace-id");
```

## Complete Inspection Workflow Example

```csharp
using var client = new HuvrApiClient("your-client-id", "your-client-secret");

// 1. Find asset
var assets = await client.ListAssetsAsync(new Dictionary<string, string>
{
    { "name", "Pump #123" }
});
var asset = assets.Results.First();

// 2. Create project
var project = await client.CreateProjectAsync(new ProjectRequest
{
    Name = $"Monthly Inspection - {DateTime.Now:yyyy-MM}",
    AssetId = asset.Id!,
    ProjectTypeId = "inspection-type-id",
    Status = "In Progress"
});

// 3. Upload photos
foreach (var photoPath in Directory.GetFiles("./photos", "*.jpg"))
{
    var bytes = await File.ReadAllBytesAsync(photoPath);
    await client.UploadInspectionMediaAsync(
        new InspectionMediaRequest
        {
            ProjectId = project.Id!,
            FileName = Path.GetFileName(photoPath),
            FileType = "image/jpeg",
            FileSize = bytes.Length
        },
        bytes,
        "image/jpeg"
    );
}

// 4. Complete checklist
await client.CreateChecklistAsync(new ChecklistRequest
{
    ProjectId = project.Id!,
    Name = "Standard Inspection",
    Responses = new Dictionary<string, object>
    {
        { "visual_inspection", "Pass" },
        { "leak_check", "Pass" }
    }
});

// 5. Record defects
await client.CreateDefectAsync(new DefectRequest
{
    ProjectId = project.Id!,
    AssetId = asset.Id!,
    Title = "Minor corrosion",
    Severity = "Low",
    Status = "Open"
});

// 6. Record measurements
await client.CreateMeasurementAsync(new MeasurementRequest
{
    ProjectId = project.Id!,
    AssetId = asset.Id!,
    MeasurementType = "Vibration",
    Value = 2.5m,
    Unit = "mm/s"
});

// 7. Complete project
await client.UpdateProjectAsync(
    project.Id!,
    new { Status = "Completed", EndDate = DateTime.UtcNow }
);
```

## Best Practices

### 1. Token Management
- Tokens are automatically refreshed 5 minutes before expiration
- No manual token management needed
- The client is thread-safe for token refresh

### 2. Resource Disposal
```csharp
// Use 'using' statement for proper disposal
using var client = new HuvrApiClient(clientId, clientSecret);
// ... use client
// Automatically disposed when out of scope
```

### 3. Error Handling
```csharp
try
{
    var asset = await client.GetAssetAsync("asset-id");
}
catch (HttpRequestException ex)
{
    // Handle API errors (404, 401, 500, etc.)
    Console.WriteLine($"API Error: {ex.Message}");
}
```

### 4. Query Parameters
```csharp
// Use query parameters for filtering
var filtered = await client.ListAssetsAsync(new Dictionary<string, string>
{
    { "status", "Active" },
    { "asset_type", "Pump" },
    { "limit", "100" }
});
```

### 5. Partial Updates
```csharp
// Only update the fields you need
await client.UpdateAssetAsync(
    "asset-id",
    new { Status = "Maintenance" }  // Only updates Status field
);
```

## Configuration

### Custom HTTP Client
```csharp
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(60)
};

var client = new HuvrApiClient("client-id", "client-secret", httpClient);
```

## API Documentation

For complete API documentation, see:
- **API Reference**: https://docs.huvrdata.app/reference
- **OpenAPI Spec**: https://docs.huvrdata.app/openapi/
- **HUVR API Definition**: See `HUVR_API_DEFINITION.md` in this repository

## Support

For API access and credentials:
- Contact your HUVR Customer Success representative
- Request service account credentials from your support contact

## License

This client library is provided as-is for use with the HUVR Data API.

## Version

- **HUVR API Version**: v3
- **Client Version**: 1.0.0
- **Last Updated**: 2025-10-21
