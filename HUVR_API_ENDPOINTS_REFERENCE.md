# HUVR API Endpoints Reference with Sample Responses

This document provides detailed information about all HUVR API endpoints, including sample requests and responses.

**Base URL**: `https://api.huvrdata.app`

**API Version**: v3

---

## Table of Contents

1. [Authentication](#authentication)
2. [Assets](#assets)
3. [Projects](#projects)
4. [Inspection Media](#inspection-media)
5. [Checklists](#checklists)
6. [Defects](#defects)
7. [Measurements](#measurements)
8. [Users](#users)
9. [Workspaces](#workspaces)

---

## Authentication

### Obtain Access Token

**Endpoint**: `POST /api/auth/obtain-access-token/`

**Authentication**: None (uses CLIENT_ID/CLIENT_SECRET)

**Purpose**: Obtain a new access token for API authentication

**Request**:
```http
POST /api/auth/obtain-access-token/
Content-Type: application/json

{
  "client_id": "[email protected]",
  "client_secret": "hvr_sk_1234567890abcdefghijklmnopqrstuvwxyz"
}
```

**Success Response** (200 OK):
```json
{
  "access_token": "hvr_at_abcdef1234567890",
  "token_type": "Token",
  "expires_in": 3600
}
```

**Error Response** (401 Unauthorized):
```json
{
  "detail": "Invalid credentials",
  "code": "invalid_credentials"
}
```

**C# Usage**:
```csharp
var client = new HuvrApiClient(clientId, clientSecret);
var token = await client.ObtainAccessTokenAsync();
Console.WriteLine($"Token expires at: {token.ExpiresAt}");
```

---

## Assets

### List Assets

**Endpoint**: `GET /api/assets/`

**Authentication**: Required

**Query Parameters**:
- `name` - Filter by asset name
- `asset_type` - Filter by asset type
- `location` - Filter by location
- `status` - Filter by status
- `limit` - Number of results per page (default: 50)
- `offset` - Pagination offset

**Request**:
```http
GET /api/assets/?status=Active&limit=10
Authorization: Token hvr_at_abcdef1234567890
```

**Success Response** (200 OK):
```json
{
  "count": 150,
  "next": "https://api.huvrdata.app/api/assets/?offset=10&limit=10",
  "previous": null,
  "results": [
    {
      "id": "ast_abc123",
      "name": "Pump #123",
      "description": "Main cooling pump for Building A",
      "asset_type": "Pump",
      "location": "Building A, Floor 2",
      "status": "Active",
      "metadata": {
        "serial_number": "P-2024-123",
        "manufacturer": "ACME Corp",
        "install_date": "2024-01-15"
      },
      "created_at": "2024-01-15T10:30:00Z",
      "updated_at": "2024-10-20T14:22:00Z"
    },
    {
      "id": "ast_def456",
      "name": "Boiler #45",
      "description": "Primary heating boiler",
      "asset_type": "Boiler",
      "location": "Building B, Basement",
      "status": "Active",
      "metadata": {
        "serial_number": "B-2023-045",
        "capacity": "500 kW"
      },
      "created_at": "2023-06-10T08:15:00Z",
      "updated_at": "2024-09-15T11:30:00Z"
    }
  ]
}
```

**C# Usage**:
```csharp
var assets = await client.ListAssetsAsync(new Dictionary<string, string>
{
    { "status", "Active" },
    { "limit", "10" }
});
Console.WriteLine($"Found {assets.Count} total assets");
```

---

### Get Asset Details

**Endpoint**: `GET /api/assets/{asset_id}/`

**Authentication**: Required

**Request**:
```http
GET /api/assets/ast_abc123/
Authorization: Token hvr_at_abcdef1234567890
```

**Success Response** (200 OK):
```json
{
  "id": "ast_abc123",
  "name": "Pump #123",
  "description": "Main cooling pump for Building A",
  "asset_type": "Pump",
  "location": "Building A, Floor 2",
  "status": "Active",
  "metadata": {
    "serial_number": "P-2024-123",
    "manufacturer": "ACME Corp",
    "install_date": "2024-01-15",
    "last_inspection": "2024-10-01"
  },
  "created_at": "2024-01-15T10:30:00Z",
  "updated_at": "2024-10-20T14:22:00Z"
}
```

**Error Response** (404 Not Found):
```json
{
  "detail": "Not found.",
  "code": "not_found"
}
```

**C# Usage**:
```csharp
var asset = await client.GetAssetAsync("ast_abc123");
Console.WriteLine($"Asset: {asset.Name}");
```

---

### Create Asset

**Endpoint**: `POST /api/assets/`

**Authentication**: Required

**Request**:
```http
POST /api/assets/
Authorization: Token hvr_at_abcdef1234567890
Content-Type: application/json

{
  "name": "Pump #124",
  "description": "Backup cooling pump",
  "asset_type": "Pump",
  "location": "Building A, Floor 2",
  "status": "Active",
  "metadata": {
    "serial_number": "P-2024-124",
    "manufacturer": "ACME Corp"
  }
}
```

**Success Response** (201 Created):
```json
{
  "id": "ast_xyz789",
  "name": "Pump #124",
  "description": "Backup cooling pump",
  "asset_type": "Pump",
  "location": "Building A, Floor 2",
  "status": "Active",
  "metadata": {
    "serial_number": "P-2024-124",
    "manufacturer": "ACME Corp"
  },
  "created_at": "2025-01-28T15:30:00Z",
  "updated_at": "2025-01-28T15:30:00Z"
}
```

**C# Usage**:
```csharp
var asset = await client.CreateAssetAsync(new AssetRequest
{
    Name = "Pump #124",
    Description = "Backup cooling pump",
    AssetType = "Pump",
    Location = "Building A, Floor 2",
    Status = "Active"
});
```

---

### Update Asset (Partial)

**Endpoint**: `PATCH /api/assets/{asset_id}/`

**Authentication**: Required

**Request**:
```http
PATCH /api/assets/ast_abc123/
Authorization: Token hvr_at_abcdef1234567890
Content-Type: application/json

{
  "status": "Maintenance",
  "metadata": {
    "last_maintenance": "2025-01-28"
  }
}
```

**Success Response** (200 OK):
```json
{
  "id": "ast_abc123",
  "name": "Pump #123",
  "description": "Main cooling pump for Building A",
  "asset_type": "Pump",
  "location": "Building A, Floor 2",
  "status": "Maintenance",
  "metadata": {
    "serial_number": "P-2024-123",
    "manufacturer": "ACME Corp",
    "install_date": "2024-01-15",
    "last_maintenance": "2025-01-28"
  },
  "created_at": "2024-01-15T10:30:00Z",
  "updated_at": "2025-01-28T15:45:00Z"
}
```

**C# Usage**:
```csharp
var updated = await client.UpdateAssetAsync("ast_abc123", new
{
    Status = "Maintenance"
});
```

---

### Delete Asset

**Endpoint**: `DELETE /api/assets/{asset_id}/`

**Authentication**: Required

**Request**:
```http
DELETE /api/assets/ast_abc123/
Authorization: Token hvr_at_abcdef1234567890
```

**Success Response** (204 No Content):
```
(No response body)
```

**C# Usage**:
```csharp
await client.DeleteAssetAsync("ast_abc123");
```

---

## Projects

### List Projects

**Endpoint**: `GET /api/projects/`

**Authentication**: Required

**Query Parameters**:
- `asset_search` - Filter by asset ID or name
- `status` - Filter by project status
- `project_type_id` - Filter by project type
- `start_date_after` - Filter by start date (ISO format)
- `limit` - Number of results per page

**Request**:
```http
GET /api/projects/?status=In Progress&limit=10
Authorization: Token hvr_at_abcdef1234567890
```

**Success Response** (200 OK):
```json
{
  "count": 85,
  "next": "https://api.huvrdata.app/api/projects/?offset=10&limit=10",
  "previous": null,
  "results": [
    {
      "id": "prj_123abc",
      "name": "Quarterly Inspection Q1 2025",
      "description": "Routine quarterly inspection",
      "asset_id": "ast_abc123",
      "project_type_id": "pt_inspection",
      "status": "In Progress",
      "start_date": "2025-01-15T09:00:00Z",
      "end_date": null,
      "assigned_users": ["usr_john123", "usr_jane456"],
      "metadata": {
        "inspector": "John Doe",
        "priority": "High"
      },
      "created_at": "2025-01-10T14:00:00Z",
      "updated_at": "2025-01-28T10:30:00Z"
    }
  ]
}
```

**C# Usage**:
```csharp
var projects = await client.ListProjectsAsync(new Dictionary<string, string>
{
    { "status", "In Progress" }
});
```

---

### Get Project Details

**Endpoint**: `GET /api/projects/{project_id}/`

**Authentication**: Required

**Request**:
```http
GET /api/projects/prj_123abc/
Authorization: Token hvr_at_abcdef1234567890
```

**Success Response** (200 OK):
```json
{
  "id": "prj_123abc",
  "name": "Quarterly Inspection Q1 2025",
  "description": "Routine quarterly inspection",
  "asset_id": "ast_abc123",
  "project_type_id": "pt_inspection",
  "status": "In Progress",
  "start_date": "2025-01-15T09:00:00Z",
  "end_date": null,
  "assigned_users": ["usr_john123", "usr_jane456"],
  "metadata": {
    "inspector": "John Doe",
    "priority": "High",
    "weather": "Sunny, 22°C"
  },
  "created_at": "2025-01-10T14:00:00Z",
  "updated_at": "2025-01-28T10:30:00Z"
}
```

**C# Usage**:
```csharp
var project = await client.GetProjectAsync("prj_123abc");
Console.WriteLine($"Project: {project.Name}");
```

---

### Create Project

**Endpoint**: `POST /api/projects/`

**Authentication**: Required

**Request**:
```http
POST /api/projects/
Authorization: Token hvr_at_abcdef1234567890
Content-Type: application/json

{
  "name": "Monthly Inspection - February 2025",
  "description": "Monthly routine inspection",
  "asset_id": "ast_abc123",
  "project_type_id": "pt_inspection",
  "status": "Scheduled",
  "start_date": "2025-02-01T09:00:00Z",
  "assigned_users": ["usr_john123"]
}
```

**Success Response** (201 Created):
```json
{
  "id": "prj_new456",
  "name": "Monthly Inspection - February 2025",
  "description": "Monthly routine inspection",
  "asset_id": "ast_abc123",
  "project_type_id": "pt_inspection",
  "status": "Scheduled",
  "start_date": "2025-02-01T09:00:00Z",
  "end_date": null,
  "assigned_users": ["usr_john123"],
  "metadata": {},
  "created_at": "2025-01-28T16:00:00Z",
  "updated_at": "2025-01-28T16:00:00Z"
}
```

**C# Usage**:
```csharp
var project = await client.CreateProjectAsync(new ProjectRequest
{
    Name = "Monthly Inspection - February 2025",
    AssetId = "ast_abc123",
    ProjectTypeId = "pt_inspection",
    Status = "Scheduled"
});
```

---

## Inspection Media

### List Inspection Media

**Endpoint**: `GET /api/inspection-media/`

**Authentication**: Required

**Query Parameters**:
- `project_id` - Filter by project ID
- `file_type` - Filter by file type
- `status` - Filter by upload status

**Request**:
```http
GET /api/inspection-media/?project_id=prj_123abc
Authorization: Token hvr_at_abcdef1234567890
```

**Success Response** (200 OK):
```json
{
  "count": 12,
  "next": null,
  "previous": null,
  "results": [
    {
      "id": "med_photo123",
      "project_id": "prj_123abc",
      "file_name": "pump_inspection_01.jpg",
      "file_type": "image/jpeg",
      "file_size": 2457600,
      "upload_url": null,
      "download_url": "https://storage.huvrdata.app/media/prj_123abc/pump_inspection_01.jpg",
      "thumbnail_url": "https://storage.huvrdata.app/thumbnails/prj_123abc/pump_inspection_01_thumb.jpg",
      "preview_url": "https://storage.huvrdata.app/previews/prj_123abc/pump_inspection_01_preview.jpg",
      "status": "UPLOADED",
      "metadata": {
        "camera": "DJI Mavic 3",
        "gps_lat": 40.7128,
        "gps_lon": -74.0060
      },
      "created_at": "2025-01-20T11:30:00Z",
      "updated_at": "2025-01-20T11:35:00Z"
    }
  ]
}
```

**C# Usage**:
```csharp
var media = await client.ListInspectionMediaAsync(new Dictionary<string, string>
{
    { "project_id", "prj_123abc" }
});
```

---

### Create Inspection Media (Part 1 of Upload)

**Endpoint**: `POST /api/inspection-media/`

**Authentication**: Required

**Request**:
```http
POST /api/inspection-media/
Authorization: Token hvr_at_abcdef1234567890
Content-Type: application/json

{
  "project_id": "prj_123abc",
  "file_name": "pump_inspection_02.jpg",
  "file_type": "image/jpeg",
  "file_size": 3145728,
  "metadata": {
    "camera": "DJI Mavic 3",
    "location": "North wall"
  }
}
```

**Success Response** (201 Created):
```json
{
  "id": "med_new789",
  "project_id": "prj_123abc",
  "file_name": "pump_inspection_02.jpg",
  "file_type": "image/jpeg",
  "file_size": 3145728,
  "upload_url": "https://upload.huvrdata.app/v1/upload/xyz123?expires=1706454000",
  "download_url": null,
  "thumbnail_url": null,
  "preview_url": null,
  "status": "PENDING",
  "metadata": {
    "camera": "DJI Mavic 3",
    "location": "North wall"
  },
  "created_at": "2025-01-28T16:10:00Z",
  "updated_at": "2025-01-28T16:10:00Z"
}
```

**Note**: The `upload_url` expires after 15 minutes.

**C# Usage**:
```csharp
var media = await client.CreateInspectionMediaAsync(new InspectionMediaRequest
{
    ProjectId = "prj_123abc",
    FileName = "pump_inspection_02.jpg",
    FileType = "image/jpeg",
    FileSize = fileBytes.Length
});
```

---

### Upload Media File (Part 2 of Upload)

**Endpoint**: `PUT {upload_url}`

**Authentication**: None (URL is pre-signed)

**Request**:
```http
PUT https://upload.huvrdata.app/v1/upload/xyz123?expires=1706454000
Content-Type: image/jpeg
Content-Length: 3145728

[Binary file data]
```

**Success Response** (200 OK):
```
(No response body)
```

**C# Usage**:
```csharp
await client.UploadInspectionMediaFileAsync(
    media.UploadUrl!,
    fileBytes,
    "image/jpeg"
);
```

---

## Checklists

### List Checklists

**Endpoint**: `GET /api/checklists/`

**Authentication**: Required

**Request**:
```http
GET /api/checklists/?project_id=prj_123abc
Authorization: Token hvr_at_abcdef1234567890
```

**Success Response** (200 OK):
```json
{
  "count": 3,
  "next": null,
  "previous": null,
  "results": [
    {
      "id": "chk_safety001",
      "project_id": "prj_123abc",
      "name": "Safety Inspection Checklist",
      "template_id": "tpl_safety_v1",
      "status": "Completed",
      "responses": {
        "visual_inspection": "Pass",
        "leak_check": "Pass",
        "pressure_test": "Pass",
        "notes": "All systems operational"
      },
      "completed_by": "usr_john123",
      "completed_at": "2025-01-20T14:30:00Z",
      "created_at": "2025-01-20T10:00:00Z",
      "updated_at": "2025-01-20T14:30:00Z"
    }
  ]
}
```

**C# Usage**:
```csharp
var checklists = await client.ListChecklistsAsync(new Dictionary<string, string>
{
    { "project_id", "prj_123abc" }
});
```

---

### Create Checklist

**Endpoint**: `POST /api/checklists/`

**Authentication**: Required

**Request**:
```http
POST /api/checklists/
Authorization: Token hvr_at_abcdef1234567890
Content-Type: application/json

{
  "project_id": "prj_123abc",
  "name": "Safety Inspection Checklist",
  "template_id": "tpl_safety_v1",
  "status": "In Progress",
  "responses": {
    "visual_inspection": "Pass",
    "leak_check": "Pending"
  }
}
```

**Success Response** (201 Created):
```json
{
  "id": "chk_new123",
  "project_id": "prj_123abc",
  "name": "Safety Inspection Checklist",
  "template_id": "tpl_safety_v1",
  "status": "In Progress",
  "responses": {
    "visual_inspection": "Pass",
    "leak_check": "Pending"
  },
  "completed_by": null,
  "completed_at": null,
  "created_at": "2025-01-28T16:20:00Z",
  "updated_at": "2025-01-28T16:20:00Z"
}
```

**C# Usage**:
```csharp
var checklist = await client.CreateChecklistAsync(new ChecklistRequest
{
    ProjectId = "prj_123abc",
    Name = "Safety Inspection Checklist",
    TemplateId = "tpl_safety_v1",
    Responses = new Dictionary<string, object>
    {
        { "visual_inspection", "Pass" }
    }
});
```

---

## Defects

### List Defects

**Endpoint**: `GET /api/defects/`

**Authentication**: Required

**Query Parameters**:
- `project_id` - Filter by project
- `asset_id` - Filter by asset
- `severity` - Filter by severity (Low, Medium, High, Critical)
- `status` - Filter by status

**Request**:
```http
GET /api/defects/?severity=High&status=Open
Authorization: Token hvr_at_abcdef1234567890
```

**Success Response** (200 OK):
```json
{
  "count": 5,
  "next": null,
  "previous": null,
  "results": [
    {
      "id": "def_crack001",
      "project_id": "prj_123abc",
      "asset_id": "ast_abc123",
      "title": "Crack in foundation",
      "description": "15cm crack detected in north wall foundation",
      "severity": "High",
      "status": "Open",
      "defect_type": "Structural",
      "location": "North wall, base level",
      "identified_by": "usr_john123",
      "identified_at": "2025-01-20T11:45:00Z",
      "metadata": {
        "length_cm": 15,
        "width_mm": 3,
        "depth_mm": 5
      },
      "created_at": "2025-01-20T12:00:00Z",
      "updated_at": "2025-01-20T12:00:00Z"
    }
  ]
}
```

**C# Usage**:
```csharp
var defects = await client.ListDefectsAsync(new Dictionary<string, string>
{
    { "severity", "High" },
    { "status", "Open" }
});
```

---

### Create Defect

**Endpoint**: `POST /api/defects/`

**Authentication**: Required

**Request**:
```http
POST /api/defects/
Authorization: Token hvr_at_abcdef1234567890
Content-Type: application/json

{
  "project_id": "prj_123abc",
  "asset_id": "ast_abc123",
  "title": "Corrosion on pipe joint",
  "description": "Surface corrosion detected on main pipe joint",
  "severity": "Medium",
  "status": "Open",
  "defect_type": "Corrosion",
  "location": "Main pipe, Joint #3",
  "metadata": {
    "corrosion_type": "Surface",
    "area_cm2": 25
  }
}
```

**Success Response** (201 Created):
```json
{
  "id": "def_new456",
  "project_id": "prj_123abc",
  "asset_id": "ast_abc123",
  "title": "Corrosion on pipe joint",
  "description": "Surface corrosion detected on main pipe joint",
  "severity": "Medium",
  "status": "Open",
  "defect_type": "Corrosion",
  "location": "Main pipe, Joint #3",
  "identified_by": "usr_john123",
  "identified_at": "2025-01-28T16:30:00Z",
  "metadata": {
    "corrosion_type": "Surface",
    "area_cm2": 25
  },
  "created_at": "2025-01-28T16:30:00Z",
  "updated_at": "2025-01-28T16:30:00Z"
}
```

**C# Usage**:
```csharp
var defect = await client.CreateDefectAsync(new DefectRequest
{
    ProjectId = "prj_123abc",
    AssetId = "ast_abc123",
    Title = "Corrosion on pipe joint",
    Severity = "Medium",
    Status = "Open",
    DefectType = "Corrosion"
});
```

---

## Measurements

### List Measurements

**Endpoint**: `GET /api/measurements/`

**Authentication**: Required

**Query Parameters**:
- `project_id` - Filter by project
- `asset_id` - Filter by asset
- `measurement_type` - Filter by type

**Request**:
```http
GET /api/measurements/?project_id=prj_123abc
Authorization: Token hvr_at_abcdef1234567890
```

**Success Response** (200 OK):
```json
{
  "count": 8,
  "next": null,
  "previous": null,
  "results": [
    {
      "id": "meas_ut001",
      "project_id": "prj_123abc",
      "asset_id": "ast_abc123",
      "measurement_type": "Ultrasonic Thickness",
      "value": 12.5,
      "unit": "mm",
      "location": "Point A1",
      "measured_by": "usr_john123",
      "measured_at": "2025-01-20T10:15:00Z",
      "metadata": {
        "temperature_c": 22,
        "humidity_percent": 45,
        "equipment": "UT-1000"
      },
      "created_at": "2025-01-20T10:20:00Z",
      "updated_at": "2025-01-20T10:20:00Z"
    }
  ]
}
```

**C# Usage**:
```csharp
var measurements = await client.ListMeasurementsAsync(new Dictionary<string, string>
{
    { "project_id", "prj_123abc" }
});
```

---

### Create Measurement

**Endpoint**: `POST /api/measurements/`

**Authentication**: Required

**Request**:
```http
POST /api/measurements/
Authorization: Token hvr_at_abcdef1234567890
Content-Type: application/json

{
  "project_id": "prj_123abc",
  "asset_id": "ast_abc123",
  "measurement_type": "Vibration",
  "value": 2.5,
  "unit": "mm/s",
  "location": "Motor bearing",
  "metadata": {
    "frequency": "60 Hz",
    "temperature_c": 45
  }
}
```

**Success Response** (201 Created):
```json
{
  "id": "meas_vib001",
  "project_id": "prj_123abc",
  "asset_id": "ast_abc123",
  "measurement_type": "Vibration",
  "value": 2.5,
  "unit": "mm/s",
  "location": "Motor bearing",
  "measured_by": "usr_john123",
  "measured_at": "2025-01-28T16:40:00Z",
  "metadata": {
    "frequency": "60 Hz",
    "temperature_c": 45
  },
  "created_at": "2025-01-28T16:40:00Z",
  "updated_at": "2025-01-28T16:40:00Z"
}
```

**C# Usage**:
```csharp
var measurement = await client.CreateMeasurementAsync(new MeasurementRequest
{
    ProjectId = "prj_123abc",
    AssetId = "ast_abc123",
    MeasurementType = "Vibration",
    Value = 2.5m,
    Unit = "mm/s",
    Location = "Motor bearing"
});
```

---

## Users

### List Users

**Endpoint**: `GET /api/users/`

**Authentication**: Required

**Request**:
```http
GET /api/users/
Authorization: Token hvr_at_abcdef1234567890
```

**Success Response** (200 OK):
```json
{
  "count": 15,
  "next": null,
  "previous": null,
  "results": [
    {
      "id": "usr_john123",
      "email": "[email protected]",
      "first_name": "John",
      "last_name": "Doe",
      "role": "Inspector",
      "is_active": true,
      "created_at": "2024-01-01T00:00:00Z",
      "updated_at": "2025-01-15T10:00:00Z"
    }
  ]
}
```

**C# Usage**:
```csharp
var users = await client.ListUsersAsync();
Console.WriteLine($"Total users: {users.Count}");
```

---

### Get User Details

**Endpoint**: `GET /api/users/{user_id}/`

**Authentication**: Required

**Request**:
```http
GET /api/users/usr_john123/
Authorization: Token hvr_at_abcdef1234567890
```

**Success Response** (200 OK):
```json
{
  "id": "usr_john123",
  "email": "[email protected]",
  "first_name": "John",
  "last_name": "Doe",
  "role": "Inspector",
  "is_active": true,
  "created_at": "2024-01-01T00:00:00Z",
  "updated_at": "2025-01-15T10:00:00Z"
}
```

**C# Usage**:
```csharp
var user = await client.GetUserAsync("usr_john123");
Console.WriteLine($"User: {user.FirstName} {user.LastName}");
```

---

## Workspaces

### List Workspaces

**Endpoint**: `GET /api/workspaces/`

**Authentication**: Required

**Request**:
```http
GET /api/workspaces/
Authorization: Token hvr_at_abcdef1234567890
```

**Success Response** (200 OK):
```json
{
  "count": 2,
  "next": null,
  "previous": null,
  "results": [
    {
      "id": "ws_main001",
      "name": "ACME Corporation",
      "description": "Main workspace for ACME Corp inspections",
      "is_active": true,
      "created_at": "2024-01-01T00:00:00Z",
      "updated_at": "2024-01-01T00:00:00Z"
    }
  ]
}
```

**C# Usage**:
```csharp
var workspaces = await client.ListWorkspacesAsync();
foreach (var workspace in workspaces.Results)
{
    Console.WriteLine($"Workspace: {workspace.Name}");
}
```

---

### Get Workspace Details

**Endpoint**: `GET /api/workspaces/{workspace_id}/`

**Authentication**: Required

**Request**:
```http
GET /api/workspaces/ws_main001/
Authorization: Token hvr_at_abcdef1234567890
```

**Success Response** (200 OK):
```json
{
  "id": "ws_main001",
  "name": "ACME Corporation",
  "description": "Main workspace for ACME Corp inspections",
  "is_active": true,
  "created_at": "2024-01-01T00:00:00Z",
  "updated_at": "2024-01-01T00:00:00Z"
}
```

**C# Usage**:
```csharp
var workspace = await client.GetWorkspaceAsync("ws_main001");
Console.WriteLine($"Workspace: {workspace.Name}");
```

---

## Error Responses

All endpoints may return the following error responses:

### 400 Bad Request
```json
{
  "detail": "Validation error",
  "errors": {
    "name": ["This field is required."],
    "asset_id": ["Invalid asset ID."]
  }
}
```

### 401 Unauthorized
```json
{
  "detail": "Invalid token or token expired",
  "code": "token_not_valid"
}
```

### 403 Forbidden
```json
{
  "detail": "You do not have permission to perform this action.",
  "code": "permission_denied"
}
```

### 404 Not Found
```json
{
  "detail": "Not found.",
  "code": "not_found"
}
```

### 429 Too Many Requests
```json
{
  "detail": "Request was throttled. Expected available in 60 seconds.",
  "code": "throttled"
}
```

### 500 Internal Server Error
```json
{
  "detail": "Internal server error",
  "code": "server_error"
}
```

---

## Rate Limiting

The HUVR API implements rate limiting to ensure fair usage:

- **Rate Limit**: Varies by endpoint and authentication level
- **Headers**: Response includes rate limit headers
  - `X-RateLimit-Limit`: Total requests allowed
  - `X-RateLimit-Remaining`: Requests remaining
  - `X-RateLimit-Reset`: Unix timestamp when limit resets

**Best Practice**: Implement exponential backoff when receiving 429 responses.

---

## Pagination

All list endpoints return paginated responses:

**Response Structure**:
```json
{
  "count": 150,
  "next": "https://api.huvrdata.app/api/assets/?offset=50&limit=50",
  "previous": "https://api.huvrdata.app/api/assets/?offset=0&limit=50",
  "results": [...]
}
```

**Using Pagination Helpers**:
```csharp
// Get all assets (handles pagination automatically)
var allAssets = await client.GetAllAssetsAsync();

// Get first 100 assets max
var limitedAssets = await client.GetAllAssetsAsync(maxResults: 100);

// Get all active assets
var activeAssets = await client.GetAllAssetsAsync(
    queryParams: new Dictionary<string, string> { { "status", "Active" } }
);
```

---

## Data Gathering Examples

### Gather Complete Project Data
```csharp
var gatherer = new HuvrDataGatherer(client);

// Get all data for a project (asset, media, checklists, defects, measurements)
var projectData = await gatherer.GatherProjectDataAsync("prj_123abc");

Console.WriteLine($"Project: {projectData.Project.Name}");
Console.WriteLine($"Asset: {projectData.Asset?.Name}");
Console.WriteLine($"Media files: {projectData.Media.Count}");
Console.WriteLine($"Checklists: {projectData.Checklists.Count}");
Console.WriteLine($"Defects: {projectData.Defects.Count}");
Console.WriteLine($"Measurements: {projectData.Measurements.Count}");
```

### Gather Asset Data with All Projects
```csharp
var assetData = await gatherer.GatherAssetDataAsync("ast_abc123");

Console.WriteLine($"Asset: {assetData.Asset.Name}");
Console.WriteLine($"Total projects: {assetData.Projects.Count}");

foreach (var projectSnapshot in assetData.ProjectSnapshots)
{
    Console.WriteLine($"  - {projectSnapshot.Project.Name}: " +
                      $"{projectSnapshot.Defects.Count} defects, " +
                      $"{projectSnapshot.Media.Count} media files");
}
```

### Get Defects Summary
```csharp
var defectsSummary = await gatherer.GetDefectsSummaryAsync();

Console.WriteLine($"Total defects: {defectsSummary.TotalDefects}");
Console.WriteLine("By severity:");
foreach (var (severity, count) in defectsSummary.BySeverity)
{
    Console.WriteLine($"  {severity}: {count}");
}
```

### Download Project Media Files
```csharp
var downloadedFiles = await gatherer.DownloadProjectMediaAsync(
    "prj_123abc",
    "./downloads/project_123abc"
);

Console.WriteLine($"Downloaded {downloadedFiles.Count} files");
```

---

## Support

For API access and credentials:
- Contact your HUVR Customer Success representative
- Request service account credentials from your support contact

For technical documentation:
- **API Reference**: https://docs.huvrdata.app/reference
- **OpenAPI Spec**: https://docs.huvrdata.app/openapi/
- **Main Documentation**: https://docs.huvrdata.app

---

*Document Version: 1.0*
*Created: 2025-01-28*
*Based on HUVR Data API v3*
