# CLAUDE.md - AI Assistant Guide for HUVR APM Integration

**Last Updated:** 2026-01-06
**Target Framework:** .NET 9.0
**Primary Language:** C#

## Table of Contents

1. [Project Overview](#project-overview)
2. [Repository Structure](#repository-structure)
3. [Technology Stack](#technology-stack)
4. [Architecture & Design Patterns](#architecture--design-patterns)
5. [Development Workflows](#development-workflows)
6. [Code Conventions & Standards](#code-conventions--standards)
7. [Key Components](#key-components)
8. [API Integration Patterns](#api-integration-patterns)
9. [Data Models & Relationships](#data-models--relationships)
10. [Common Tasks & Examples](#common-tasks--examples)
11. [Testing Strategy](#testing-strategy)
12. [Deployment & Configuration](#deployment--configuration)
13. [Troubleshooting Guide](#troubleshooting-guide)
14. [Important Constraints](#important-constraints)

---

## Project Overview

### Purpose
HUVR APM Integration is a comprehensive solution for integrating **HUVR Data API** (inspection and asset management) with **GE APM On-Premises** systems. The project provides:

- **REST API Clients** for both HUVR and GE APM
- **Web Application** for data visualization and Excel export
- **Multi-sheet Excel exports** with model relationship linking
- **Template system** for reusable export configurations
- **Image download** capabilities for inspection media

### Primary Use Cases
1. Export inspection data (projects, defects, measurements) to Excel
2. Download inspection media (photos, overlays) as ZIP archives
3. Integrate HUVR inspection data with GE APM TML (Thickness Measurement Location) systems
4. Create custom data exports with field mapping and relationship resolution

### Business Context
Used by asset integrity teams to:
- Extract inspection findings from HUVR mobile app
- Generate compliance reports for asset management
- Integrate field inspection data with enterprise APM systems
- Create standardized Excel reports for stakeholders

---

## Repository Structure

```
HUVR_APM_INTEGRATION/
├── .claude/                          # Claude Code configuration
├── GeApmClient/                      # GE APM On-Premises API Client Library
│   ├── GeApmClient.cs               # Main client (658 lines)
│   ├── Models/                       # Entity models (4 files)
│   │   ├── AuthenticationModels.cs
│   │   ├── EntityModels.cs
│   │   ├── QueryModels.cs
│   │   └── TmlModels.cs
│   ├── Examples/                     # Usage examples
│   └── GeApmClient.csproj
│
├── HuvrApiClient/                    # HUVR Data API v3 Client Library
│   ├── HuvrApiClient.cs             # Main client (1322 lines)
│   ├── HuvrApiClientConfig.cs       # Configuration
│   ├── HuvrDataGatherer.cs          # Data aggregation utilities
│   ├── Models/                       # Entity models (15 files)
│   │   ├── Asset.cs
│   │   ├── Project.cs
│   │   ├── Defect.cs
│   │   ├── Measurement.cs
│   │   ├── InspectionMedia.cs
│   │   ├── Checklist.cs
│   │   ├── User.cs
│   │   ├── Workspace.cs
│   │   ├── Library.cs
│   │   ├── DefectOverlay.cs
│   │   ├── Task.cs
│   │   ├── AssetType.cs
│   │   ├── QueryParameters.cs
│   │   ├── AuthenticationModels.cs
│   │   └── ErrorModels.cs
│   ├── JsonConverters/
│   │   └── FlexibleIdConverter.cs   # Handles string/numeric IDs
│   ├── Examples/
│   └── HuvrApiClient.csproj
│
├── HuvrWebApp/                       # ASP.NET Core MVC Web Application
│   ├── Controllers/                  # MVC Controllers (5 files)
│   │   ├── AuthController.cs        # Login/logout (81 lines)
│   │   ├── DashboardController.cs   # API exploration (210 lines)
│   │   ├── ExcelController.cs       # Excel export (473 lines)
│   │   ├── ImageController.cs       # Image downloads (195 lines)
│   │   └── TemplateController.cs    # Template management (198 lines)
│   ├── Models/                       # View models and DTOs (4 files)
│   │   ├── LoginViewModel.cs
│   │   ├── FieldMappingViewModel.cs
│   │   ├── ModelRelationshipConfiguration.cs
│   │   └── ExportTemplateModels.cs
│   ├── Services/                     # Business logic (5 files)
│   │   ├── IHuvrService.cs
│   │   ├── HuvrService.cs           # Session-based client management
│   │   ├── ExportTemplateService.cs # Template CRUD
│   │   ├── ImageDownloadService.cs  # Image/ZIP handling
│   │   └── RelatedEntityFieldResolver.cs # Relationship resolution
│   ├── Views/                        # Razor views
│   │   ├── Auth/
│   │   ├── Dashboard/
│   │   ├── Excel/
│   │   ├── Template/
│   │   └── Shared/
│   ├── wwwroot/                      # Static files
│   │   └── css/
│   ├── Data/                         # File-based storage
│   │   └── ExportTemplates/
│   │       └── templates.json       # Saved export templates
│   ├── Program.cs                    # Application entry point (40 lines)
│   ├── appsettings.json             # Configuration
│   └── HuvrWebApp.csproj
│
├── Documentation/                    # Markdown documentation
│   ├── HUVR_API_DEFINITION.md
│   ├── HUVR_API_ENDPOINTS_REFERENCE.md
│   ├── GE_APM_API_SPECIFICATION.md
│   ├── MULTI_SHEET_EXPORT_IMPLEMENTATION.md
│   ├── TEMPLATE_SYSTEM_DOCUMENTATION.md
│   ├── IMAGE_DOWNLOAD_DOCUMENTATION.md
│   └── FRONTEND_ENHANCEMENT_SUMMARY.md
│
└── HuvrWebApp.sln                    # Solution file
```

### Project References
- **HuvrWebApp** references **HuvrApiClient**
- **GeApmClient** is standalone (not currently in solution)
- No circular dependencies

---

## Technology Stack

### Core Frameworks
- **.NET 9.0** (latest version as of project creation)
- **C# 13** with latest language features
- **ASP.NET Core MVC 9.0** for web application
- **Nullable reference types** enabled project-wide
- **Implicit usings** enabled

### Web Application Dependencies
```xml
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="ClosedXML" Version="0.102.1" />
<PackageReference Include="DocumentFormat.OpenXml" Version="2.20.0" />
```

### API Client Dependencies
```xml
<!-- HuvrApiClient -->
<PackageReference Include="System.Net.Http.Json" Version="9.0.10" />

<!-- GeApmClient -->
<PackageReference Include="System.Net.Http.Json" Version="8.0.0" />
```

### Frontend Stack
- **Bootstrap 5.3.0** (CDN-based, no npm)
- **jQuery 3.6.0** (CDN-based)
- **Bootstrap Icons** for UI elements

### Infrastructure
- **File-based storage** for templates (no database)
- **Session-based authentication** (in-memory)
- **HttpClient** for all API communication
- **System.IO.Compression** for ZIP file generation

---

## Architecture & Design Patterns

### Overall Architecture

```
┌─────────────────────────────────────────────────────┐
│                  User Browser                       │
└──────────────────┬──────────────────────────────────┘
                   │ HTTPS
                   ↓
┌─────────────────────────────────────────────────────┐
│         HuvrWebApp (ASP.NET Core MVC)               │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────┐ │
│  │ Controllers  │→ │  Services    │→ │  Models   │ │
│  └──────────────┘  └──────────────┘  └───────────┘ │
└──────────────────┬──────────────────────────────────┘
                   │
        ┌──────────┴──────────┐
        ↓                     ↓
┌────────────────┐    ┌────────────────┐
│ HuvrApiClient  │    │  GeApmClient   │
│ (Library)      │    │  (Library)     │
└────────┬───────┘    └────────┬───────┘
         │                     │
         ↓                     ↓
    HUVR API v3           GE APM On-Prem
```

### Design Patterns Used

#### 1. MVC Pattern (Primary)
**Location:** HuvrWebApp
**Implementation:**
- **Models:** View models + configuration models
- **Views:** Razor templates with Bootstrap 5
- **Controllers:** Thin controllers delegating to services

#### 2. Service Layer Pattern
**Purpose:** Separate business logic from controllers
**Services:**
- `HuvrService`: Manages API client instances per session
- `ExportTemplateService`: Template CRUD operations
- `ImageDownloadService`: Image downloads and ZIP creation
- `RelatedEntityFieldResolver`: Cross-entity field resolution

#### 3. Repository Pattern (Implicit)
**Implementation:** API clients act as repositories
- `HuvrApiClient` → Repository for HUVR entities
- `GeApmClient` → Repository for GE APM entities

#### 4. Singleton Pattern
```csharp
// Program.cs
builder.Services.AddSingleton<IHuvrService, HuvrService>();
builder.Services.AddSingleton<ExportTemplateService>();
```

**Why Singleton:**
- `HuvrService` manages multiple client instances (one per session)
- `ExportTemplateService` maintains in-memory template cache

#### 5. Factory Pattern (Implicit)
**Location:** `HuvrService.GetOrCreateClient()`
```csharp
public HuvrApiClient GetOrCreateClient(string sessionId, string clientId, string clientSecret)
{
    return _clients.GetOrAdd(sessionId, _ => new HuvrApiClient(clientId, clientSecret));
}
```

#### 6. Strategy Pattern
**Location:** Excel export with different entity type strategies
```csharp
private async Task<List<JObject>> FetchDataForEntityType(string entityType, ...)
{
    return entityType.ToLower() switch
    {
        "assets" => await FetchAssetsAsync(...),
        "projects" => await FetchProjectsAsync(...),
        "defects" => await FetchDefectsAsync(...),
        // ...
    };
}
```

### Dependency Injection Configuration

```csharp
// Program.cs - Complete DI setup
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IHuvrService, HuvrService>();
builder.Services.AddSingleton<ExportTemplateService>();
builder.Services.AddScoped<ImageDownloadService>();
```

**Service Lifetimes:**
- **Singleton:** Services that manage state across users (HuvrService, ExportTemplateService)
- **Scoped:** Services tied to HTTP request lifecycle (ImageDownloadService)
- **Transient:** Not used (controllers are scoped by default)

---

## Development Workflows

### Building the Solution

```bash
# Restore dependencies
dotnet restore

# Build entire solution
dotnet build

# Build specific project
dotnet build HuvrWebApp/HuvrWebApp.csproj

# Run web application
cd HuvrWebApp
dotnet run
# Access at: https://localhost:56810
```

### Running the Web Application

```bash
cd HuvrWebApp
dotnet run

# Or with watch mode (auto-restart on changes)
dotnet watch run
```

**Default URLs:**
- HTTPS: `https://localhost:56810`
- HTTP: `http://localhost:56811`

### Configuration Files

#### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "HuvrApi": {
    "BaseUrl": "https://eastman.staging3.huvrdata.dev"
  }
}
```

**Environment-specific overrides:**
- `appsettings.Development.json` (not committed)
- `appsettings.Production.json` (not committed)

### Git Workflow

**Branch Naming Convention:**
```
claude/<description>-<session-id>
```

**Example:**
```
claude/add-claude-documentation-f98Y5
```

**Important Git Rules:**
1. Always develop on `claude/*` branches
2. Never push directly to `main` or `master`
3. Create pull requests for all changes
4. Use descriptive commit messages

**Commit Message Format:**
```
<verb> <description>

Examples:
- Add comprehensive task interface with project data integration
- Fix HuvrApiClient constructor overload
- Update all projects to target .NET 9.0
- Merge pull request #9 from mothware/...
```

---

## Code Conventions & Standards

### C# Naming Conventions

```csharp
// Class names: PascalCase
public class HuvrApiClient { }

// Interface names: IPascalCase
public interface IHuvrService { }

// Method names: PascalCase
public async Task<Asset> GetAssetAsync(string id) { }

// Properties: PascalCase
public string ClientId { get; set; }

// Private fields: _camelCase (with underscore)
private readonly HttpClient _httpClient;
private string? _authToken;

// Local variables and parameters: camelCase
var assetId = "123";
public void ProcessData(string entityType) { }

// Constants: PascalCase or UPPER_CASE
private const int DefaultTimeout = 120;
```

### Async/Await Patterns

**CRITICAL RULE:** 100% async throughout the codebase

```csharp
// ✅ CORRECT: Async methods suffixed with "Async"
public async Task<List<Asset>> GetAllAssetsAsync(CancellationToken cancellationToken = default)

// ✅ CORRECT: CancellationToken as last parameter with default
public async Task<Project> GetProjectAsync(string id, CancellationToken cancellationToken = default)

// ✅ CORRECT: Use ConfigureAwait(false) in library code
var result = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

// ❌ WRONG: Synchronous methods in async context
public List<Asset> GetAllAssets() // No Async suffix, returns List instead of Task

// ❌ WRONG: Blocking on async code
var result = GetAssetAsync(id).Result; // Deadlock risk!
```

### Null Handling

**Nullable Reference Types Enabled:**
```csharp
// ✅ Explicitly nullable
public string? OptionalField { get; set; }

// ✅ Non-nullable (must have value)
public string RequiredField { get; set; } = string.Empty;

// ✅ Null-coalescing operator
var value = optionalValue ?? "default";

// ✅ Null-conditional operator
var length = name?.Length ?? 0;

// ✅ Null-forgiving operator (when you're certain)
var definitelyNotNull = maybeNull!;
```

### JSON Property Naming

**HUVR API Convention:** snake_case
```csharp
[JsonPropertyName("asset_id")]
public string? AssetId { get; set; }

[JsonPropertyName("created_at")]
public DateTime? CreatedAt { get; set; }

[JsonPropertyName("inspection_media")]
public List<InspectionMedia> InspectionMedia { get; set; } = new();
```

**GE APM Convention:** PascalCase (matches C# properties)
```csharp
public string TmlId { get; set; }
public decimal? MeasuredThickness { get; set; }
```

### Error Handling Patterns

#### Controller-Level Error Handling
```csharp
try
{
    var result = await client.GetAssetAsync(assetId);
    return Json(new { success = true, data = result });
}
catch (HttpRequestException ex)
{
    return Json(new { success = false, error = ex.Message });
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error in GetAsset");
    return Json(new { success = false, error = "An unexpected error occurred" });
}
```

#### Service-Level Error Handling
```csharp
public async Task<ImageDownloadResult> DownloadImageAsync(string url)
{
    try
    {
        // Download logic
        return new ImageDownloadResult { Success = true, Content = data };
    }
    catch (Exception ex)
    {
        return new ImageDownloadResult { Success = false, Error = ex.Message };
    }
}
```

#### Validation Pattern
```csharp
public void Validate()
{
    if (string.IsNullOrWhiteSpace(ClientId))
        throw new InvalidOperationException("ClientId is required");
    if (string.IsNullOrWhiteSpace(ClientSecret))
        throw new InvalidOperationException("ClientSecret is required");
}
```

### Documentation Standards

**XML Documentation Required for:**
- All public APIs in client libraries
- All public methods in services
- Complex algorithms

```csharp
/// <summary>
/// Retrieves all assets from the HUVR API with automatic pagination.
/// </summary>
/// <param name="queryParams">Optional query parameters for filtering</param>
/// <param name="maxResults">Maximum number of results to return (null for all)</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>List of all assets matching the query</returns>
/// <exception cref="HttpRequestException">Thrown when API request fails</exception>
public async Task<List<Asset>> GetAllAssetsAsync(
    Dictionary<string, string>? queryParams = null,
    int? maxResults = null,
    CancellationToken cancellationToken = default)
{
    // Implementation
}
```

### File Organization

**Controllers:** One controller per domain area
```
AuthController.cs      - Authentication only
DashboardController.cs - API exploration
ExcelController.cs     - Excel export
ImageController.cs     - Image downloads
TemplateController.cs  - Template management
```

**Models:** Grouped by purpose
```
LoginViewModel.cs                    - Auth models
FieldMappingViewModel.cs             - Export models
ModelRelationshipConfiguration.cs    - Relationship metadata
ExportTemplateModels.cs              - Template models
```

**Services:** One service per responsibility
```
HuvrService.cs                  - API client lifecycle
ExportTemplateService.cs        - Template persistence
ImageDownloadService.cs         - Image/ZIP operations
RelatedEntityFieldResolver.cs   - Relationship resolution
```

---

## Key Components

### HuvrApiClient - HUVR API Integration

**Location:** `HuvrApiClient/HuvrApiClient.cs` (1322 lines)

**Responsibilities:**
1. Authenticate with HUVR API using service account credentials
2. Automatic token management with refresh 5 minutes before expiry
3. CRUD operations for all HUVR entities
4. Automatic pagination handling with `GetAll*` methods
5. Media upload with two-part workflow

**Key Features:**

```csharp
// Automatic authentication
await EnsureAuthenticatedAsync(cancellationToken);

// Pagination helper
public async Task<List<Asset>> GetAllAssetsAsync(
    Dictionary<string, string>? queryParams = null,
    int? maxResults = null,
    CancellationToken cancellationToken = default)
{
    var allAssets = new List<Asset>();
    string? url = "/api/v3/assets/";

    while (url != null && (!maxResults.HasValue || allAssets.Count < maxResults.Value))
    {
        var response = await GetPagedResultAsync<AssetListResponse>(url, queryParams, cancellationToken);
        allAssets.AddRange(response.Results);
        url = response.Next; // Follow pagination links
        queryParams = null; // Only use params on first request
    }

    return allAssets;
}
```

**Thread-Safe Token Management:**
```csharp
private readonly SemaphoreSlim _tokenLock = new(1, 1);

private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
{
    if (_currentToken == null || DateTime.UtcNow >= _currentToken.ExpiresAt.AddMinutes(-5))
    {
        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            await ObtainAccessTokenAsync(cancellationToken);
        }
        finally
        {
            _tokenLock.Release();
        }
    }
}
```

### GeApmClient - GE APM Integration

**Location:** `GeApmClient/GeApmClient.cs` (658 lines)

**Responsibilities:**
1. Authenticate with GE APM using username/password
2. Execute OData queries with full query parameter support
3. TML (Thickness Measurement Location) CRUD operations
4. Execute Genome queries (GE APM proprietary query language)
5. Generic entity operations for any family

**Authentication Pattern:**
```csharp
// MeridiumToken format: "sessionid;TimezoneName"
var meridiumToken = $"{_authToken};{_timeZone}";
_httpClient.DefaultRequestHeaders.Add("MeridiumToken", meridiumToken);
```

**OData Query Support:**
```csharp
public async Task<QueryResult<T>> QueryAsync<T>(
    string path,
    QueryParameters? parameters = null,
    CancellationToken cancellationToken = default)
{
    var url = $"/meridium/api/odata/{path.TrimStart('/')}";
    if (parameters != null)
    {
        // Supports: $filter, $select, $expand, $orderby, $top, $skip, $count
        url += $"?{parameters.ToQueryString()}";
    }
    // Execute query
}
```

### ModelRelationshipConfiguration - Entity Relationships

**Location:** `HuvrWebApp/Models/ModelRelationshipConfiguration.cs`

**Purpose:** Centralized configuration of all entity relationships

**Relationship Types:**
```csharp
public class RelationshipDefinition
{
    public string RelatedModel { get; set; }       // e.g., "Asset"
    public string ForeignKeyField { get; set; }    // e.g., "AssetId"
    public string RelatedKeyField { get; set; }    // e.g., "Id"
    public string Description { get; set; }        // Human-readable
    public bool IsCollection { get; set; }         // One-to-many vs one-to-one
}
```

**Example Relationships:**
```csharp
["Project"] = new List<RelationshipDefinition>
{
    new("Asset", "AssetId", "Id", "Parent asset information"),
    new("Defect", "Id", "ProjectId", "Associated defects", isCollection: true),
    new("Measurement", "Id", "ProjectId", "Measurements", isCollection: true)
},

["Defect"] = new List<RelationshipDefinition>
{
    new("Project", "ProjectId", "Id", "Parent project"),
    new("Asset", "AssetId", "Id", "Associated asset"),
    new("User", "IdentifiedBy", "Id", "User who identified defect"),
    new("DefectOverlay", "Id", "DefectId", "Overlay images", isCollection: true)
}
```

**Usage in Field Resolution:**
```csharp
// Get available fields including relationships
var fields = ModelRelationshipConfiguration.GetAvailableFieldsWithRelationships("Project");
// Returns: ["Id", "Name", "Asset.Name", "Asset.Location", ...]
```

### RelatedEntityFieldResolver - Cross-Entity Field Access

**Location:** `HuvrWebApp/Services/RelatedEntityFieldResolver.cs`

**Purpose:** Resolve field values across related entities efficiently

**Caching Strategy:**
```csharp
// Build cache once with all entity data
var resolver = RelatedEntityFieldResolver.BuildCache(new Dictionary<string, List<JObject>>
{
    ["Asset"] = assetList,
    ["Project"] = projectList,
    ["Defect"] = defectList,
    ["User"] = userList
});

// Fast lookups using cached data
foreach (var project in projects)
{
    var assetName = resolver.ResolveFieldValue(project, "Asset.Name", "Project");
    var assetLocation = resolver.ResolveFieldValue(project, "Asset.Location", "Project");
}
```

**Field Path Resolution:**
```csharp
// Direct field: "Name" → returns project.Name
// Related field: "Asset.Name" → finds Asset via AssetId, returns asset.Name
// Nested: "Asset.Library.Name" → follows chain of relationships
```

### ExportTemplateService - Template Persistence

**Location:** `HuvrWebApp/Services/ExportTemplateService.cs`

**Storage:** File-based JSON (`Data/ExportTemplates/templates.json`)

**Features:**
- In-memory cache using `ConcurrentDictionary<string, ExportTemplate>`
- Thread-safe writes with `SemaphoreSlim`
- CRUD operations: Create, Read, Update, Delete, Search, Duplicate

**Template Structure:**
```csharp
public class ExportTemplate
{
    public string Id { get; set; }                       // GUID
    public string Name { get; set; }                     // User-friendly name
    public string Description { get; set; }              // Optional
    public ExportTemplateType Type { get; set; }         // SingleSheet or MultiSheet
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; }                // Session ID

    public ExportRequest? SingleSheetConfig { get; set; }
    public MultiSheetExportRequest? MultiSheetConfig { get; set; }
}
```

---

## API Integration Patterns

### HUVR API Authentication Flow

```
1. Initialize client with ClientId/ClientSecret
   ↓
2. First API call triggers authentication
   ↓
3. POST /api/v3/auth/token/ with credentials
   ↓
4. Store token with expiration (60 minutes)
   ↓
5. Add Authorization: Bearer {token} to all requests
   ↓
6. Monitor token expiration
   ↓
7. Refresh token 5 minutes before expiry
   ↓
8. Repeat from step 3
```

**Implementation:**
```csharp
private async Task ObtainAccessTokenAsync(CancellationToken cancellationToken)
{
    var authRequest = new AuthRequest
    {
        ClientId = _clientId,
        ClientSecret = _clientSecret
    };

    var response = await _httpClient.PostAsJsonAsync(
        "/api/v3/auth/token/",
        authRequest,
        cancellationToken
    );
    response.EnsureSuccessStatusCode();

    _currentToken = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken);
    _httpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", _currentToken!.AccessToken);
}
```

### GE APM Authentication Flow

```
1. Initialize client with base URL and timezone
   ↓
2. POST /meridium/api/security/login with username/password
   ↓
3. Receive session ID
   ↓
4. Set MeridiumToken header: "{sessionId};{timezone}"
   ↓
5. Token remains valid for session duration
```

**Implementation:**
```csharp
public async Task<LoginResponse> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
{
    var loginRequest = new { UserName = username, Password = password };
    var response = await _httpClient.PostAsJsonAsync("/meridium/api/security/login", loginRequest, cancellationToken);
    response.EnsureSuccessStatusCode();

    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
    _authToken = loginResponse!.SessionId;

    // Set MeridiumToken header for all future requests
    var meridiumToken = $"{_authToken};{_timeZone}";
    _httpClient.DefaultRequestHeaders.Remove("MeridiumToken");
    _httpClient.DefaultRequestHeaders.Add("MeridiumToken", meridiumToken);

    return loginResponse;
}
```

### Pagination Pattern (HUVR API)

**HUVR API Response Format:**
```json
{
  "count": 150,
  "next": "https://api.huvrdata.app/api/v3/assets/?page=2",
  "previous": null,
  "results": [
    { "id": "1", "name": "Asset 1" },
    { "id": "2", "name": "Asset 2" }
  ]
}
```

**Automatic Pagination Handling:**
```csharp
var allItems = new List<Asset>();
string? url = "/api/v3/assets/";

while (url != null)
{
    var response = await GetPagedResultAsync<AssetListResponse>(url, queryParams, cancellationToken);
    allItems.AddRange(response.Results);
    url = response.Next; // Follow next link until null
    queryParams = null;  // Only use query params on first request
}
```

### Media Upload Pattern (HUVR API)

**Two-Part Upload Process:**
```csharp
// Part 1: Create media record
var mediaRequest = new InspectionMediaRequest
{
    ProjectId = projectId,
    FileName = "photo.jpg",
    FileType = "image/jpeg",
    FileSize = fileBytes.Length
};
var media = await client.CreateInspectionMediaAsync(mediaRequest);

// Part 2: Upload file to presigned URL (valid 15 minutes)
await client.UploadInspectionMediaFileAsync(
    media.UploadUrl!,
    fileBytes,
    "image/jpeg"
);
```

**Simplified Helper Method:**
```csharp
// All-in-one upload
var media = await client.UploadInspectionMediaAsync(
    mediaRequest,
    fileBytes,
    contentType
);
```

---

## Data Models & Relationships

### Core HUVR Entities

```
Asset
├── Id (string)
├── Name (string)
├── Description (string?)
├── AssetType (string?)
├── Location (string?)
├── Status (string?)
├── LibraryId (string?) → Library
├── WorkspaceId (string?) → Workspace
└── Projects (1:many) ← Project.AssetId

Project
├── Id (string)
├── Name (string)
├── Description (string?)
├── AssetId (string?) → Asset
├── Status (string?)
├── StartDate (DateTime?)
├── EndDate (DateTime?)
├── WorkspaceId (string?) → Workspace
├── Defects (1:many) ← Defect.ProjectId
├── Measurements (1:many) ← Measurement.ProjectId
├── InspectionMedia (1:many) ← InspectionMedia.ProjectId
└── Checklists (1:many) ← Checklist.ProjectId

Defect
├── Id (string)
├── ProjectId (string) → Project
├── AssetId (string?) → Asset
├── Title (string)
├── Description (string?)
├── Severity (string?)
├── Status (string?)
├── DefectType (string?)
├── Location (string?)
├── IdentifiedBy (string?) → User
├── IdentifiedAt (DateTime?)
└── Overlays (1:many) ← DefectOverlay.DefectId

DefectOverlay
├── Id (string)
├── DefectId (string) → Defect
├── MediaId (string?) → InspectionMedia
├── DisplayUrl (string?)
├── Note (string?)
├── GeometryExtra (object?)
├── CreatedBy (string?) → User
├── CreatedOn (DateTime?)
└── UpdatedOn (DateTime?)

Measurement
├── Id (string)
├── ProjectId (string?) → Project
├── AssetId (string?) → Asset
├── MeasurementType (string?)
├── Value (decimal?)
├── Unit (string?)
├── Location (string?)
├── MeasuredBy (string?)
└── MeasuredAt (DateTime?)

InspectionMedia
├── Id (string)
├── ProjectId (string) → Project
├── FileName (string)
├── FileType (string)
├── FileSize (long)
├── Status (string?)
├── DownloadUrl (string?)
├── ThumbnailUrl (string?)
├── PreviewUrl (string?)
├── UploadUrl (string?) (temporary, 15 min expiry)
├── Metadata (Dictionary<string, object>?)
├── CreatedAt (DateTime?)
└── UpdatedAt (DateTime?)
```

### Relationship Matrix

| Source Entity | Related Entity | Foreign Key | Type | Access Pattern |
|---------------|----------------|-------------|------|----------------|
| Project | Asset | AssetId | 1:1 | `project.Asset.Name` |
| Project | Defect | Id → ProjectId | 1:many | Separate sheet |
| Project | Measurement | Id → ProjectId | 1:many | Separate sheet |
| Project | InspectionMedia | Id → ProjectId | 1:many | Separate sheet |
| Defect | Project | ProjectId | 1:1 | `defect.Project.Name` |
| Defect | Asset | AssetId | 1:1 | `defect.Asset.Name` |
| Defect | User | IdentifiedBy | 1:1 | `defect.User.Email` |
| Defect | DefectOverlay | Id → DefectId | 1:many | Separate sheet |
| DefectOverlay | Defect | DefectId | 1:1 | `overlay.Defect.Title` |
| DefectOverlay | InspectionMedia | MediaId | 1:1 | `overlay.Media.DownloadUrl` |
| DefectOverlay | User | CreatedBy | 1:1 | `overlay.User.Email` |
| Measurement | Project | ProjectId | 1:1 | `measurement.Project.Name` |
| Measurement | Asset | AssetId | 1:1 | `measurement.Asset.Name` |
| InspectionMedia | Project | ProjectId | 1:1 | `media.Project.Name` |
| Asset | Library | LibraryId | 1:1 | `asset.Library.Name` |
| Checklist | Project | ProjectId | 1:1 | `checklist.Project.Name` |

---

## Common Tasks & Examples

### Task 1: Add a New HUVR API Endpoint

**Scenario:** HUVR adds a new "Equipment" entity

**Steps:**

1. **Create the model** in `HuvrApiClient/Models/Equipment.cs`:
```csharp
using System.Text.Json.Serialization;

namespace HuvrApiClient.Models;

public class Equipment
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("asset_id")]
    public string? AssetId { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
}

public class EquipmentRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("asset_id")]
    public string? AssetId { get; set; }
}

public class EquipmentListResponse
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }

    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    [JsonPropertyName("results")]
    public List<Equipment> Results { get; set; } = new();
}
```

2. **Add methods to HuvrApiClient.cs**:
```csharp
// List equipment
public async Task<EquipmentListResponse> ListEquipmentAsync(
    Dictionary<string, string>? queryParams = null,
    CancellationToken cancellationToken = default)
{
    await EnsureAuthenticatedAsync(cancellationToken);
    var url = BuildUrl("/api/v3/equipment/", queryParams);
    var response = await _httpClient.GetAsync(url, cancellationToken);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<EquipmentListResponse>(cancellationToken)
        ?? new EquipmentListResponse();
}

// Get single equipment
public async Task<Equipment> GetEquipmentAsync(string id, CancellationToken cancellationToken = default)
{
    await EnsureAuthenticatedAsync(cancellationToken);
    var url = $"/api/v3/equipment/{id}/";
    var response = await _httpClient.GetAsync(url, cancellationToken);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<Equipment>(cancellationToken)
        ?? throw new InvalidOperationException("Equipment not found");
}

// Create equipment
public async Task<Equipment> CreateEquipmentAsync(
    EquipmentRequest request,
    CancellationToken cancellationToken = default)
{
    await EnsureAuthenticatedAsync(cancellationToken);
    var response = await _httpClient.PostAsJsonAsync("/api/v3/equipment/", request, cancellationToken);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadFromJsonAsync<Equipment>(cancellationToken)
        ?? throw new InvalidOperationException("Failed to create equipment");
}

// Pagination helper
public async Task<List<Equipment>> GetAllEquipmentAsync(
    Dictionary<string, string>? queryParams = null,
    int? maxResults = null,
    CancellationToken cancellationToken = default)
{
    var allEquipment = new List<Equipment>();
    string? url = "/api/v3/equipment/";

    while (url != null && (!maxResults.HasValue || allEquipment.Count < maxResults.Value))
    {
        var response = await GetPagedResultAsync<EquipmentListResponse>(url, queryParams, cancellationToken);
        allEquipment.AddRange(response.Results);
        url = response.Next;
        queryParams = null;
    }

    return allEquipment;
}
```

3. **Add relationship in ModelRelationshipConfiguration.cs**:
```csharp
["Equipment"] = new List<RelationshipDefinition>
{
    new("Asset", "AssetId", "Id", "Parent asset"),
},

["Asset"] = new List<RelationshipDefinition>
{
    // ... existing relationships ...
    new("Equipment", "Id", "AssetId", "Equipment items", isCollection: true),
}
```

4. **Add to ExcelController.cs**:
```csharp
private List<string> GetFieldsForEntityType(string entityType)
{
    return entityType.ToLower() switch
    {
        // ... existing cases ...
        "equipment" => new List<string> { "Id", "Name", "AssetId", "CreatedAt", "Asset.Name" },
        _ => new List<string>()
    };
}

private async Task<List<JObject>> FetchDataForEntityType(string entityType, ...)
{
    return entityType.ToLower() switch
    {
        // ... existing cases ...
        "equipment" => await FetchEquipmentAsync(client, cancellationToken),
        _ => new List<JObject>()
    };
}

private async Task<List<JObject>> FetchEquipmentAsync(HuvrApiClient client, CancellationToken cancellationToken)
{
    var equipment = await client.GetAllEquipmentAsync(cancellationToken: cancellationToken);
    return equipment.Select(e => JObject.FromObject(e)).ToList();
}
```

5. **Update UI dropdown** in Views:
```html
<option value="equipment">Equipment</option>
```

### Task 2: Create a Multi-Sheet Excel Export

**Scenario:** Export all projects with their defects and measurements

**Code Example:**
```csharp
// In ExcelController or custom service
var exportRequest = new MultiSheetExportRequest
{
    Sheets = new List<SheetConfiguration>
    {
        // Sheet 1: Projects
        new SheetConfiguration
        {
            SheetName = "Projects",
            EntityType = "projects",
            StartRow = 1,
            Mappings = new List<FieldMapping>
            {
                new() { ApiField = "Id", ExcelColumn = "Project ID", IsSelected = true },
                new() { ApiField = "Name", ExcelColumn = "Project Name", IsSelected = true },
                new() { ApiField = "Asset.Name", ExcelColumn = "Asset", IsSelected = true },
                new() { ApiField = "Status", ExcelColumn = "Status", IsSelected = true },
            }
        },

        // Sheet 2: Defects
        new SheetConfiguration
        {
            SheetName = "Defects",
            EntityType = "defects",
            StartRow = 1,
            Mappings = new List<FieldMapping>
            {
                new() { ApiField = "Id", ExcelColumn = "Defect ID", IsSelected = true },
                new() { ApiField = "Title", ExcelColumn = "Title", IsSelected = true },
                new() { ApiField = "Severity", ExcelColumn = "Severity", IsSelected = true },
                new() { ApiField = "Project.Name", ExcelColumn = "Project", IsSelected = true },
                new() { ApiField = "Asset.Name", ExcelColumn = "Asset", IsSelected = true },
                new() { ApiField = "User.Email", ExcelColumn = "Identified By", IsSelected = true },
            }
        },

        // Sheet 3: Measurements
        new SheetConfiguration
        {
            SheetName = "Measurements",
            EntityType = "measurements",
            StartRow = 1,
            Mappings = new List<FieldMapping>
            {
                new() { ApiField = "Id", ExcelColumn = "ID", IsSelected = true },
                new() { ApiField = "MeasurementType", ExcelColumn = "Type", IsSelected = true },
                new() { ApiField = "Value", ExcelColumn = "Value", IsSelected = true },
                new() { ApiField = "Unit", ExcelColumn = "Unit", IsSelected = true },
                new() { ApiField = "Project.Name", ExcelColumn = "Project", IsSelected = true },
            }
        }
    },
    LinkRelatedData = true
};

// Export via controller action
return await ExportToExcelMultiSheet(exportRequest);
```

### Task 3: Download Images for a Defect

**Via UI:**
```javascript
function downloadDefectImages(defectId) {
    $.ajax({
        url: '/Image/DownloadDefectImages',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ DefectId: defectId }),
        xhrFields: { responseType: 'blob' },
        success: function(blob) {
            var url = window.URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.href = url;
            a.download = 'defect_' + defectId + '_images.zip';
            a.click();
            window.URL.revokeObjectURL(url);
        },
        error: function(xhr) {
            alert('Error downloading images: ' + xhr.statusText);
        }
    });
}
```

**Via Service (C#):**
```csharp
var imageService = serviceProvider.GetRequiredService<ImageDownloadService>();
var huvrService = serviceProvider.GetRequiredService<IHuvrService>();
var client = huvrService.GetClient(sessionId);

// Get defect with overlays
var defect = await client.GetDefectAsync(defectId);

// Extract image info
var overlayImages = new List<DefectOverlayImageInfo>();
foreach (var overlay in defect.Overlays ?? new List<DefectOverlay>())
{
    overlayImages.Add(new DefectOverlayImageInfo
    {
        OverlayId = overlay.Id,
        DisplayUrl = overlay.DisplayUrl,
        Media = overlay.Media
    });
}

// Download as ZIP
var result = await imageService.DownloadDefectOverlayImagesAsync(overlayImages, defectId);

if (result.Success)
{
    // result.Content contains ZIP bytes
    // result.FileName is "defect_{defectId}_images.zip"
}
```

### Task 4: Save and Reuse Export Template

**Save Template via UI:**
```javascript
// User configures export on FieldMapping page
var templateData = {
    Name: "Weekly Project Report",
    Description: "Standard weekly export with assets and defects",
    Type: 1, // MultiSheet
    MultiSheetConfig: {
        Sheets: [
            {
                SheetName: "Projects",
                EntityType: "projects",
                StartRow: 1,
                Mappings: [/* field mappings */]
            },
            {
                SheetName: "Defects",
                EntityType: "defects",
                StartRow: 1,
                Mappings: [/* field mappings */]
            }
        ],
        LinkRelatedData: true
    }
};

$.post('/Template/SaveTemplate', JSON.stringify(templateData), function(response) {
    if (response.success) {
        alert('Template saved: ' + response.template.id);
    }
});
```

**Load and Export from Template:**
```javascript
// Load template
$.get('/Template/GetTemplate', { id: templateId }, function(response) {
    if (response.success) {
        var config = response.template.multiSheetConfig;
        // Populate UI with config
    }
});

// Export directly from template
window.location.href = '/Excel/ExportFromTemplate?templateId=' + templateId;
```

---

## Testing Strategy

### Current State
**No automated tests** currently exist in the repository.

### Recommended Testing Approach

#### Unit Tests (Recommended)
```csharp
// HuvrApiClient.Tests/AuthenticationTests.cs
public class AuthenticationTests
{
    [Fact]
    public async Task ObtainAccessToken_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("/api/v3/auth/token/")
            .Respond("application/json", "{\"access_token\":\"test\",\"expires_at\":\"2025-01-01T00:00:00Z\"}");

        var client = new HuvrApiClient("client-id", "secret", new HttpClient(mockHttp));

        // Act
        var token = await client.ObtainAccessTokenAsync();

        // Assert
        Assert.NotNull(token);
        Assert.Equal("test", token.AccessToken);
    }
}
```

#### Integration Tests (Recommended)
```csharp
// HuvrWebApp.Tests/ExcelExportTests.cs
public class ExcelExportTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ExcelExportTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ExportToExcel_WithValidData_ReturnsExcelFile()
    {
        // Arrange
        var client = _factory.CreateClient();
        // ... setup test data

        // Act
        var response = await client.PostAsJsonAsync("/Excel/ExportToExcel", exportRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            response.Content.Headers.ContentType?.MediaType);
    }
}
```

#### Manual Testing Checklist

**Authentication:**
- [ ] Login with valid credentials
- [ ] Login with invalid credentials
- [ ] Session timeout after 60 minutes
- [ ] Logout clears session

**Excel Export:**
- [ ] Single-sheet export for each entity type
- [ ] Multi-sheet export with 2+ sheets
- [ ] Export with related entity fields
- [ ] Export with custom column names
- [ ] Export with start row configuration

**Template System:**
- [ ] Save single-sheet template
- [ ] Save multi-sheet template
- [ ] Load template
- [ ] Export from template
- [ ] Duplicate template
- [ ] Delete template
- [ ] Search templates

**Image Download:**
- [ ] Download images for defect with overlays
- [ ] Handle defect with no overlays
- [ ] Handle invalid defect ID
- [ ] Verify ZIP file contents

---

## Deployment & Configuration

### Production Deployment

**Prerequisites:**
- .NET 9.0 Runtime installed
- HTTPS certificate configured
- Access to HUVR API (network connectivity)
- (Optional) Access to GE APM On-Premises

**Build for Production:**
```bash
dotnet publish HuvrWebApp/HuvrWebApp.csproj -c Release -o ./publish
```

**Configuration:**

Create `appsettings.Production.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "your-domain.com",
  "HuvrApi": {
    "BaseUrl": "https://api.huvrdata.app"
  }
}
```

**Run in Production:**
```bash
cd publish
export ASPNETCORE_ENVIRONMENT=Production
dotnet HuvrWebApp.dll
```

**Recommended: Use systemd service (Linux)**
```ini
# /etc/systemd/system/huvrwebapp.service
[Unit]
Description=HUVR APM Integration Web App

[Service]
WorkingDirectory=/var/www/huvrwebapp
ExecStart=/usr/bin/dotnet /var/www/huvrwebapp/HuvrWebApp.dll
Restart=always
RestartSec=10
SyslogIdentifier=huvrwebapp
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

### Environment Variables

| Variable | Purpose | Default |
|----------|---------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | Development |
| `ASPNETCORE_URLS` | Listen URLs | http://localhost:5000 |
| `HuvrApi__BaseUrl` | HUVR API URL | https://eastman.staging3.huvrdata.dev |

**Override via environment:**
```bash
export HuvrApi__BaseUrl="https://api.huvrdata.app"
export ASPNETCORE_URLS="http://0.0.0.0:5000;https://0.0.0.0:5001"
```

### File Storage Considerations

**Template Storage:**
- Location: `HuvrWebApp/Data/ExportTemplates/templates.json`
- Ensure directory is writable by application user
- Backup regularly (contains user configurations)

**Temporary Files:**
- ZIP downloads created in temp directory
- Automatically cleaned up after response
- No persistent storage required

### Scaling Considerations

**Current Limitations:**
- **Session-based auth** → Sticky sessions required for load balancing
- **In-memory template cache** → Not shared across instances
- **File-based templates** → Need shared storage for multi-instance deployment

**Scaling Solutions:**
1. **Sticky Sessions**: Configure load balancer for session affinity
2. **Shared Storage**: Mount `Data/ExportTemplates` as shared volume (NFS, Azure Files, etc.)
3. **Future Enhancement**: Migrate templates to database (SQL Server, PostgreSQL)

---

## Troubleshooting Guide

### Common Issues

#### Issue: "Not authenticated" errors
**Symptoms:** API calls fail with 401 Unauthorized
**Causes:**
- Session expired (60-minute timeout)
- Invalid credentials
- HUVR API service account disabled

**Solutions:**
1. Check session timeout: `HttpContext.Session.GetString("SessionId")`
2. Verify credentials in login form
3. Test credentials directly with HUVR API
4. Check HUVR API base URL in configuration

#### Issue: Excel export returns empty file
**Symptoms:** Downloaded Excel has no data rows
**Causes:**
- No data available for entity type
- API query returns empty results
- Field mapping excludes all fields

**Solutions:**
1. Check API response in Dashboard first
2. Verify entity type has data in HUVR
3. Review field selection (ensure at least one field selected)
4. Check browser console for JavaScript errors

#### Issue: Related entity fields show null
**Symptoms:** `Asset.Name` shows empty in export
**Causes:**
- Foreign key field is null (no relationship)
- Related entity doesn't exist
- Relationship not configured in ModelRelationshipConfiguration

**Solutions:**
1. Verify foreign key has value: Check `AssetId` is not null
2. Ensure related entity exists in HUVR
3. Check ModelRelationshipConfiguration has relationship defined
4. Use direct fields instead of related fields as fallback

#### Issue: Image download fails
**Symptoms:** "No overlays found" or empty ZIP
**Causes:**
- Defect has no overlay images
- Image URLs are invalid/expired
- Network connectivity issues

**Solutions:**
1. Verify defect has overlays in HUVR web UI
2. Check DefectOverlay data via Dashboard
3. Test image URLs directly in browser
4. Check network/firewall settings

#### Issue: Template not saving
**Symptoms:** Save button doesn't work or returns error
**Causes:**
- `Data/ExportTemplates` directory doesn't exist
- No write permissions
- JSON serialization error
- Disk full

**Solutions:**
1. Create directory manually: `mkdir -p HuvrWebApp/Data/ExportTemplates`
2. Check permissions: `chmod 755 HuvrWebApp/Data/ExportTemplates`
3. Check application logs for serialization errors
4. Verify disk space: `df -h`

### Debugging Tips

**Enable Detailed Logging:**
```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "HuvrWebApp": "Debug"
    }
  }
}
```

**Inspect API Responses:**
```csharp
// In controller action
var json = JsonConvert.SerializeObject(data, Formatting.Indented);
Console.WriteLine($"API Response: {json}");
```

**Test API Clients Directly:**
```csharp
// In Program.cs or separate console app
var client = new HuvrApiClient("client-id", "secret");
var assets = await client.ListAssetsAsync();
Console.WriteLine($"Found {assets.Results.Count} assets");
```

**Browser DevTools:**
- **Network Tab:** Check API call responses (XHR)
- **Console:** Look for JavaScript errors
- **Application Tab:** Inspect session storage

---

## Important Constraints

### CRITICAL Rules for AI Assistants

#### 1. Async/Await is Mandatory
```csharp
// ✅ ALWAYS use async/await
public async Task<Asset> GetAssetAsync(string id, CancellationToken cancellationToken = default)

// ❌ NEVER use synchronous methods
public Asset GetAsset(string id) // FORBIDDEN
```

**Reason:** Entire codebase is 100% async. Mixing sync/async causes deadlocks.

#### 2. Null Safety is Enabled
```csharp
// ✅ Explicitly mark nullable types
public string? OptionalValue { get; set; }

// ✅ Provide non-null default for required fields
public string RequiredValue { get; set; } = string.Empty;

// ❌ NEVER ignore nullable warnings
#nullable disable // FORBIDDEN
```

#### 3. JSON Property Naming
```csharp
// ✅ HUVR API uses snake_case
[JsonPropertyName("asset_id")]
public string? AssetId { get; set; }

// ❌ Don't rely on default camelCase serialization
public string AssetId { get; set; } // Will fail - API expects "asset_id"
```

#### 4. Session Management
```csharp
// ✅ Always check session before API calls
var sessionId = HttpContext.Session.GetString("SessionId");
if (sessionId == null)
{
    return RedirectToAction("Login", "Auth");
}

// ❌ Don't assume user is authenticated
var client = _huvrService.GetClient(sessionId); // May be null!
```

#### 5. Pagination is Required
```csharp
// ✅ Use GetAll* methods for complete data
var allAssets = await client.GetAllAssetsAsync();

// ⚠️ List* methods return only first page (default 50-100 items)
var firstPage = await client.ListAssetsAsync(); // Only first page!
```

#### 6. Thread Safety for Shared State
```csharp
// ✅ Use thread-safe collections
private readonly ConcurrentDictionary<string, HuvrApiClient> _clients = new();

// ✅ Use locks/semaphores for critical sections
private readonly SemaphoreSlim _lock = new(1, 1);

// ❌ Don't use non-thread-safe collections in singletons
private Dictionary<string, HuvrApiClient> _clients = new(); // DANGEROUS in Singleton
```

#### 7. CancellationToken Pattern
```csharp
// ✅ Always include CancellationToken as last parameter with default
public async Task<Asset> GetAssetAsync(
    string id,
    CancellationToken cancellationToken = default)

// ✅ Pass cancellationToken to all async calls
await _httpClient.GetAsync(url, cancellationToken);

// ❌ Don't omit CancellationToken
public async Task<Asset> GetAssetAsync(string id) // Missing cancellation support
```

#### 8. Model Relationships Must Be Registered
```csharp
// ✅ Add to ModelRelationshipConfiguration when creating new models
["NewEntity"] = new List<RelationshipDefinition>
{
    new("RelatedEntity", "ForeignKeyField", "Id", "Description")
}

// ❌ Don't expect relationship resolution without configuration
// "NewEntity.RelatedEntity.Field" won't work without registration
```

#### 9. Dispose IDisposable Resources
```csharp
// ✅ Use 'using' statements
using var client = new HuvrApiClient(clientId, secret);

// ✅ Or implement IDisposable properly
public void Dispose()
{
    _httpClient?.Dispose();
    _semaphore?.Dispose();
}

// ❌ Don't leak resources
var client = new HuvrApiClient(clientId, secret);
// ... no disposal
```

#### 10. Error Handling Standards
```csharp
// ✅ Return structured errors in controllers
return Json(new { success = false, error = ex.Message });

// ✅ Use specific exception types
throw new InvalidOperationException("Asset ID is required");

// ❌ Don't return generic errors
return StatusCode(500); // User sees unhelpful "Internal Server Error"
```

### Performance Constraints

**HttpClient Timeout:** 5 minutes (for large media downloads)
**Session Timeout:** 60 minutes (configurable in Program.cs)
**Max Excel Rows:** Limited by ClosedXML and memory (tested up to 100K rows)
**Concurrent API Calls:** Not limited, but be mindful of rate limits

### Security Constraints

**Authentication:**
- Credentials stored in session only (not persisted)
- No authentication bypass allowed
- Session ID must be validated on every protected action

**File Access:**
- Only allow reading from `Data/ExportTemplates`
- Sanitize all user-provided filenames
- Validate image URLs before downloading

**API Access:**
- Only use authenticated API clients
- Never expose service account credentials to client-side
- Validate all user input before API calls

---

## Quick Reference

### File Locations

| Component | Location |
|-----------|----------|
| HUVR API Client | `HuvrApiClient/HuvrApiClient.cs` |
| GE APM Client | `GeApmClient/GeApmClient.cs` |
| Excel Export Controller | `HuvrWebApp/Controllers/ExcelController.cs` |
| Template Service | `HuvrWebApp/Services/ExportTemplateService.cs` |
| Image Service | `HuvrWebApp/Services/ImageDownloadService.cs` |
| Relationship Config | `HuvrWebApp/Models/ModelRelationshipConfiguration.cs` |
| Field Resolver | `HuvrWebApp/Services/RelatedEntityFieldResolver.cs` |
| Configuration | `HuvrWebApp/appsettings.json` |
| Templates Storage | `HuvrWebApp/Data/ExportTemplates/templates.json` |

### URLs & Endpoints

| Feature | URL |
|---------|-----|
| Login | `/Auth/Login` |
| Dashboard | `/Dashboard` |
| Single-Sheet Export | `/Excel/FieldMapping` |
| Multi-Sheet Export | `/Excel/MultiSheetMapping` |
| Template Management | `/Template/Index` |
| Download Images | `POST /Image/DownloadDefectImages` |
| Export from Template | `POST /Excel/ExportFromTemplate?templateId={id}` |

### Documentation Files

| Topic | File |
|-------|------|
| HUVR API Spec | `HUVR_API_DEFINITION.md` |
| HUVR API Endpoints | `HUVR_API_ENDPOINTS_REFERENCE.md` |
| GE APM API Spec | `GE_APM_API_SPECIFICATION.md` |
| Multi-Sheet Export | `MULTI_SHEET_EXPORT_IMPLEMENTATION.md` |
| Template System | `TEMPLATE_SYSTEM_DOCUMENTATION.md` |
| Image Downloads | `IMAGE_DOWNLOAD_DOCUMENTATION.md` |
| Frontend Enhancements | `FRONTEND_ENHANCEMENT_SUMMARY.md` |
| **This Guide** | `CLAUDE.md` |

---

## Changelog

| Date | Version | Changes |
|------|---------|---------|
| 2026-01-06 | 1.0 | Initial CLAUDE.md creation |

---

## Contact & Support

For questions about this codebase:
1. Review this documentation first
2. Check specific feature documentation in root directory
3. Review inline code comments and XML documentation
4. Consult README files in each project directory

**Key Resources:**
- HuvrApiClient: `HuvrApiClient/README.md`
- GeApmClient: `GeApmClient/README.md`
- Web Application: `HuvrWebApp/README.md`
- Quick Start: `HuvrWebApp/QUICKSTART.md`

---

**End of CLAUDE.md**
