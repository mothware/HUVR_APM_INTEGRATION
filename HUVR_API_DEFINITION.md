# HUVR Data API Definition Document

## Table of Contents
1. [Overview](#overview)
2. [Getting Started](#getting-started)
3. [Authentication](#authentication)
4. [API Endpoints](#api-endpoints)
5. [Best Practices](#best-practices)
6. [Common Use Cases](#common-use-cases)

---

## Overview

The HUVR Data API is a RESTful API that provides programmatic access to the HUVR reliability automation platform. The API enables integration with asset management systems, inspection workflows, and reliability data processing.

**Base URL**: `https://api.huvrdata.app`

**API Version**: v3

**Documentation**: https://docs.huvrdata.app

**OpenAPI Specification**: https://docs.huvrdata.app/openapi/

---

## Getting Started

### Prerequisites

To start building with HUVR, you need:
1. **HUVR Workspace** - An active HUVR workspace account
2. **User Credentials** - Valid user account credentials
3. **Service Account** - For machine-to-machine integrations

For existing customers exploring the API, contact your Customer Success representative for API credentials.

### Initial Setup

1. Log into your HUVR workspace
2. Create at least one demo asset to use with the API
3. Request service account credentials from your support contact
4. Review the full list of available API endpoints with code samples at https://docs.huvrdata.app

**Important**: Normal HUVR account credentials will not work with API authentication flows. You must request a dedicated service account.

---

## Authentication

### Authentication Method

HUVR uses token-based authentication with short-lived access tokens.

### Service Account Credentials

You will receive:
- `HUVR_CLIENT_ID` - Format: `[email protected]`
- `HUVR_CLIENT_SECRET` - Secret key for authentication

### Obtaining an Access Token

**Endpoint**: `POST /api/auth/obtain-access-token/`

**Request Body**:
```json
{
  "client_id": "your-client-id",
  "client_secret": "your-client-secret"
}
```

**Response**:
```json
{
  "access_token": "your-access-token",
  "token_type": "Token",
  "expires_in": 3600
}
```

**Token Validity**: 60 minutes

### Using the Access Token

For any API endpoint requiring authentication, include the following header:

```
Authorization: Token {access_token}
```

### Token Refresh

HUVR does not use traditional OAuth refresh tokens. When you receive a 401 Unauthorized response:
1. Your token has expired
2. Make a new POST request to `/api/auth/obtain-access-token/`
3. Use the CLIENT_ID and CLIENT_SECRET to obtain a new access token

---

## API Endpoints

### Authentication Endpoints

#### Obtain Access Token
- **Method**: `POST`
- **Endpoint**: `/api/auth/obtain-access-token/`
- **Authentication**: None (uses CLIENT_ID/CLIENT_SECRET)
- **Purpose**: Obtain a new access token
- **Token Lifetime**: 60 minutes

---

### Assets

Assets represent physical equipment, infrastructure, or items being monitored and inspected in the HUVR system.

#### List Assets
- **Method**: `GET`
- **Endpoint**: `/api/assets/`
- **Authentication**: Required
- **Query Parameters**: Extensive query params available for filtering

#### Get Asset Details
- **Method**: `GET`
- **Endpoint**: `/api/assets/{asset_id}/`
- **Authentication**: Required

#### Create Asset
- **Method**: `POST`
- **Endpoint**: `/api/assets/`
- **Authentication**: Required

#### Update Asset (Partial)
- **Method**: `PATCH`
- **Endpoint**: `/api/assets/{asset_id}/`
- **Authentication**: Required
- **Note**: All API objects support partial updates using PATCH

#### Delete Asset
- **Method**: `DELETE`
- **Endpoint**: `/api/assets/{asset_id}/`
- **Authentication**: Required

---

### Projects (Work Orders)

Projects are core units of work inside HUVR, representing inspections, repairs, surveys, or any variety of work performed on assets.

**Project Components**:
- Media files
- Digital forms (checklists)
- Structured metadata around inspection findings
- Detailed measurements (e.g., Ultrasonic Testing data)

#### List Projects
- **Method**: `GET`
- **Endpoint**: `/api/projects/`
- **Authentication**: Required
- **Query Parameters**:
  - `asset_search` - Filter projects by asset
  - Additional extensive query params available

#### Get Project Details
- **Method**: `GET`
- **Endpoint**: `/api/projects/{project_id}/`
- **Authentication**: Required

#### Create Project
- **Method**: `POST`
- **Endpoint**: `/api/projects/`
- **Authentication**: Required
- **Request Body**: Includes asset ID and project type ID

#### Update Project (Partial)
- **Method**: `PATCH`
- **Endpoint**: `/api/projects/{project_id}/`
- **Authentication**: Required

#### Delete Project
- **Method**: `DELETE`
- **Endpoint**: `/api/projects/{project_id}/`
- **Authentication**: Required

---

### Inspection Media

Media files associated with inspections and projects. Media objects are immutable once uploaded, but their metadata can be updated.

#### List Inspection Media
- **Method**: `GET`
- **Endpoint**: `/api/inspection-media/`
- **Authentication**: Required

#### Get Inspection Media Details
- **Method**: `GET`
- **Endpoint**: `/api/inspection-media/{media_id}/`
- **Authentication**: Required
- **Note**: Retrieves updated upload URL if expired

#### Upload Media (Two-Part Process)

**Part 1: Create Media Object**
- **Method**: `POST`
- **Endpoint**: `/api/inspection-media/`
- **Authentication**: Required
- **Response**: Returns media object with upload URL

**Part 2: Upload File**
- **Method**: `PUT`
- **Endpoint**: Use the `upload.url` from Part 1 response
- **Body**: Binary file data
- **URL Expiration**: 15 minutes

**Post-Upload Processing**:
- System automatically marks media as `UPLOADED`
- Generates thumbnail and preview images for image files

**Upload URL Expiration Handling**:
- Upload URL expires after 15 minutes
- If upload fails or is delayed, GET the media object again (`GET /api/inspection-media/{media_id}/`)
- Response will include a newly generated URL with a fresh expiration time

#### Update Inspection Media Metadata
- **Method**: `PATCH`
- **Endpoint**: `/api/inspection-media/{media_id}/`
- **Authentication**: Required
- **Note**: Updates metadata only; file content is immutable

---

### Checklists (Digital Forms)

Digital forms used to capture structured data during inspections.

#### List Checklists
- **Method**: `GET`
- **Endpoint**: `/api/checklists/`
- **Authentication**: Required

#### Get Checklist Details
- **Method**: `GET`
- **Endpoint**: `/api/checklists/{checklist_id}/`
- **Authentication**: Required

#### Create Checklist
- **Method**: `POST`
- **Endpoint**: `/api/checklists/`
- **Authentication**: Required

#### Update Checklist (Partial)
- **Method**: `PATCH`
- **Endpoint**: `/api/checklists/{checklist_id}/`
- **Authentication**: Required

---

### Defects (Findings)

Structured metadata representing inspection findings and defects identified during work.

#### List Defects
- **Method**: `GET`
- **Endpoint**: `/api/defects/`
- **Authentication**: Required

#### Get Defect Details
- **Method**: `GET`
- **Endpoint**: `/api/defects/{defect_id}/`
- **Authentication**: Required

#### Create Defect
- **Method**: `POST`
- **Endpoint**: `/api/defects/`
- **Authentication**: Required

#### Update Defect (Partial)
- **Method**: `PATCH`
- **Endpoint**: `/api/defects/{defect_id}/`
- **Authentication**: Required

---

### Measurements

Detailed measurement data for specialized inspection workflows (e.g., Ultrasonic Testing).

#### List Measurements
- **Method**: `GET`
- **Endpoint**: `/api/measurements/`
- **Authentication**: Required

#### Get Measurement Details
- **Method**: `GET`
- **Endpoint**: `/api/measurements/{measurement_id}/`
- **Authentication**: Required

#### Create Measurement
- **Method**: `POST`
- **Endpoint**: `/api/measurements/`
- **Authentication**: Required

#### Update Measurement (Partial)
- **Method**: `PATCH`
- **Endpoint**: `/api/measurements/{measurement_id}/`
- **Authentication**: Required

---

### Users

#### List Users
- **Method**: `GET`
- **Endpoint**: `/api/users/`
- **Authentication**: Required

#### Get User Details
- **Method**: `GET`
- **Endpoint**: `/api/users/{user_id}/`
- **Authentication**: Required

---

### Workspaces

#### List Workspaces
- **Method**: `GET`
- **Endpoint**: `/api/workspaces/`
- **Authentication**: Required

#### Get Workspace Details
- **Method**: `GET`
- **Endpoint**: `/api/workspaces/{workspace_id}/`
- **Authentication**: Required

---

## Best Practices

### 1. Token Management
- **Short-lived tokens**: Access tokens expire after 60 minutes
- **Proactive refresh**: Monitor for 401 responses and refresh tokens immediately
- **No refresh token**: Re-authenticate using CLIENT_ID/CLIENT_SECRET to get new access tokens
- **Secure storage**: Store CLIENT_ID and CLIENT_SECRET securely

### 2. Query Parameters
- Most endpoints include extensive query parameters for filtering and searching
- Use specific filters to reduce response payload size
- Reference documentation may be overwhelming due to parameter count
- Start with basic queries and add filters as needed

### 3. Partial Updates (PATCH)
- All API objects support partial updates using the PATCH method
- Only send fields that need to be updated
- Reduces bandwidth and processing time
- Prevents accidental overwrites of unchanged data

### 4. Media Upload Workflow
- **Two-part process**: Create media object first, then upload file
- **URL expiration**: Upload URLs expire after 15 minutes
- **Retry logic**: If upload fails, retrieve media object again for fresh URL
- **Immutable files**: Media file content cannot be changed after upload
- **Mutable metadata**: Media metadata can be updated via PATCH

### 5. Rate Limiting
- Implement exponential backoff for failed requests
- Monitor for rate limit responses (429 status code)
- Cache responses when appropriate

### 6. Error Handling
- **401 Unauthorized**: Token expired - refresh immediately
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource doesn't exist
- **429 Too Many Requests**: Rate limit exceeded
- **500+ Server Errors**: Retry with exponential backoff

### 7. Data Retrieval
- Use pagination for large datasets
- Implement cursor-based pagination when available
- Filter at the API level rather than client-side

---

## Common Use Cases

### Use Case 1: Asset Inspection Workflow

1. **Authenticate**: Obtain access token
2. **List Assets**: Find the asset to inspect
3. **Create Project**: Create an inspection project for the asset
4. **Upload Media**: Upload inspection photos/videos
5. **Create Checklist**: Fill out digital inspection forms
6. **Record Defects**: Document any findings
7. **Add Measurements**: Record detailed measurement data (if applicable)

### Use Case 2: Batch Data Export

1. **Authenticate**: Obtain access token
2. **List Projects**: Retrieve projects with appropriate filters
3. **For each project**:
   - Get project details
   - Retrieve associated media
   - Download checklists
   - Export defects
   - Collect measurements
4. **Process Data**: Aggregate and transform for reporting

### Use Case 3: Integration with Asset Management System

1. **Authenticate**: Obtain access token
2. **Sync Assets**:
   - Retrieve assets from HUVR
   - Compare with local asset registry
   - Create/update assets as needed
3. **Monitor Projects**:
   - Query recent projects
   - Update work order status in local system
   - Link inspection media to asset records
4. **Refresh Token**: Re-authenticate as needed (every 60 minutes)

### Use Case 4: Automated Inspection Data Collection

1. **Service Account Setup**: Configure with CLIENT_ID/CLIENT_SECRET
2. **Scheduled Token Refresh**: Implement 55-minute refresh cycle
3. **Poll for New Data**:
   - Query projects updated since last check
   - Retrieve new media uploads
   - Download completed checklists
4. **Process and Store**: Save data to local database/data warehouse
5. **Error Recovery**: Implement retry logic for failed requests

---

## Additional Resources

- **Main Documentation**: https://docs.huvrdata.app
- **API Reference**: https://docs.huvrdata.app/reference
- **OpenAPI Specification**: https://docs.huvrdata.app/openapi/
- **Getting Started Guide**: https://docs.huvrdata.app/docs/getting-started
- **Authentication Guide**: https://docs.huvrdata.app/docs/authentication
- **Best Practices**: https://docs.huvrdata.app/docs/best-practices
- **Working with Projects**: https://docs.huvrdata.app/docs/working-with-projects
- **Python Client**: https://github.com/huvrdata/huvr-client

---

## Support

For API access and credentials:
- Contact your Customer Success representative
- Request service account credentials from your support contact

---

## Version History

- **v3** - Current version (as of January 2025)
- OpenAPI specification available for version-specific details

---

## Notes

1. **OpenAPI Specification**: All endpoints are generated from the OpenAPI spec. For the most up-to-date and complete endpoint documentation, refer to https://docs.huvrdata.app/openapi/

2. **Endpoint Discovery**: The API reference at https://docs.huvrdata.app/reference provides an interactive explorer with code samples for all available endpoints

3. **Documentation Updates**: The HUVR API documentation is actively maintained. Check the official documentation for the latest updates and new features

4. **Platform Focus**: HUVR is purpose-built for reliability automation, with particular strength in drone inspections, asset management, and industrial inspection workflows

---

*Document created: 2025-10-21*
*Based on HUVR Data API v3 documentation*
*Source: https://docs.huvrdata.app*
