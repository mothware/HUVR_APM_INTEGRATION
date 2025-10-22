# GE APM On-Premises Client for C#

A comprehensive C# client library for GE APM On-Premises API. This library provides easy-to-use methods for authentication, TML (Thickness Measurement Location) operations, and generic OData queries.

## Features

- **Simple Authentication**: Login with username/password to obtain Meridium token
- **MeridiumToken Header**: Automatic authentication with format "sessionid;Timezonename"
- **Timezone Support**: Configure timezone for all API calls
- **TML Operations**: Complete CRUD operations for Thickness Measurement Locations and measurements
- **Genome Query API**: Execute GE APM proprietary queries with parameters
- **Generic Query API**: Flexible OData querying with path and parameter arrays
- **OData Support**: Full OData query options ($filter, $select, $expand, $orderby, etc.)
- **Batch Operations**: Load multiple TML measurements efficiently
- **Type-Safe Models**: Strongly-typed C# models for all entities
- **Async/Await**: Modern async patterns throughout

## Installation

Add the GeApmClient files to your project:

```
GeApmClient/
├── GeApmClient.cs
├── Models/
│   ├── AuthenticationModels.cs
│   ├── EntityModels.cs
│   ├── TmlModels.cs
│   └── QueryModels.cs
└── Examples/
    └── UsageExamples.cs
```

## Prerequisites

- .NET 6.0 or higher
- GE APM On-Premises installation
- Valid user credentials

## Quick Start

### 1. Login with Timezone

```csharp
using GeApmClient;

// Initialize with timezone (default: UTC)
using var client = new GeApmClient("https://apm.company.com", "America/New_York");

// Authenticate - MeridiumToken header is automatically set to: "{sessionid};America/New_York"
var loginResponse = await client.LoginAsync("username", "password");
Console.WriteLine($"Logged in as {loginResponse.UserName}");
```

**Important**: The timezone parameter is required for proper MeridiumToken header format. The client automatically sets the `MeridiumToken` header to `{sessionid};{timezone}` for all authenticated requests.

### 2. Query TMLs

```csharp
// Get all active TMLs
var tmls = await client.GetTmlsAsync(new QueryParameters
{
    Filter = "Status eq 'Active'",
    OrderBy = "TML_ID asc",
    Top = 100
});

foreach (var tml in tmls.Items)
{
    Console.WriteLine($"{tml.TmlId}: {tml.Description}");
}
```

### 3. Load TML Measurements

```csharp
// Create a new measurement
var measurement = new TmlMeasurementRequest
{
    TmlId = "TML-001",
    MeasurementDate = DateTime.UtcNow,
    MeasuredThickness = 12.5m,
    MeasurementTakenBy = "John Doe",
    InspectionId = "INS-2025-001",
    Status = "Active"
};

var result = await client.LoadTmlMeasurementAsync(measurement);
Console.WriteLine($"Measurement created: {result.EntityKey}");
```

### 4. Genome Query API

```csharp
// Execute Genome Query (GE APM proprietary query language)
var parameters = new Dictionary<string, string>
{
    { "startDate", "2025-01-01" },
    { "endDate", "2025-01-31" },
    { "status", "Active" }
};

var result = await client.ExecuteGenomeQueryAsync("reports/tml-summary", parameters);
Console.WriteLine("Query Result:");
Console.WriteLine(result);
```

### 5. Generic Query with Path and Parameters

```csharp
// Query any family with custom parameters
var equipment = await client.QueryAsync<dynamic>(
    "Equipment",
    new QueryParameters
    {
        Filter = "Status eq 'Active'",
        Select = "EntityKey,Name,Status,Location",
        Top = 50
    });

Console.WriteLine($"Found {equipment.Items.Count} equipment items");
```

## API Reference

### Authentication

#### LoginAsync

Authenticates with GE APM and obtains a Meridium token.

```csharp
public async Task<LoginResponse> LoginAsync(
    string username,
    string password,
    CancellationToken cancellationToken = default)
```

**Example**:
```csharp
var response = await client.LoginAsync("user@company.com", "password");
```

---

### TML Operations

#### GetTmlsAsync

Gets all Thickness Measurement Locations with optional filtering.

```csharp
public async Task<QueryResult<ThicknessMeasurementLocation>> GetTmlsAsync(
    QueryParameters? parameters = null,
    CancellationToken cancellationToken = default)
```

**Example**:
```csharp
var tmls = await client.GetTmlsAsync(new QueryParameters
{
    Filter = "Status eq 'Active' and Corrosion_Rate gt 0.5",
    OrderBy = "Corrosion_Rate desc",
    Top = 100
});
```

#### GetTmlByIdAsync

Gets a specific TML by TML ID.

```csharp
public async Task<ThicknessMeasurementLocation?> GetTmlByIdAsync(
    string tmlId,
    CancellationToken cancellationToken = default)
```

**Example**:
```csharp
var tml = await client.GetTmlByIdAsync("TML-001");
```

#### GetTmlMeasurementsAsync

Gets all measurements for a specific TML.

```csharp
public async Task<QueryResult<TmlMeasurement>> GetTmlMeasurementsAsync(
    string tmlId,
    QueryParameters? parameters = null,
    CancellationToken cancellationToken = default)
```

**Example**:
```csharp
var measurements = await client.GetTmlMeasurementsAsync(
    "TML-001",
    new QueryParameters
    {
        OrderBy = "Measurement_Date desc",
        Top = 10
    });
```

#### GetMultipleTmlMeasurementsAsync

Gets measurements for multiple TMLs in a single query.

```csharp
public async Task<Dictionary<string, List<TmlMeasurement>>> GetMultipleTmlMeasurementsAsync(
    string[] tmlIds,
    QueryParameters? parameters = null,
    CancellationToken cancellationToken = default)
```

**Example**:
```csharp
var tmlIds = new[] { "TML-001", "TML-002", "TML-003" };
var measurementsByTml = await client.GetMultipleTmlMeasurementsAsync(tmlIds);

foreach (var (tmlId, measurements) in measurementsByTml)
{
    Console.WriteLine($"{tmlId}: {measurements.Count} measurements");
}
```

#### LoadTmlMeasurementAsync

Loads/creates a new TML measurement.

```csharp
public async Task<TmlMeasurement> LoadTmlMeasurementAsync(
    TmlMeasurementRequest measurement,
    CancellationToken cancellationToken = default)
```

**Example**:
```csharp
var measurement = new TmlMeasurementRequest
{
    TmlId = "TML-001",
    MeasurementDate = DateTime.UtcNow,
    MeasuredThickness = 12.5m,
    MeasurementTakenBy = "Inspector Name",
    Comments = "Normal reading",
    Status = "Active"
};

var result = await client.LoadTmlMeasurementAsync(measurement);
```

#### LoadTmlMeasurementsBatchAsync

Loads multiple TML measurements in batch.

```csharp
public async Task<List<(TmlMeasurementRequest Request, TmlMeasurement? Result, Exception? Error)>>
    LoadTmlMeasurementsBatchAsync(
        List<TmlMeasurementRequest> measurements,
        CancellationToken cancellationToken = default)
```

**Example**:
```csharp
var measurements = new List<TmlMeasurementRequest>
{
    new() { TmlId = "TML-001", MeasuredThickness = 12.5m, ... },
    new() { TmlId = "TML-002", MeasuredThickness = 15.2m, ... },
    new() { TmlId = "TML-003", MeasuredThickness = 10.8m, ... }
};

var results = await client.LoadTmlMeasurementsBatchAsync(measurements);

foreach (var (request, result, error) in results)
{
    if (error == null)
        Console.WriteLine($"✓ {request.TmlId}: Success");
    else
        Console.WriteLine($"✗ {request.TmlId}: {error.Message}");
}
```

#### UpdateTmlMeasurementAsync

Updates an existing TML measurement.

```csharp
public async Task<TmlMeasurement> UpdateTmlMeasurementAsync(
    long entityKey,
    Dictionary<string, object?> updates,
    CancellationToken cancellationToken = default)
```

**Example**:
```csharp
var updates = new Dictionary<string, object?>
{
    { "Comments", "Updated comment" },
    { "Temperature", 23.5m }
};

await client.UpdateTmlMeasurementAsync(12345, updates);
```

---

### Generic Query API

#### QueryAsync

Executes a generic OData query with path and parameters.

```csharp
public async Task<QueryResult<T>> QueryAsync<T>(
    string path,
    QueryParameters? parameters = null,
    CancellationToken cancellationToken = default)
```

**Parameters**:
- `path`: OData entity path (e.g., "Equipment", "Work_History", "Thickness_Measurement_Location")
- `parameters`: Query parameters object with filter, select, expand, etc.

**Example**:
```csharp
// Query Equipment
var equipment = await client.QueryAsync<dynamic>(
    "Equipment",
    new QueryParameters
    {
        Filter = "Status eq 'Active'",
        Select = "EntityKey,Name,Location",
        OrderBy = "Name asc",
        Top = 100
    });

// Query Work History
var workOrders = await client.QueryAsync<dynamic>(
    "Work_History",
    new QueryParameters
    {
        Filter = "Status eq 'Open'",
        Expand = "Equipment",
        Top = 50
    });
```

#### QueryRawAsync

Executes a raw OData query and returns JSON string.

```csharp
public async Task<string> QueryRawAsync(
    string path,
    Dictionary<string, string>? parameters = null,
    CancellationToken cancellationToken = default)
```

**Example**:
```csharp
var parameters = new Dictionary<string, string>
{
    { "$filter", "Corrosion_Rate gt 0.5" },
    { "$select", "TML_ID,Corrosion_Rate" },
    { "$top", "25" }
};

var json = await client.QueryRawAsync("Thickness_Measurement_Location", parameters);
```

#### GetCountAsync

Gets the count of entities in a family.

```csharp
public async Task<int> GetCountAsync(
    string familyKey,
    string? filter = null,
    CancellationToken cancellationToken = default)
```

**Example**:
```csharp
// Total count
var total = await client.GetCountAsync("Thickness_Measurement_Location");

// Count with filter
var active = await client.GetCountAsync(
    "Thickness_Measurement_Location",
    "Status eq 'Active'");

Console.WriteLine($"Active TMLs: {active} of {total}");
```

---

### Genome Query API

The Genome Query API allows execution of GE APM's proprietary query language for custom reports and data extraction.

#### ExecuteGenomeQueryAsync

Executes a Genome Query with path and parameters, returns raw JSON.

```csharp
public async Task<string> ExecuteGenomeQueryAsync(
    string queryPath,
    Dictionary<string, string>? parameters = null,
    CancellationToken cancellationToken = default)
```

**Parameters**:
- `queryPath`: Query path or query identifier (e.g., "reports/tml-summary", "queries/equipment-status")
- `parameters`: Dictionary of query parameters as key-value pairs

**Example**:
```csharp
var parameters = new Dictionary<string, string>
{
    { "startDate", "2025-01-01" },
    { "endDate", "2025-01-31" },
    { "equipmentId", "VESSEL-001" }
};

var result = await client.ExecuteGenomeQueryAsync("reports/inspection-summary", parameters);
Console.WriteLine(result);
```

#### ExecuteGenomeQueryAsync&lt;T&gt;

Executes a Genome Query with typed result deserialization.

```csharp
public async Task<T?> ExecuteGenomeQueryAsync<T>(
    string queryPath,
    Dictionary<string, string>? parameters = null,
    CancellationToken cancellationToken = default)
```

**Example**:
```csharp
var result = await client.ExecuteGenomeQueryAsync<List<TmlSummary>>(
    "queries/tml-summary",
    new Dictionary<string, string>
    {
        { "location", "Building A" }
    });

foreach (var item in result)
{
    Console.WriteLine($"{item.TmlId}: {item.Status}");
}
```

#### ExecuteGenomeQueryByIdAsync

Executes a saved query by ID with parameter array.

```csharp
public async Task<string> ExecuteGenomeQueryByIdAsync(
    string queryId,
    string[]? parameterValues = null,
    CancellationToken cancellationToken = default)
```

**Parameters**:
- `queryId`: Query ID from GE APM catalog (e.g., "Q12345")
- `parameterValues`: Array of parameter values (positional parameters)

**Example**:
```csharp
// Execute query Q12345 with 3 parameters
var parameterValues = new[] { "TML-001", "2025-01-01", "2025-01-31" };
var result = await client.ExecuteGenomeQueryByIdAsync("Q12345", parameterValues);
```

#### ExecuteGenomeQueryPostAsync

Executes a POST-based Genome Query with complex parameters.

```csharp
public async Task<string> ExecuteGenomeQueryPostAsync(
    string queryPath,
    object queryRequest,
    CancellationToken cancellationToken = default)
```

**Example**:
```csharp
var queryRequest = new
{
    QueryId = "ComplexReport",
    Parameters = new
    {
        EquipmentIds = new[] { "VESSEL-001", "VESSEL-002" },
        DateRange = new
        {
            Start = "2025-01-01T00:00:00Z",
            End = "2025-01-31T23:59:59Z"
        }
    }
};

var result = await client.ExecuteGenomeQueryPostAsync("queries/complex", queryRequest);
```

---

### Query Parameters

The `QueryParameters` class supports all OData query options:

```csharp
var parameters = new QueryParameters
{
    Filter = "Status eq 'Active' and Priority gt 3",  // $filter
    Select = "EntityKey,Name,Status",                 // $select
    Expand = "RelatedEntity",                         // $expand (depth: 1)
    OrderBy = "Name asc",                             // $orderby
    Top = 100,                                        // $top
    Skip = 200,                                       // $skip (pagination)
    Count = true                                      // $count
};
```

### OData Filter Operators

**Comparison**:
- `eq` - Equal: `Status eq 'Active'`
- `ne` - Not equal: `Status ne 'Retired'`
- `gt` - Greater than: `Corrosion_Rate gt 0.5`
- `ge` - Greater than or equal: `Thickness ge 10.0`
- `lt` - Less than: `Priority lt 5`
- `le` - Less than or equal: `Reading le 100`

**Logical**:
- `and` - And: `Status eq 'Active' and Priority gt 3`
- `or` - Or: `Status eq 'Active' or Status eq 'Pending'`

**Arithmetic**:
- `add` - Add: `Thickness add 5 gt 20`
- `sub` - Subtract: `Nominal_Thickness sub Minimum_Thickness lt 2`
- `mul` - Multiply: `Rate mul 2 gt 1`
- `div` - Divide: `Total div Count gt 10`

**String Functions**:
- `contains`: `contains(Description, 'pump')`
- `startswith`: `startswith(TML_ID, 'TML-')`
- `endswith`: `endswith(Location, 'Floor 1')`

---

### Entity Operations

#### CreateEntityAsync

Creates a new entity in any family.

```csharp
public async Task<EntityDto> CreateEntityAsync(
    EntityDto entity,
    CancellationToken cancellationToken = default)
```

**Example**:
```csharp
var entity = new EntityDto
{
    EntityKey = 0,  // Must be 0 for INSERT
    FamilyKey = "Equipment",
    Properties = new Dictionary<string, object?>
    {
        { "Name", "New Pump" },
        { "Status", "Active" },
        { "Location", "Building A" }
    }
};

var created = await client.CreateEntityAsync(entity);
```

#### UpdateEntityAsync

Updates an existing entity (partial update).

```csharp
public async Task<EntityDto> UpdateEntityAsync(
    string familyKey,
    long entityKey,
    Dictionary<string, object?> updates,
    CancellationToken cancellationToken = default)
```

**Example**:
```csharp
var updates = new Dictionary<string, object?>
{
    { "Status", "Maintenance" },
    { "Location", "Building B" }
};

await client.UpdateEntityAsync("Equipment", 12345, updates);
```

#### DeleteEntityAsync

Deletes an entity.

```csharp
public async Task DeleteEntityAsync(
    string familyKey,
    long entityKey,
    CancellationToken cancellationToken = default)
```

**Example**:
```csharp
await client.DeleteEntityAsync("Equipment", 12345);
```

---

## Common Scenarios

### Scenario 1: Load Daily TML Measurements

```csharp
using var client = new GeApmClient("https://apm.company.com");
await client.LoginAsync("username", "password");

// Load measurements from CSV or other source
var measurements = ReadMeasurementsFromFile("daily_readings.csv");

var requests = measurements.Select(m => new TmlMeasurementRequest
{
    TmlId = m.TmlId,
    MeasurementDate = m.Date,
    MeasuredThickness = m.Thickness,
    MeasurementTakenBy = m.Inspector,
    Status = "Active"
}).ToList();

// Batch load
var results = await client.LoadTmlMeasurementsBatchAsync(requests);

Console.WriteLine($"Loaded {results.Count(r => r.Error == null)} measurements");
```

### Scenario 2: Extract TML Data for Reporting

```csharp
using var client = new GeApmClient("https://apm.company.com");
await client.LoginAsync("username", "password");

// Get all TMLs for an asset
var tmls = await client.QueryAsync<ThicknessMeasurementLocation>(
    "Thickness_Measurement_Location",
    new QueryParameters
    {
        Filter = "Equipment_ID eq 'VESSEL-001'",
        Select = "TML_ID,Description,Nominal_Thickness,Minimum_Thickness,Corrosion_Rate",
        OrderBy = "Location asc"
    });

// Get latest measurement for each TML
var tmlIds = tmls.Items.Select(t => t.TmlId!).ToArray();
var measurementsByTml = await client.GetMultipleTmlMeasurementsAsync(
    tmlIds,
    new QueryParameters
    {
        OrderBy = "Measurement_Date desc",
        Top = 1000
    });

// Generate report
foreach (var tml in tmls.Items)
{
    var measurements = measurementsByTml.GetValueOrDefault(tml.TmlId!, new List<TmlMeasurement>());
    var latest = measurements.OrderByDescending(m => m.MeasurementDate).FirstOrDefault();

    Console.WriteLine($"{tml.TmlId}: Latest = {latest?.MeasuredThickness}mm on {latest?.MeasurementDate:yyyy-MM-dd}");
}
```

### Scenario 3: Query Multiple Families

```csharp
using var client = new GeApmClient("https://apm.company.com");
await client.LoginAsync("username", "password");

// Query Equipment
var equipment = await client.QueryAsync<dynamic>(
    "Equipment",
    new QueryParameters { Filter = "Status eq 'Active'", Top = 100 });

// Query Functional Locations
var locations = await client.QueryAsync<dynamic>(
    "Functional_Location",
    new QueryParameters { Filter = "Status eq 'Active'", Top = 100 });

// Query Work History
var workOrders = await client.QueryAsync<dynamic>(
    "Work_History",
    new QueryParameters { Filter = "Status eq 'Open'", Top = 50 });

Console.WriteLine($"Equipment: {equipment.Items.Count}");
Console.WriteLine($"Locations: {locations.Items.Count}");
Console.WriteLine($"Work Orders: {workOrders.Items.Count}");
```

### Scenario 4: Paginate Through Large Datasets

```csharp
using var client = new GeApmClient("https://apm.company.com");
await client.LoginAsync("username", "password");

int pageSize = 100;
int skip = 0;
var allMeasurements = new List<TmlMeasurement>();

while (true)
{
    var page = await client.QueryAsync<TmlMeasurement>(
        "TML_Measurement",
        new QueryParameters
        {
            Filter = "Measurement_Date ge 2025-01-01T00:00:00Z",
            OrderBy = "Measurement_Date desc",
            Top = pageSize,
            Skip = skip
        });

    allMeasurements.AddRange(page.Items);

    if (page.Items.Count < pageSize)
        break;

    skip += pageSize;
}

Console.WriteLine($"Retrieved {allMeasurements.Count} total measurements");
```

---

## Best Practices

### 1. Use `using` Statement

Always dispose of the client properly:

```csharp
using var client = new GeApmClient("https://apm.company.com");
// Client will be disposed automatically
```

### 2. Check Authentication

```csharp
if (!client.IsAuthenticated)
{
    await client.LoginAsync("username", "password");
}
```

### 3. Handle Errors

```csharp
try
{
    var tml = await client.GetTmlByIdAsync("TML-001");
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    Console.WriteLine("TML not found");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"API error: {ex.Message}");
}
```

### 4. Use Filters Server-Side

```csharp
// Good - filter on server
var tmls = await client.GetTmlsAsync(new QueryParameters
{
    Filter = "Status eq 'Active'"
});

// Bad - filter on client
var allTmls = await client.GetTmlsAsync();
var active = allTmls.Items.Where(t => t.Status == "Active");
```

### 5. Batch Operations

```csharp
// Good - batch load
var results = await client.LoadTmlMeasurementsBatchAsync(measurements);

// Bad - individual loads
foreach (var m in measurements)
{
    await client.LoadTmlMeasurementAsync(m);  // Too many API calls!
}
```

### 6. Select Only Needed Fields

```csharp
// Good - select specific fields
var tmls = await client.GetTmlsAsync(new QueryParameters
{
    Select = "TML_ID,Description,Status"
});

// Bad - retrieve all fields
var tmls = await client.GetTmlsAsync();
```

---

## Family Keys Reference

Common GE APM family keys:

- `Thickness_Measurement_Location` - TML entities
- `TML_Measurement` - TML readings
- `Equipment` - Equipment entities
- `Functional_Location` - Location hierarchy
- `Work_History` - Work orders
- `Work_History_Detail` - Work order tasks
- `Inspection` - Inspection records
- `Component` - Asset components

---

## Support

For issues with:
- **GE APM API**: Contact your GE Vernova support representative
- **This Client Library**: See repository issues

---

## Version

- **GE APM API**: On-Premises v5.22 compatible
- **Client Version**: 1.0.0
- **Last Updated**: 2025-01-15

---

## License

This client library is provided for use with GE APM On-Premises installations.
