# HUVR Web App - Quick Start Guide

Get up and running with the HUVR API Web Application in 5 minutes!

## Step 1: Build and Run

```bash
cd HuvrWebApp
dotnet restore
dotnet run
```

The application will start on:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

## Step 2: Login

Open your browser to `https://localhost:5001` and enter your HUVR credentials:

- **Client ID**: `[email protected]`
- **Client Secret**: Your service account secret

Click "Login" to authenticate.

## Step 3: Explore the Dashboard

### Try these quick actions:

**List all assets:**
1. Select "Assets" from Entity Type
2. Select "List All" from Method
3. Click "Execute API Call"

**Get a specific project:**
1. Select "Projects" from Entity Type
2. Select "Get by ID" from Method
3. Enter a project ID
4. Click "Execute API Call"

## Step 4: Export to Excel

1. Click "Excel Export" in the navigation menu
2. Select an entity type (e.g., "Assets")
3. Click "Load Available Fields"
4. Customize the Excel column names:
   - Check/uncheck fields to include/exclude
   - Edit column names (or leave blank for defaults)
5. Click "Export to Excel"
6. Your file will download automatically!

## Example: Export Assets to Excel

1. Go to Excel Export page
2. Select "Assets"
3. Click "Load Available Fields"
4. The default mappings appear:
   ```
   Id → "Id"
   Name → "Name"
   Description → "Description"
   AssetType → "Asset Type"
   Location → "Location"
   Status → "Status"
   CreatedAt → "Created At"
   UpdatedAt → "Updated At"
   ```
5. Customize as needed (e.g., change "Id" to "Asset ID")
6. Click "Export to Excel"
7. Open the downloaded file in Excel!

## Supported Entity Types

- **Assets** - Physical assets being inspected
- **Projects** - Work orders and inspection projects
- **Defects** - Findings and issues discovered
- **Measurements** - Quantitative measurements taken
- **Inspection Media** - Photos, videos, and files
- **Checklists** - Digital forms and checklists
- **Users** - User accounts
- **Workspaces** - Workspace information

## Common Use Cases

### Use Case 1: Export All Defects with Custom Fields

```
1. Excel Export → Select "Defects"
2. Load Available Fields
3. Customize columns:
   - Title → "Issue Title"
   - Severity → "Priority Level"
   - Status → "Current Status"
   - IdentifiedAt → "Discovery Date"
4. Export to Excel
```

### Use Case 2: Get Project Details

```
1. Dashboard → Select "Projects"
2. Select "Get by ID"
3. Enter project ID (e.g., "proj_123abc")
4. Execute API Call
5. View full project details in JSON
```

### Use Case 3: List All Measurements

```
1. Dashboard → Select "Measurements"
2. Select "List All"
3. Execute API Call
4. Review all measurements in JSON format
```

## Tips & Tricks

- **Select All**: Use the checkbox in the table header to quickly select/deselect all fields
- **Reset Mappings**: Click "Reset" to restore default field mappings
- **Clear Results**: Use "Clear Results" on the dashboard to clean up the view
- **Auto-fit Columns**: Excel exports automatically adjust column widths

## Troubleshooting

**Can't login?**
- Double-check your Client ID format ([email protected])
- Verify your Client Secret is correct
- Ensure you have internet connectivity

**No data in export?**
- The entity type might be empty
- Check your API permissions
- Try listing data on the Dashboard first

**Session expired?**
- Sessions last 60 minutes
- Simply logout and login again

## What's Next?

- Explore all entity types
- Experiment with different field mappings
- Use the Excel exports for reporting and analysis
- Integrate the exported data with your workflows

## Need Help?

- See full documentation in `README.md`
- Check HUVR API docs in `/HuvrApiClient/README.md`
- Review API endpoints in `/HUVR_API_ENDPOINTS_REFERENCE.md`

Happy exploring! 🚀
