# GE APM API Specification Document

## Table of Contents
1. [Overview](#overview)
2. [Platform Versions](#platform-versions)
3. [API Types](#api-types)
4. [Authentication](#authentication)
5. [Custom Development API (OData)](#custom-development-api-odata)
6. [Simple Ingestion API v2](#simple-ingestion-api-v2)
7. [Asset API (Predix APM)](#asset-api-predix-apm)
8. [Data Models and Families](#data-models-and-families)
9. [Error Handling](#error-handling)
10. [Best Practices](#best-practices)
11. [Common Use Cases](#common-use-cases)

---

## Overview

GE Vernova APM (Asset Performance Management) provides multiple REST APIs for accessing and managing asset data, inspections, work orders, and analytics. The platform supports both on-premises and cloud deployments with different authentication and API patterns.

**Primary Documentation Sources**:
- On-Premises APM: https://www.gevernova.com/software/documentation/onpremises-apm/
- Cloud APM: https://www.ge.com/digital/documentation/cloud-apm/
- Predix APM: https://www.ge.com/digital/documentation/predix-apm/

**API Platform**: RESTful APIs with OData support

**Data Format**: JSON (primary), XML (supported)

---

## Platform Versions

### Available Versions
- **v5.22** (Latest On-Premises APM release as of 2025)
- **v5.11**
- **v5.07**
- **v5.0**
- **Predix APM** (Cloud-based, various versions)
- **APM Classic** (v4.5, v4.6.x)

**Note**: Version 5.3 documentation referenced in original URLs may use alternate version numbering (e.g., v5.22).

---

## API Types

GE APM offers three primary API categories:

### 1. Custom Development API (OData)
- Transaction-based interface for Core families
- Full CRUD operations on entities and relationships
- OData v3/v4 compliant
- Primary use: Direct database operations

### 2. Simple Ingestion API v2
- Asynchronous bulk data ingestion
- Works with all data loaders
- Bundle-based submission
- Primary use: Large-scale data imports

### 3. Asset API (Predix APM)
- Cloud-native asset management
- S95 hierarchy support (Enterprise, Site, Segment, Asset)
- Tag and classification management
- Primary use: Asset modeling and hierarchy

---

## Authentication

### On-Premises APM Authentication

#### Meridium Token Authentication

**Endpoint**: POST to Login API

**Process**:
1. Make HTTP POST call to the Login API
2. Use your credentials to generate a Meridium token
3. Include token in subsequent requests

**Token Usage**:
```
Authorization: Bearer {meridium_token}
```

**Base URL Format**: `{{host}}/meridium/api/`

**Requirements**:
- All OData requests must be authenticated
- Tokens are session-based
- Subject to standard WebApi authentication constraints

### Predix APM Authentication (Cloud)

#### UAA OAuth2 Authentication

**Token Endpoint**:
```
https://<YOUR_UAA_ID>.predix-uaa.run.aws-usw02-pr.ice.predix.io/oauth/token
```

**Grant Types Supported**:
- `password` - User credentials
- `client_credentials` - Service account (no refresh token)
- `authorization_code` - OAuth flow
- `refresh_token` - Token refresh

**Obtaining Token** (Password Grant):

**Request**:
```http
POST /oauth/token?grant_type=password&username=<username>&password=<password>
Authorization: Basic <Base64(client_id:client_secret)>
```

**Response**:
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsI...",
  "token_type": "bearer",
  "expires_in": 3600,
  "scope": "openid",
  "refresh_token": "eyJhbGciOiJSUzI1NiIsI..."
}
```

**Using Token**:
```
Authorization: Bearer {access_token}
```

**Important Notes**:
- Client credentials grant does NOT return refresh_token
- Tokens typically expire after 3600 seconds (1 hour)
- APM Asset microservices use Predix UAA service

---

## Custom Development API (OData)

### Base URL

```
{{host}}/meridium/api/odata
```

### Supported HTTP Methods

- **GET** - Retrieve entities, relationships, metadata
- **POST** - Create new entities
- **PUT** - Update existing entities (full update)
- **PATCH** - Update existing entities (partial update)
- **DELETE** - Delete entities

### OData Query Options

The following OData system query options are supported on root entities:

#### $metadata
Get schema information
```
GET {{host}}/meridium/api/odata/$metadata
```

#### $count
Get count of entities in a family
```
GET {{host}}/meridium/api/odata/Work_Order/$count
```

#### $select (Depth: 1)
Select specific properties
```
GET {{host}}/meridium/api/odata/Equipment?$select=Name,Status
```

#### $filter
Filter entities
```
GET {{host}}/meridium/api/odata/Equipment?$filter=Status eq 'Active'
```

**Supported Binary Operations**:
- `Add`, `Subtract`, `Multiply`, `Divide`
- `Equal` (eq), `NotEqual` (ne)
- `GreaterThan` (gt), `GreaterThanOrEqual` (ge)
- `LessThan` (lt), `LessThanOrEqual` (le)
- `And`, `Or`

**Filter Examples**:
```
?$filter=FirstName eq 'Scott'
?$filter=contains(Location, 'Building A')
?$filter=Status eq 'Active' and Priority gt 3
```

#### $expand (Depth: 1)
Get relationship data
```
GET {{host}}/meridium/api/odata/Equipment?$expand=WorkHistory
GET {{host}}/meridium/api/odata/Equipment?$expand=WorkHistory($filter=Status eq 'Open')
```

#### $orderby
Sort results
```
GET {{host}}/meridium/api/odata/Equipment?$orderby=Name asc
```

#### $top
Limit results
```
GET {{host}}/meridium/api/odata/Equipment?$top=50
```

#### $skip
Skip results (pagination)
```
GET {{host}}/meridium/api/odata/Equipment?$skip=100&$top=50
```

### Configuration

**Default Page Size**: 1000 (configurable in Web.config)

**Query Parameter Page Size**: 1-1000 for most endpoints
```
/v1/assets?pageSize=250
```

**Browse API Maximum**: 250 results (not changeable)

### Data Transfer Objects (DTOs)

#### Entity DTO Structure (EntDevDTO)

**Fields**:
- **Entity Key**: Int64 format (must be zero for INSERT)
- **Family Key**: Required for all operations
- **Entity ID**: Not required to be unique (returns array on GET)
- **Properties**: Dictionary of field name/value pairs

**Example Entity DTO**:
```json
{
  "EntityKey": 0,
  "FamilyKey": "Equipment",
  "EntityId": "PUMP-001",
  "Properties": {
    "Name": "Primary Cooling Pump",
    "Status": "Active",
    "Location": "Building A",
    "InstallDate": "2023-01-15T00:00:00Z"
  }
}
```

### Entity Operations

#### Retrieve Entity (GET)

**By Family**:
```http
GET {{host}}/meridium/api/odata/Equipment
```

**By Entity ID** (returns array):
```http
GET {{host}}/meridium/api/odata/Equipment?$filter=EntityId eq 'PUMP-001'
```

**Count Entities**:
```http
GET {{host}}/meridium/api/odata/Equipment/$count
```

#### Create Entity (POST)

**Endpoint**: `POST {{host}}/meridium/api/odata/{FamilyKey}`

**Requirements**:
- Entity Key must be zero
- Family Key is required
- DTO is required in request body

**Example Request**:
```json
{
  "EntityKey": 0,
  "FamilyKey": "Equipment",
  "EntityId": "PUMP-002",
  "Properties": {
    "Name": "Secondary Pump",
    "Status": "Active"
  }
}
```

#### Update Entity (PUT/PATCH)

**Endpoint**: `PATCH {{host}}/meridium/api/odata/{FamilyKey}({EntityKey})`

**Requirements**:
- Must retrieve entity before updating
- Include only fields to update (PATCH)
- Include all fields for full update (PUT)

**Example Request** (PATCH):
```json
{
  "Properties": {
    "Status": "Maintenance"
  }
}
```

#### Delete Entity (DELETE)

**Endpoint**: `DELETE {{host}}/meridium/api/odata/{FamilyKey}({EntityKey})`

### Relationship Operations

#### Retrieve Relationships

**Using $expand**:
```http
GET {{host}}/meridium/api/odata/Equipment?$expand=WorkHistory
```

#### Create Relationship

**Endpoint**: `POST {{host}}/meridium/api/odata/{RelationshipFamily}`

**Requirements**:
- Relationship Key must be zero
- Both entity keys must be provided

**Example**:
```json
{
  "RelationshipKey": 0,
  "LeftEntityKey": 12345,
  "RightEntityKey": 67890,
  "FamilyKey": "Has_Work_History"
}
```

#### Update Relationship

**Note**: Fields on relationships were obsoleted prior to v4.0. Only update legacy relationships with fields.

### API Constraints

All Custom Development API endpoints are subject to:
- **Authentication**: Meridium token required
- **Authorization**: User permissions enforced
- **Localization**: Supports multiple languages
- **Globalization**: Date/time formats, number formats
- **Serialization**: JSON and XML supported

---

## Simple Ingestion API v2

### Overview

The Simple Ingestion API v2 provides asynchronous bulk data ingestion capabilities for all GE APM data loaders.

**Key Features**:
- Asynchronous processing
- Works with all data loaders
- Bundle-based submission
- Status tracking
- Error reporting with rejected rows

### Base URL

```
{{host}}/meridium/api/ingestion/v2
```

### Workflow

#### 1. Register Data Bundle

**Endpoint**: `POST {{host}}/meridium/api/ingestion/v2/bundles`

**Request Body**:
```json
{
  "bundleName": "Monthly Asset Import",
  "description": "January 2025 asset data",
  "tables": [
    {
      "tableName": "Equipment",
      "rows": [
        {
          "Name": "Pump A",
          "Status": "Active",
          "Location": "Building 1"
        },
        {
          "Name": "Pump B",
          "Status": "Inactive",
          "Location": "Building 2"
        }
      ]
    }
  ]
}
```

**Response**:
```json
{
  "bundleId": "abc-123-def-456",
  "status": "Registered",
  "tableCount": 1,
  "rowCount": 2,
  "submittedAt": "2025-01-15T10:30:00Z"
}
```

**Response Fields**:
- `bundleId`: Unique identifier for tracking
- `tableCount`: Number of tables submitted
- `rowCount`: Total rows across all tables
- `status`: Current processing status

#### 2. Check Bundle Status

**Endpoint**: `GET {{host}}/meridium/api/ingestion/v2/bundles/{bundleId}`

**Response**:
```json
{
  "bundleId": "abc-123-def-456",
  "bundleName": "Monthly Asset Import",
  "status": "Processing",
  "progress": 75,
  "tablesProcessed": 1,
  "rowsProcessed": 1500,
  "rowsRejected": 25,
  "startedAt": "2025-01-15T10:30:05Z",
  "updatedAt": "2025-01-15T10:35:00Z"
}
```

**Status Values**:
- `Registered` - Bundle received, awaiting processing
- `Processing` - Currently importing data
- `Completed` - Successfully completed
- `CompletedWithErrors` - Completed with some rejected rows
- `Failed` - Processing failed

#### 3. Retrieve Rejected Rows

**Endpoint**: `GET {{host}}/meridium/api/ingestion/v2/bundles/{bundleId}/rejected-rows`

**Query Parameters**:
- `tableName` (optional): Filter by specific table
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Rows per page (default: 100)

**Response**:
```json
{
  "bundleId": "abc-123-def-456",
  "tableName": "Equipment",
  "totalRejected": 25,
  "page": 1,
  "pageSize": 100,
  "rejectedRows": [
    {
      "rowNumber": 15,
      "rowData": {
        "Name": "Pump C",
        "Status": "InvalidStatus"
      },
      "errorCode": "VALIDATION_ERROR",
      "errorMessage": "Invalid Status value: 'InvalidStatus'. Expected: Active, Inactive, Maintenance"
    }
  ]
}
```

#### 4. Export Rejected Rows

**Endpoint**: `GET {{host}}/meridium/api/ingestion/v2/bundles/{bundleId}/rejected-rows/export`

**Response**: CSV file download

### Data Loaders Supported

The v2 API works with all data loaders, including:

- **Equipment and Functional Location Data Loader**
- **Work History Data Loader**
- **Inspection Data Loaders**
- **Unified Asset Ingestion Data Loader**
- **Asset Hierarchy Data Loaders**
- **Custom Data Loaders**

### Bundle Structure

**Tables**: Each bundle contains one or more tables
- Table name corresponds to target family or entity type
- Each table contains rows of data

**Rows**: Each row represents a unique record
- Key/value pairs of field names and values
- Must conform to target family schema

### Error Handling

**Validation Errors**: Caught during submission
- Schema mismatches
- Required field violations
- Data type errors

**Processing Errors**: Caught during import
- Duplicate key violations
- Relationship errors
- Business rule violations

**Rejected Row Details**:
- Original row data preserved
- Error code provided
- Detailed error message
- Row number in original submission

### Best Practices

1. **Bundle Size**: Keep bundles under 10,000 rows for optimal performance
2. **Status Polling**: Poll status every 30-60 seconds during processing
3. **Error Review**: Always review rejected rows after completion
4. **Retry Logic**: Implement exponential backoff for status checks
5. **Idempotency**: Use unique bundle names for tracking

---

## Asset API (Predix APM)

### Base URL

```
https://apm-asset-apidoc-svc-prod.app-api.aws-usw02-pr.predix.io
```

### API Versions

- **v1**: Legacy API (still supported)
- **v3**: Current API version

### S95 Asset Hierarchy

GE APM uses the ISA-95 standard for asset modeling with four classification levels:

#### Classification Types (ccomClass)

1. **ENTERPRISE_TYPE** - Top-level organization
2. **SITE_TYPE** - Physical sites/locations
3. **SEGMENT_TYPE** - Operational segments
4. **ASSET_TYPE** - Individual assets

#### Instance Types (ccomClass)

1. **ENTERPRISE** - Enterprise instance
2. **SITE** - Site instance
3. **SEGMENT** - Segment instance
4. **ASSET** - Asset instance

### Asset API Endpoints

#### Enterprise Types

**List Enterprise Types**:
```http
GET /v1/enterpriseTypes
```

**Get Enterprise Type**:
```http
GET /v1/enterpriseTypes/{UUID}
```

**Response**:
```json
{
  "uuid": "2408ba25-77d2-42b2-9a8f-88b01875a86b",
  "name": "Manufacturing Enterprise",
  "ccomClass": "ENTERPRISE_TYPE",
  "childClassifications": [
    {
      "uuid": "...",
      "name": "North America Division",
      "ccomClass": "SITE_TYPE"
    }
  ]
}
```

#### Site Types

**List Site Types**:
```http
GET /v1/siteTypes
```

**Get Site Type**:
```http
GET /v1/siteTypes/{UUID}
```

#### Segment Types

**List Segment Types**:
```http
GET /v1/segmentTypes
```

**Get Segment Type**:
```http
GET /v1/segmentTypes/{UUID}
```

#### Asset Types

**List Asset Types**:
```http
GET /v1/assetTypes
```

**Get Asset Type**:
```http
GET /v1/assetTypes/{UUID}
```

### Asset Instances

#### Create Asset Instance (POST)

**Endpoint**: `POST /v1/assets`

**Request Body**:
```json
{
  "name": "Pump A-101",
  "description": "Primary cooling pump",
  "ccomClass": "ASSET",
  "classificationUri": "/v1/assetTypes/{typeUUID}",
  "properties": {
    "manufacturer": "GE",
    "model": "CP-5000",
    "serialNumber": "SN-123456",
    "installDate": "2023-01-15"
  },
  "reservedProperties": {
    "status": "Active",
    "location": "Building A, Floor 2"
  },
  "tags": ["critical", "cooling-system", "monitored"]
}
```

**Response**:
```json
{
  "uuid": "a7f3bc45-1234-5678-9abc-def012345678",
  "name": "Pump A-101",
  "uri": "/v1/assets/a7f3bc45-1234-5678-9abc-def012345678",
  "ccomClass": "ASSET",
  "status": "INGESTED"
}
```

**Important**:
- Save the `uuid` from response for tracking
- Ingestion happens asynchronously
- Status can be checked via GET endpoint

#### Get Asset Instance

**Endpoint**: `GET /v1/assets/{UUID}`

**Response**: Full asset details including properties, tags, and classifications

#### Update Asset Instance

**Endpoint**: `PUT /v1/assets/{UUID}`

**Request Body**: Similar to POST, include fields to update

#### Delete Asset Instance

**Endpoint**: `DELETE /v1/assets/{UUID}`

**Note**: Deletes one instance at a time by UUID

#### List/Browse Assets

**Endpoint**: `GET /v1/assets`

**Query Parameters**:
- `pageSize`: 1-1000 (default varies by endpoint)
- `page`: Page number for pagination
- `filter`: Filter expression
- `sort`: Sort field and direction

**Example**:
```http
GET /v1/assets?pageSize=250&filter=status eq 'Active'
```

**Browse Endpoint**:
```http
GET /v1/assets/browse
```
**Note**: Maximum 250 results (not configurable)

### Tag Instances

Tags represent measurements from sensors, analytic calculations, or asset-specific inputs.

#### Tag Structure

```json
{
  "name": "Temperature_Sensor_1",
  "description": "Pump outlet temperature",
  "dataType": "DOUBLE",
  "unit": "Celsius",
  "tagClassification": "/v1/tagClassifications/{UUID}",
  "reservedProperties": {
    "samplingRate": "1000",
    "accuracy": "0.1"
  },
  "properties": {
    "calibrationDate": "2024-12-01",
    "sensorType": "Thermocouple"
  }
}
```

#### Tag Operations

**Create Tag**:
```http
POST /v1/tags
```

**Get Tag**:
```http
GET /v1/tags/{UUID}
```

**List Tags**:
```http
GET /v1/tags?pageSize=250
```

### Classifications

Classifications define types and hierarchies for assets and tags.

#### About Classifications

- **Classification Hierarchy**: Parent-child relationships
- **Reserved Properties**: Standard system properties
- **Custom Properties**: User-defined attributes
- **Inheritance**: Child classifications inherit from parents

#### Classification Operations

**Create Classification**:
```http
POST /v1/classifications
```

**Get Classification**:
```http
GET /v1/classifications/{UUID}
```

**List Classifications**:
```http
GET /v1/classifications
```

### Asset Ingestion Files

For bulk ingestion, files are processed in specific order:

1. **instances_filename.zip** - Asset and classification instances
2. **connections_filename.zip** - Relationships between instances
3. **tagClassifications_filename.zip** - Tag classification definitions
4. **tagAssociations_filename.zip** - Tag-to-asset associations

---

## Data Models and Families

### Core Families

GE APM organizes data into "families" - logical groupings of related entities.

#### Equipment Family

Represents physical assets and equipment.

**Common Fields**:
- `Name` - Equipment name/identifier
- `Description` - Equipment description
- `Status` - Operational status (Active, Inactive, Maintenance)
- `Location` - Physical location
- `SerialNumber` - Manufacturer serial number
- `Model` - Equipment model
- `Manufacturer` - Equipment manufacturer
- `InstallDate` - Installation date
- `CommissionDate` - Commissioning date

#### Functional Location Family

Represents locations in the asset hierarchy.

**Common Fields**:
- `Name` - Location identifier
- `Description` - Location description
- `LocationType` - Type of location
- `ParentLocation` - Parent in hierarchy
- `Status` - Location status

**Hierarchy**:
- Functional Locations can have child Functional Locations
- Equipment is typically at lowest levels of Functional Locations

#### Work History Family

Represents work orders and maintenance activities.

**Common Fields**:
- `WorkOrderID` - Work order identifier
- `Description` - Work description
- `Status` - Work order status
- `Priority` - Priority level
- `AssignedTo` - Assigned user/team
- `ScheduledDate` - Scheduled start date
- `CompletionDate` - Actual completion date
- `EstimatedHours` - Estimated duration
- `ActualHours` - Actual duration

**Relationships**:
- Linked to Equipment or Functional Location
- Can have multiple Work History Detail records

#### Work History Detail Family

Detailed records for work order tasks and activities.

**Common Fields**:
- `TaskDescription` - Task details
- `TaskStatus` - Task status
- `CompletedBy` - User who completed task
- `CompletionDate` - Task completion date
- `Notes` - Additional notes

### Taxonomy

Equipment classification system with three levels:

1. **Category** - Highest level classification
2. **Class** - Mid-level classification
3. **Type** - Specific equipment type

**Taxonomy Mapping Family**: Maps Object Type to equipment taxonomy

### Relationships

Relationships connect entities across families.

**Common Relationship Types**:
- `Has Equipment` - Functional Location to Equipment
- `Has Work History` - Equipment/Location to Work History
- `Has Parent` - Hierarchy relationships
- `Has Classification` - Entity to Taxonomy

**Relationship Structure**:
```json
{
  "RelationshipKey": 0,
  "FamilyKey": "Has_Equipment",
  "LeftEntityKey": 12345,
  "RightEntityKey": 67890
}
```

**Important Notes**:
- Fields on relationships were obsoleted before v4.0
- Only update legacy relationships that have fields
- Use $expand to retrieve relationship data

---

## Error Handling

### HTTP Status Codes

#### 2xx Success

- **200 OK** - Successful GET, PUT, PATCH
- **201 Created** - Successful POST
- **204 No Content** - Successful DELETE

#### 4xx Client Errors

**401 Unauthorized**
- Missing or invalid authentication token
- Token expired
- Insufficient credentials

**Solution**:
- Obtain new token via Login API (On-Premises)
- Refresh OAuth token via UAA (Predix)
- Verify credentials

**403 Forbidden**
- Valid authentication but insufficient permissions
- User not authorized for requested operation

**Solution**:
- Verify user has required permissions
- Check family/entity-level security
- Contact administrator

**404 Not Found**
- Requested entity does not exist
- Invalid endpoint URL
- Entity deleted

**Solution**:
- Verify entity key/UUID
- Check endpoint path
- Confirm entity exists

**429 Too Many Requests**
- Rate limit exceeded
- Too many concurrent requests

**Solution**:
- Implement exponential backoff
- Reduce request frequency
- Add retry logic with delays

#### 5xx Server Errors

**500 Internal Server Error**
- Generic server error
- Unhandled exception
- Database connection issue

**Solution**:
- Retry with exponential backoff
- Check server logs
- Contact support if persistent

**503 Service Unavailable**
- Service temporarily down
- Maintenance window
- Overloaded server

**Solution**:
- Retry after delay
- Check service status
- Contact support

### Error Response Format

**Standard Error Response**:
```json
{
  "error": {
    "code": "ENTITY_NOT_FOUND",
    "message": "Entity with key 12345 not found in family 'Equipment'",
    "details": {
      "entityKey": 12345,
      "familyKey": "Equipment"
    },
    "timestamp": "2025-01-15T10:30:00Z"
  }
}
```

### Validation Errors

**Common Validation Errors**:

1. **Required Field Missing**
```json
{
  "error": {
    "code": "REQUIRED_FIELD_MISSING",
    "message": "Required field 'Name' is missing",
    "field": "Name"
  }
}
```

2. **Invalid Data Type**
```json
{
  "error": {
    "code": "INVALID_DATA_TYPE",
    "message": "Field 'InstallDate' expects DateTime, received String",
    "field": "InstallDate",
    "expected": "DateTime",
    "received": "String"
  }
}
```

3. **Invalid Entity Key**
```json
{
  "error": {
    "code": "INVALID_ENTITY_KEY",
    "message": "Entity Key must be zero for INSERT operations",
    "field": "EntityKey",
    "value": 12345
  }
}
```

### Ingestion Errors

**Bundle Processing Errors**:
- Schema validation failures
- Duplicate key violations
- Relationship constraint violations
- Business rule violations

**Error Tracking**:
- Check bundle status for error indicators
- Retrieve rejected rows for details
- Export rejected data for analysis

---

## Best Practices

### Authentication

1. **Token Caching**: Cache tokens until near expiration
   - On-Premises: Session-based, cache for session duration
   - Predix: Cache for (expires_in - 300) seconds

2. **Token Refresh**: Implement proactive refresh
   - Request new token 5 minutes before expiration
   - Handle 401 responses with automatic re-authentication

3. **Secure Storage**: Store credentials securely
   - Use environment variables
   - Never hardcode in source code
   - Use secure credential managers

### Query Optimization

1. **Use $select**: Request only needed fields
```http
GET {{host}}/meridium/api/odata/Equipment?$select=Name,Status,Location
```

2. **Use $filter**: Filter server-side, not client-side
```http
GET {{host}}/meridium/api/odata/Equipment?$filter=Status eq 'Active'
```

3. **Use $top**: Limit results to needed amount
```http
GET {{host}}/meridium/api/odata/Equipment?$top=100
```

4. **Pagination**: Use $skip and $top together
```http
GET {{host}}/meridium/api/odata/Equipment?$skip=200&$top=100
```

5. **Expansion Limit**: Remember $expand is limited to depth 1
```http
GET {{host}}/meridium/api/odata/Equipment?$expand=WorkHistory
```

### Data Ingestion

1. **Batch Size**: Optimal bundle sizes
   - On-Premises: 5,000-10,000 rows per bundle
   - Cloud: Adjust based on network and processing capacity

2. **Async Processing**: Don't wait synchronously
   - Submit bundle
   - Store bundle ID
   - Poll status asynchronously
   - Process results when complete

3. **Error Handling**: Always check for rejected rows
   - Review rejection reasons
   - Fix data issues
   - Resubmit corrected data

4. **Data Validation**: Validate before submission
   - Check required fields
   - Validate data types
   - Verify relationships exist
   - Conform to business rules

### Entity Operations

1. **Retrieve Before Update**: Always GET before PUT/PATCH
   - Ensures entity exists
   - Gets current EntityKey
   - Prevents overwriting recent changes

2. **Use PATCH**: Prefer partial updates
   - Only send changed fields
   - Reduces payload size
   - Prevents accidental overwrites

3. **Entity ID Uniqueness**: Don't rely on Entity ID uniqueness
   - Entity ID is not unique in APM
   - Use Entity Key for unique identification
   - GET by Entity ID returns array

4. **Relationship Management**: Handle relationships carefully
   - Verify both entities exist before creating relationship
   - Don't update relationship fields (obsoleted)
   - Use $expand to retrieve related data

### Performance

1. **Connection Pooling**: Reuse HTTP connections
   - Use persistent connections
   - Configure connection pool size
   - Set appropriate timeouts

2. **Parallel Requests**: Process in parallel when possible
   - Query multiple families simultaneously
   - Batch independent operations
   - Respect rate limits

3. **Caching**: Cache reference data
   - Taxonomy mappings
   - Classification hierarchies
   - User lists
   - Configuration data

4. **Compression**: Enable HTTP compression
   - Reduces payload size
   - Faster transmission
   - Lower bandwidth costs

### Monitoring

1. **Logging**: Log all API interactions
   - Request/response pairs
   - Timestamps
   - Response times
   - Error details

2. **Metrics**: Track key metrics
   - Request count
   - Success rate
   - Error rate
   - Response times
   - Bundle processing times

3. **Alerting**: Set up alerts for
   - High error rates
   - Slow response times
   - Authentication failures
   - Bundle processing failures

---

## Common Use Cases

### Use Case 1: Asset Data Synchronization

**Scenario**: Sync asset data from external system to GE APM

**Steps**:

1. **Authenticate**
```http
POST {{host}}/meridium/api/login
Body: { "username": "user", "password": "pass" }
```

2. **Retrieve Existing Assets**
```http
GET {{host}}/meridium/api/odata/Equipment?$select=EntityKey,EntityId,Name,Status
```

3. **Compare and Identify Changes**
   - Match external IDs to Entity IDs
   - Identify new assets (CREATE)
   - Identify changed assets (UPDATE)
   - Identify removed assets (DELETE/deactivate)

4. **Create New Assets**
```http
POST {{host}}/meridium/api/odata/Equipment
Body: {
  "EntityKey": 0,
  "FamilyKey": "Equipment",
  "EntityId": "EXT-12345",
  "Properties": {
    "Name": "New Pump",
    "Status": "Active",
    "Location": "Building A"
  }
}
```

5. **Update Existing Assets**
```http
PATCH {{host}}/meridium/api/odata/Equipment(67890)
Body: {
  "Properties": {
    "Status": "Maintenance",
    "Location": "Building B"
  }
}
```

6. **Log Results**
   - Record success/failure for each operation
   - Store new Entity Keys for future syncs
   - Generate sync report

### Use Case 2: Bulk Work Order Import

**Scenario**: Import monthly work orders from maintenance system

**Steps**:

1. **Prepare Data Bundle**
```json
{
  "bundleName": "January 2025 Work Orders",
  "tables": [
    {
      "tableName": "Work_History",
      "rows": [
        {
          "WorkOrderID": "WO-2025-001",
          "Description": "Quarterly inspection",
          "Status": "Scheduled",
          "EquipmentID": "PUMP-001",
          "ScheduledDate": "2025-01-20"
        },
        // ... more work orders
      ]
    }
  ]
}
```

2. **Submit Bundle**
```http
POST {{host}}/meridium/api/ingestion/v2/bundles
```

3. **Track Processing**
```http
GET {{host}}/meridium/api/ingestion/v2/bundles/{bundleId}
```

4. **Check Every 30 Seconds Until Complete**

5. **Review Rejected Rows**
```http
GET {{host}}/meridium/api/ingestion/v2/bundles/{bundleId}/rejected-rows
```

6. **Export and Fix Rejected Data**
   - Download rejected rows
   - Correct data issues
   - Resubmit as new bundle

### Use Case 3: Asset Hierarchy Creation (Predix)

**Scenario**: Create multi-level asset hierarchy for new facility

**Steps**:

1. **Authenticate with UAA**
```http
POST https://{uaa}.predix-uaa.run.aws-usw02-pr.ice.predix.io/oauth/token
```

2. **Create Enterprise Type** (if not exists)
```http
POST /v1/enterpriseTypes
Body: {
  "name": "Manufacturing Division",
  "ccomClass": "ENTERPRISE_TYPE"
}
```

3. **Create Site Type**
```http
POST /v1/siteTypes
Body: {
  "name": "Texas Facility",
  "ccomClass": "SITE_TYPE",
  "parentUri": "/v1/enterpriseTypes/{enterpriseUUID}"
}
```

4. **Create Segment Type**
```http
POST /v1/segmentTypes
Body: {
  "name": "Production Line 1",
  "ccomClass": "SEGMENT_TYPE",
  "parentUri": "/v1/siteTypes/{siteUUID}"
}
```

5. **Create Asset Type**
```http
POST /v1/assetTypes
Body: {
  "name": "Centrifugal Pump",
  "ccomClass": "ASSET_TYPE",
  "parentUri": "/v1/segmentTypes/{segmentUUID}"
}
```

6. **Create Asset Instances**
```http
POST /v1/assets
Body: {
  "name": "Pump A-101",
  "ccomClass": "ASSET",
  "classificationUri": "/v1/assetTypes/{assetTypeUUID}",
  "properties": { ... },
  "tags": ["critical"]
}
```

7. **Verify Hierarchy**
```http
GET /v1/assets?$expand=classification
```

### Use Case 4: Tag Data Ingestion

**Scenario**: Ingest sensor tag data and associate with assets

**Steps**:

1. **Create Tag Classifications**
```http
POST /v1/tagClassifications
Body: {
  "name": "Temperature Sensor",
  "dataType": "DOUBLE",
  "unit": "Celsius"
}
```

2. **Create Tag Instances**
```http
POST /v1/tags
Body: {
  "name": "TEMP_PUMP_A101_OUTLET",
  "tagClassification": "/v1/tagClassifications/{UUID}",
  "properties": {
    "samplingRate": 1000,
    "accuracy": 0.1
  }
}
```

3. **Associate Tags with Assets**
```http
POST /v1/tagAssociations
Body: {
  "assetUri": "/v1/assets/{assetUUID}",
  "tagUri": "/v1/tags/{tagUUID}",
  "associationType": "MONITORS"
}
```

4. **Ingest Tag Values** (via separate time-series API)

### Use Case 5: Work History Reporting

**Scenario**: Generate work history report for equipment

**Steps**:

1. **Query Equipment with Work History**
```http
GET {{host}}/meridium/api/odata/Equipment
  ?$filter=EntityId eq 'PUMP-001'
  &$expand=WorkHistory($orderby=CompletionDate desc;$top=10)
```

2. **Get Work History Details**
```http
GET {{host}}/meridium/api/odata/Work_History
  ?$filter=EquipmentKey eq 12345
  &$orderby=CompletionDate desc
  &$expand=WorkHistoryDetail
```

3. **Extract and Format Data**
   - Parse JSON response
   - Transform to desired format
   - Generate report (PDF, Excel, etc.)

### Use Case 6: Equipment Status Dashboard

**Scenario**: Real-time dashboard of equipment status

**Steps**:

1. **Query Active Equipment**
```http
GET {{host}}/meridium/api/odata/Equipment
  ?$filter=Status ne 'Retired'
  &$select=EntityId,Name,Status,Location
  &$orderby=Name
```

2. **Group by Status**
   - Client-side grouping
   - Count by status
   - Calculate percentages

3. **Get Critical Equipment in Maintenance**
```http
GET {{host}}/meridium/api/odata/Equipment
  ?$filter=Status eq 'Maintenance' and Priority eq 'Critical'
  &$select=EntityId,Name,Location
```

4. **Refresh Periodically**
   - Poll every 60 seconds
   - Update dashboard
   - Highlight changes

---

## Additional Resources

### Official Documentation

**GE Vernova APM**:
- On-Premises APM v5.22: https://www.gevernova.com/software/documentation/onpremises-apm/v522/
- Cloud APM: https://www.ge.com/digital/documentation/cloud-apm/latest/
- Essentials: https://www.gevernova.com/software/documentation/essentials/latest/

**GE Digital APM**:
- Predix APM: https://www.ge.com/digital/documentation/predix-apm/latest/
- APM Classic: https://www.ge.com/digital/documentation/apm-classic/

**Specific Topics**:
- Custom Development: https://www.gevernova.com/software/documentation/onpremises-apm/v522/help/custdev-getting-started.html
- Simple Ingestion API v2: https://www.gevernova.com/software/documentation/onpremises-apm/v522/help/simple-ingestion-overview-v2.html
- Data Transfer Objects: https://www.gevernova.com/software/documentation/onpremises-apm/v522/help/custdev-data-transfer-objects.html
- OData Supported Features: https://www.ge.com/digital/documentation/cloud-apm/latest/ade-supported-features.html

**Asset API**:
- Asset API v1 Guide: https://apm-asset-apidoc-svc-prod.app-api.aws-usw02-pr.predix.io/docs/indexv1.html
- Asset Developer Documentation: https://www.ge.com/digital/documentation/predix-apm/v46x/assets-developer-doc.html
- Asset Ingestion: https://www.ge.com/digital/documentation/predix-apm/latest/assets-ingestion.html

**Authentication**:
- UAA OAuth2: https://www.ge.com/digital/documentation/predix-services/
- On-Premises Authentication: https://www.ge.com/digital/documentation/onpremises-apm/v50/help/ade-authentication.html

### Support

- **Customer Support**: Contact your GE Vernova Account Representative
- **Documentation Portal**: Requires customer account credentials
- **Training**: GE Digital Learning (https://www.gedigital-learning.com/)

---

## Appendix

### Glossary

**APM**: Asset Performance Management

**DTO**: Data Transfer Object - Structured data format for API requests/responses

**Entity**: A single record in a family (e.g., one piece of equipment)

**Family**: Logical grouping of related entities (e.g., Equipment family)

**Family Key**: Unique identifier for a family

**Entity Key**: Unique identifier for an entity (Int64)

**Entity ID**: User-defined identifier (not necessarily unique)

**Relationship**: Connection between two entities

**OData**: Open Data Protocol - RESTful protocol for querying and updating data

**UAA**: User Account and Authentication - GE Predix authentication service

**OAuth2**: Open standard for access delegation and authorization

**S95/ISA-95**: International standard for enterprise-control system integration

**Bundle**: Collection of data tables submitted for ingestion

**Tag**: Measurement point from sensors or analytics

**Classification**: Type definition in asset hierarchy

**Instance**: Specific occurrence of a classification

### Version History

- **2025-01-15**: Initial specification document created
- Based on GE Vernova APM v5.22, Predix APM, and APM Classic documentation

---

*Document Created: 2025-01-15*
*Based on GE Vernova APM API Documentation*
*Sources: gevernova.com, ge.com/digital*
