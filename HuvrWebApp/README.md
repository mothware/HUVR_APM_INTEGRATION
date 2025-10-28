# HUVR API Web Application

A modern ASP.NET Core MVC web application that provides a user-friendly interface for interacting with the HUVR API and exporting data to Excel with customizable field mappings.

## Features

### 1. Secure Authentication
- Login page with service account credentials (ClientId/ClientSecret)
- Session-based authentication with automatic token management
- Secure token storage and automatic refresh

### 2. API Dashboard
- Interactive interface to call HUVR API methods
- Support for all major entities:
  - Assets
  - Projects (Work Orders)
  - Defects (Findings)
  - Measurements
  - Inspection Media
  - Checklists (Digital Forms)
  - Users
  - Workspaces
- List all records or get specific records by ID
- Real-time JSON response viewer

### 3. Excel Export with Field Mapping
- Select any entity type to export
- Visual field mapping interface
- Customize Excel column names for each API field
- Select/deselect fields to include in export
- Automatic data fetching and Excel generation
- Download Excel files directly from browser

## Project Structure

```
HuvrWebApp/
├── Controllers/
│   ├── AuthController.cs         # Authentication and login
│   ├── DashboardController.cs    # API method execution
│   └── ExcelController.cs        # Excel export and field mapping
├── Models/
│   ├── LoginViewModel.cs         # Login form model
│   └── FieldMappingViewModel.cs  # Field mapping models
├── Services/
│   ├── IHuvrService.cs          # Service interface
│   └── HuvrService.cs           # Session-based API client management
├── Views/
│   ├── Auth/
│   │   └── Login.cshtml         # Login page
│   ├── Dashboard/
│   │   └── Index.cshtml         # API methods dashboard
│   ├── Excel/
│   │   └── FieldMapping.cshtml  # Field mapping interface
│   └── Shared/
│       ├── _Layout.cshtml       # Main layout
│       └── _ValidationScriptsPartial.cshtml
├── wwwroot/
│   └── css/
│       └── site.css             # Custom styles
├── Program.cs                    # Application startup
├── appsettings.json             # Configuration
└── HuvrWebApp.csproj            # Project file
```

## Prerequisites

- .NET 6.0 SDK or higher
- HUVR API service account credentials
- Internet connection to access HUVR API

## Installation

1. **Navigate to the project directory:**
   ```bash
   cd HuvrWebApp
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the project:**
   ```bash
   dotnet build
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

5. **Open in browser:**
   ```
   https://localhost:5001
   or
   http://localhost:5000
   ```

## Configuration

Edit `appsettings.json` to customize the HUVR API base URL:

```json
{
  "HuvrApi": {
    "BaseUrl": "https://api.huvrdata.app"
  }
}
```

## Usage Guide

### 1. Login

1. Navigate to the application URL
2. Enter your HUVR service account credentials:
   - **Client ID**: Your service account email (e.g., `[email protected]`)
   - **Client Secret**: Your service account secret key
3. Click "Login"
4. You'll be redirected to the dashboard upon successful authentication

### 2. Using the API Dashboard

1. **Select Entity Type**: Choose from Assets, Projects, Defects, etc.
2. **Select Method**:
   - **List All**: Retrieve all records for the entity type
   - **Get by ID**: Retrieve a specific record (requires ID)
3. **Enter ID** (if using Get by ID method)
4. **Click "Execute API Call"**
5. View the JSON response in the results panel

### 3. Exporting to Excel

1. Navigate to "Excel Export" from the menu
2. **Select Entity Type**: Choose what data to export
3. **Click "Load Available Fields"**
4. **Customize Field Mappings**:
   - Check/uncheck fields to include/exclude
   - Edit Excel column names (or leave blank to use API field name)
   - Use "Select All" checkbox to toggle all fields
5. **Click "Export to Excel"**
6. The Excel file will be automatically downloaded

### Field Mapping Examples

| API Field    | Excel Column Name | Result in Excel |
|--------------|-------------------|-----------------|
| Id           | Asset ID          | "Asset ID"      |
| Name         | Asset Name        | "Asset Name"    |
| CreatedAt    | Created Date      | "Created Date"  |
| Status       | *(leave blank)*   | "Status"        |

## API Entities and Fields

### Assets
- Id, Name, Description, AssetType, Location, Status, CreatedAt, UpdatedAt

### Projects
- Id, Name, Description, AssetId, ProjectTypeId, Status, StartDate, EndDate

### Defects
- Id, ProjectId, AssetId, Title, Description, Severity, Status, DefectType, Location, IdentifiedBy, IdentifiedAt

### Measurements
- Id, ProjectId, AssetId, MeasurementType, Value, Unit, Location, MeasuredBy, MeasuredAt

### Inspection Media
- Id, ProjectId, FileName, FileType, FileSize, Status, DownloadUrl, ThumbnailUrl

### Checklists
- Id, ProjectId, Name, TemplateId, Status, CompletedBy, CompletedAt

### Users
- Id, Email, FirstName, LastName, Role

### Workspaces
- Id, Name, Description

## Technical Details

### Dependencies

- **ASP.NET Core MVC** 6.0 - Web framework
- **Newtonsoft.Json** - JSON serialization
- **ClosedXML** - Excel file generation
- **Bootstrap 5** - UI framework
- **jQuery** - Client-side interactions

### Session Management

- Sessions expire after 60 minutes of inactivity
- API client is stored per session for security
- Automatic cleanup on logout

### Security Features

- Session-based authentication
- Secure credential handling
- Automatic token refresh
- No credentials stored in cookies or localStorage

### Excel Export Process

1. User configures field mappings
2. Application fetches data from HUVR API using pagination
3. Data is mapped to Excel columns based on user configuration
4. Excel workbook is generated using ClosedXML
5. File is streamed to browser for download

## Troubleshooting

### Login Issues

- **Error: "Login failed: Unauthorized"**
  - Verify your Client ID and Client Secret are correct
  - Ensure your service account is active
  - Check that the API base URL is correct

### API Call Issues

- **Error: "Not authenticated"**
  - Your session may have expired
  - Log out and log back in

### Excel Export Issues

- **Error: "No data available"**
  - The selected entity type may have no records
  - Check your API access permissions

## Development

### Adding New Entity Types

1. Add the entity to `GetFieldsForEntityType()` in `ExcelController.cs`
2. Add the case to `FetchDataForEntityType()` in `ExcelController.cs`
3. Add the entity to dropdown in views
4. Add execution method in `DashboardController.cs`

### Customizing the UI

- Edit `wwwroot/css/site.css` for custom styles
- Modify `Views/Shared/_Layout.cshtml` for layout changes
- Update Bootstrap version in `_Layout.cshtml` if needed

## API Documentation

For complete HUVR API documentation, refer to:
- `/HuvrApiClient/README.md` - Client library usage
- `/HUVR_API_DEFINITION.md` - API specification
- `/HUVR_API_ENDPOINTS_REFERENCE.md` - Endpoint reference

## License

This application is part of the HUVR API Integration project.

## Support

For issues or questions:
1. Check the HUVR API documentation
2. Review the HuvrApiClient README
3. Contact your HUVR account administrator
