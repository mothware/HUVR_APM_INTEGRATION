# Image Download Feature Documentation

## Overview

The Image Download feature allows users to download images associated with defect overlays from the HUVR API. When defects are inspected, they often have associated overlay images that provide visual context. This feature enables bulk downloading of these images as ZIP archives.

## Table of Contents

1. [Architecture](#architecture)
2. [Components](#components)
3. [API Endpoints](#api-endpoints)
4. [User Interface](#user-interface)
5. [Usage Guide](#usage-guide)
6. [Implementation Details](#implementation-details)
7. [Error Handling](#error-handling)
8. [Future Enhancements](#future-enhancements)

---

## Architecture

The image download system consists of three main layers:

1. **Service Layer**: `ImageDownloadService` - Handles HTTP requests to download images and create ZIP archives
2. **Controller Layer**: `ImageController` - Provides REST API endpoints for image download operations
3. **UI Layer**: JavaScript in `FieldMapping.cshtml` - Provides user interface for triggering downloads

### Data Flow

```
User Interface (Browser)
    ↓
ImageController (API Endpoint)
    ↓
ImageDownloadService (Download & ZIP Creation)
    ↓
HuvrApiClient (Fetch Defect & Overlay Data)
    ↓
External Image URLs (Download Images)
    ↓
ZIP Archive (Return to User)
```

---

## Components

### 1. ImageDownloadService (`HuvrWebApp/Services/ImageDownloadService.cs`)

A service responsible for:
- Downloading individual images from URLs
- Creating ZIP archives containing multiple images
- Handling specialized defect overlay image downloads

**Key Methods:**

```csharp
// Download a single image
Task<ImageDownloadResult> DownloadImageAsync(string url, string? fileName = null)

// Download multiple images as ZIP
Task<ZipDownloadResult> DownloadImagesAsZipAsync(List<ImageDownloadRequest> requests, string zipFileName)

// Download all defect overlay images
Task<ZipDownloadResult> DownloadDefectOverlayImagesAsync(List<DefectOverlayImageInfo> overlays, string defectId)
```

**Features:**
- Automatic file extension detection from Content-Type headers
- Duplicate filename handling with numerical suffixes
- Error handling for failed downloads
- Temporary file cleanup

### 2. ImageController (`HuvrWebApp/Controllers/ImageController.cs`)

REST API controller providing endpoints for image download operations.

**Endpoints:**

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/Image/DownloadDefectImages` | POST | Download all images for a specific defect |
| `/Image/DownloadImageFromUrl` | POST | Download a single image from a URL |
| `/Image/DownloadMultipleImages` | POST | Download multiple images as ZIP |

**Authentication:** All endpoints require a valid session (authenticated user).

### 3. User Interface

Located in `/Views/Excel/FieldMapping.cshtml`

**UI Elements:**
- **Download Images Button**: Appears when "Defects" entity type is selected
- **Loading Indicator**: Shows download progress
- **Success/Error Messages**: Provides user feedback

---

## API Endpoints

### 1. Download Defect Images

**Endpoint:** `POST /Image/DownloadDefectImages`

**Request Body:**
```json
{
  "DefectId": "string"
}
```

**Response:**
- **Success**: Binary ZIP file download
- **Error**: JSON error message

**Process:**
1. Fetches defect data including overlays from HUVR API
2. Extracts all image URLs from overlays:
   - `DisplayUrl` from overlay
   - `DownloadUrl` from overlay's Media
   - `ThumbnailUrl` from overlay's Media
   - `PreviewUrl` from overlay's Media
3. Downloads all images concurrently
4. Creates ZIP archive with proper file naming
5. Returns ZIP file to user

**File Naming Convention:**
```
defect_{defectId}_overlay_{overlayId}.jpg      - Display image
defect_{defectId}_media_{mediaId}.jpg          - Media download image
defect_{defectId}_media_{mediaId}_thumb.jpg    - Thumbnail
```

**Example Usage (JavaScript):**
```javascript
$.ajax({
    url: '/Image/DownloadDefectImages',
    type: 'POST',
    contentType: 'application/json',
    data: JSON.stringify({ DefectId: "123" }),
    xhrFields: { responseType: 'blob' },
    success: function(blob) {
        // Trigger file download
        var url = window.URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = 'defect_123_images.zip';
        a.click();
    }
});
```

### 2. Download Single Image

**Endpoint:** `POST /Image/DownloadImageFromUrl`

**Request Body:**
```json
{
  "Url": "https://example.com/image.jpg",
  "FileName": "optional_name.jpg"
}
```

**Response:** Binary image file

### 3. Download Multiple Images

**Endpoint:** `POST /Image/DownloadMultipleImages`

**Request Body:**
```json
{
  "ImageRequests": [
    {
      "Url": "https://example.com/image1.jpg",
      "FileName": "image1.jpg"
    },
    {
      "Url": "https://example.com/image2.jpg",
      "FileName": "image2.jpg"
    }
  ],
  "ZipFileName": "images.zip"
}
```

**Response:** Binary ZIP file

---

## User Interface

### Location

The image download button is available in the **Excel Field Mapping & Export** page:

**Navigation:** Home → Excel Export → Single Sheet Export

### Usage Flow

1. **Select Entity Type**: Choose "Defects (Findings)" from the dropdown
2. **Load Fields**: Click "Load Fields" button
3. **Download Images Button Appears**: A yellow "Download Images" button becomes visible
4. **Click Download Images**: Prompts for Defect ID
5. **Enter Defect ID**: Input the specific defect ID to download images for
6. **Processing**: Loading indicator shows progress
7. **Download**: ZIP file automatically downloads to browser

### Visual Elements

```
┌─────────────────────────────────────────────────────────┐
│ Entity Type: [Defects (Findings) ▼]                    │
│ [Load Fields]                                            │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ [✓ Export to Excel]  [⚠ Download Images]  [↻ Reset]    │
│ [+ Save as Template]  [📁 Manage Templates]             │
└─────────────────────────────────────────────────────────┘
```

**Button Styling:**
- **Color**: Warning (yellow/orange) to distinguish from export button
- **Icon**: Images icon (bi-images)
- **Visibility**: Only shown when entity type is "defects"

---

## Usage Guide

### For End Users

#### Downloading Images for a Single Defect

1. Navigate to **Excel Export → Single Sheet Export**
2. Select **"Defects (Findings)"** from Entity Type dropdown
3. Click **"Load Fields"**
4. Click the **"Download Images"** button (yellow/orange button)
5. When prompted, enter the Defect ID (e.g., "DEF-12345")
6. Wait for processing (loading indicator will show)
7. ZIP file will download automatically containing all overlay images

#### What Images Are Included?

For each defect overlay, the following images are downloaded (if available):
- **Display Image**: The main overlay visualization
- **Media Download Image**: Full-resolution media file
- **Thumbnail Image**: Smaller preview version
- **Preview Image**: Medium-sized preview (if available)

#### File Organization

All images are downloaded in a single ZIP file named:
```
defect_{DefectId}_images.zip
```

Inside the ZIP, files are organized with descriptive names:
```
defect_123_overlay_456.jpg
defect_123_media_789.jpg
defect_123_media_789_thumb.jpg
```

### For Developers

#### Adding Image Download to Other Pages

1. **Add Button to View:**
```html
<button id="downloadImagesBtn" class="btn btn-warning">
    <i class="bi bi-images me-2"></i>Download Images
</button>
```

2. **Add JavaScript Handler:**
```javascript
$('#downloadImagesBtn').click(function() {
    var defectId = prompt('Enter Defect ID:');
    if (defectId) {
        downloadDefectImages(defectId);
    }
});

function downloadDefectImages(defectId) {
    $.ajax({
        url: '@Url.Action("DownloadDefectImages", "Image")',
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
        }
    });
}
```

---

## Implementation Details

### Model Relationships

The feature relies on the model relationships defined in `ModelRelationshipConfiguration.cs`:

```csharp
["Defect"] = new List<RelationshipDefinition>
{
    new("DefectOverlay", "Id", "DefectId", "Defect overlays", isCollection: true)
},
["DefectOverlay"] = new List<RelationshipDefinition>
{
    new("Defect", "DefectId", "Id", "Parent defect"),
    new("InspectionMedia", "MediaId", "Id", "Associated media")
}
```

### Field Definitions

Available fields for DefectOverlay:
- `Id`, `DefectId`, `Note`, `DisplayUrl`, `GeometryExtra`, `CreatedOn`, `UpdatedOn`

Available fields for InspectionMedia:
- `Id`, `ProjectId`, `FileName`, `FileType`, `FileSize`, `Status`
- `DownloadUrl`, `ThumbnailUrl`, `PreviewUrl`, `UploadUrl`
- `CreatedAt`, `UpdatedAt`

### Image Download Process

```csharp
// 1. Fetch defect with overlays
var defect = await client.GetDefectByIdAsync(defectId);

// 2. Extract overlay information
foreach (var overlay in defect.Overlays)
{
    // Collect DisplayUrl
    if (!string.IsNullOrEmpty(overlay.DisplayUrl))
        imageRequests.Add(overlay.DisplayUrl);

    // Collect Media URLs
    if (overlay.Media != null)
    {
        if (!string.IsNullOrEmpty(overlay.Media.DownloadUrl))
            imageRequests.Add(overlay.Media.DownloadUrl);
        if (!string.IsNullOrEmpty(overlay.Media.ThumbnailUrl))
            imageRequests.Add(overlay.Media.ThumbnailUrl);
    }
}

// 3. Download all images
foreach (var request in imageRequests)
{
    var result = await DownloadImageAsync(request.Url);
    // Add to ZIP archive
}

// 4. Return ZIP file
return File(zipContent, "application/zip", zipFileName);
```

### Duplicate Filename Handling

When multiple overlays have the same filename:
```csharp
// Original: image.jpg
// Duplicate 1: image_2.jpg
// Duplicate 2: image_3.jpg
```

Implementation:
```csharp
if (fileNameCounts.ContainsKey(finalFileName))
{
    fileNameCounts[finalFileName]++;
    var nameWithoutExt = Path.GetFileNameWithoutExtension(finalFileName);
    var ext = Path.GetExtension(finalFileName);
    finalFileName = $"{nameWithoutExt}_{fileNameCounts[finalFileName]}{ext}";
}
```

---

## Error Handling

### Common Errors and Solutions

#### 1. "Defect not found"
**Cause:** Invalid or non-existent Defect ID
**Solution:** Verify the Defect ID exists in the system

#### 2. "No overlays found for this defect"
**Cause:** Defect has no associated overlays
**Solution:** Ensure the defect has been inspected and has overlay data

#### 3. "HTTP 404: Not Found" for image URL
**Cause:** Image URL is invalid or image has been deleted
**Solution:** Check if the image still exists at the source URL

#### 4. "Not authenticated"
**Cause:** User session has expired
**Solution:** Log in again

#### 5. Network timeout
**Cause:** Downloading large images or slow connection
**Solution:** HttpClient timeout is set to 5 minutes; increase if needed

### Error Response Format

```json
{
  "error": "Description of the error"
}
```

### Service-Level Error Handling

```csharp
public async Task<ImageDownloadResult> DownloadImageAsync(string url, string? fileName = null)
{
    try
    {
        // Download logic
    }
    catch (Exception ex)
    {
        return new ImageDownloadResult
        {
            Success = false,
            Error = ex.Message
        };
    }
}
```

---

## Future Enhancements

### Planned Features

1. **Bulk Defect Image Download**
   - Download images for all defects in a project
   - Batch processing with progress indicator
   - Organized folder structure in ZIP

2. **Image Preview Before Download**
   - Show thumbnails of available images
   - Allow selective image download
   - Display image metadata (size, type, date)

3. **Image Filtering Options**
   - Filter by image type (display, thumbnail, preview)
   - Filter by date range
   - Filter by file size

4. **Progress Tracking**
   - Real-time download progress bar
   - Individual image download status
   - Estimated time remaining

5. **Background Processing**
   - Queue large download jobs
   - Email notification when download is ready
   - Download history and retry failed downloads

6. **Image Processing Options**
   - Resize images before download
   - Convert image formats
   - Add watermarks

7. **Integration with Multi-Sheet Export**
   - Download images for all defects in selected sheets
   - Coordinate image downloads with Excel export

### Implementation Considerations

#### Bulk Download Architecture

For downloading images from multiple defects:

```csharp
public async Task<ZipDownloadResult> DownloadProjectDefectImagesAsync(string projectId)
{
    // 1. Fetch all defects for project
    var defects = await client.GetDefectsByProjectIdAsync(projectId);

    // 2. Create organized ZIP structure
    // project_{projectId}_defect_images.zip
    //   ├─ defect_123/
    //   │   ├─ overlay_1.jpg
    //   │   └─ overlay_2.jpg
    //   └─ defect_456/
    //       └─ overlay_1.jpg

    // 3. Download with progress tracking
    // 4. Return ZIP
}
```

---

## Technical Specifications

### Dependencies

- **ClosedXML**: Not used for image downloads, but available in project
- **Newtonsoft.Json**: JSON parsing for API responses
- **System.IO.Compression**: ZIP file creation
- **.NET HttpClient**: Image downloading

### Configuration

**HttpClient Timeout:**
```csharp
_httpClient.Timeout = TimeSpan.FromMinutes(5);
```

**Downloads Directory:**
```csharp
Path.Combine(environment.ContentRootPath, "Data", "Downloads")
```

### Performance Considerations

- **Concurrent Downloads**: Images are downloaded sequentially to avoid overwhelming the server
- **Memory Usage**: Large images are streamed directly to ZIP without holding entire file in memory
- **Temporary Files**: ZIP files are created in temp directory and cleaned up after sending
- **Caching**: No caching implemented; images are downloaded fresh each time

### Security Considerations

1. **URL Validation**: Only downloads images from defect overlay data (trusted source)
2. **Authentication**: All endpoints require valid session
3. **File Type Validation**: Content-Type headers are checked
4. **Path Traversal Prevention**: Filenames are sanitized
5. **Size Limits**: Individual file size not limited; consider adding limits for production

---

## Testing Guide

### Manual Testing

1. **Test Single Defect with Overlays:**
   - Find a defect ID with known overlays
   - Use Download Images button
   - Verify ZIP contains expected images

2. **Test Defect Without Overlays:**
   - Use defect ID with no overlays
   - Verify appropriate error message

3. **Test Invalid Defect ID:**
   - Enter non-existent defect ID
   - Verify "Defect not found" error

4. **Test Network Failure:**
   - Use defect with invalid image URLs
   - Verify partial success (some images downloaded)

### Automated Testing

#### Unit Test Example

```csharp
[Fact]
public async Task DownloadImageAsync_ValidUrl_ReturnsSuccess()
{
    // Arrange
    var service = new ImageDownloadService(httpClientFactory, environment);
    var url = "https://example.com/test-image.jpg";

    // Act
    var result = await service.DownloadImageAsync(url);

    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.Content);
    Assert.NotEmpty(result.FileName);
}
```

---

## Support and Troubleshooting

### Common Issues

**Q: Images are not downloading for a specific defect**
A: Check if the defect has overlays with valid image URLs. Verify the URLs are accessible.

**Q: ZIP file is empty**
A: All image downloads may have failed. Check network connectivity and URL validity.

**Q: Download is very slow**
A: Large image files or slow network connection. Consider implementing progress tracking.

**Q: "Download Images" button doesn't appear**
A: Ensure "Defects" is selected as the entity type and fields have been loaded.

### Debug Logging

Enable detailed logging in `ImageDownloadService`:

```csharp
Console.WriteLine($"Downloading image from: {url}");
Console.WriteLine($"HTTP Status: {response.StatusCode}");
Console.WriteLine($"Content-Type: {response.Content.Headers.ContentType?.MediaType}");
Console.WriteLine($"Content-Length: {content.Length} bytes");
```

---

## Changelog

### Version 1.0 (Current)
- Initial implementation
- Single defect image download
- ZIP archive creation
- UI integration in FieldMapping page
- Error handling and user feedback

### Planned for Version 1.1
- Bulk defect image download
- Progress tracking
- Image preview functionality

---

## Contact and Contributions

For questions, bug reports, or feature requests related to the image download feature, please contact the development team or create an issue in the project repository.

**Key Files:**
- Service: `/HuvrWebApp/Services/ImageDownloadService.cs`
- Controller: `/HuvrWebApp/Controllers/ImageController.cs`
- UI: `/HuvrWebApp/Views/Excel/FieldMapping.cshtml`
- Models: `/HuvrWebApp/Services/ImageDownloadService.cs` (bottom of file)
