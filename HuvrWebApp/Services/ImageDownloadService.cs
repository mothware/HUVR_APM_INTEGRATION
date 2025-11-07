using System.IO.Compression;

namespace HuvrWebApp.Services;

/// <summary>
/// Service for downloading images from URLs
/// </summary>
public class ImageDownloadService
{
    private readonly HttpClient _httpClient;
    private readonly IWebHostEnvironment _environment;
    private readonly string _downloadsDirectory;

    public ImageDownloadService(IHttpClientFactory httpClientFactory, IWebHostEnvironment environment)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
        _environment = environment;
        _downloadsDirectory = Path.Combine(environment.ContentRootPath, "Data", "Downloads");

        // Ensure downloads directory exists
        Directory.CreateDirectory(_downloadsDirectory);
    }

    /// <summary>
    /// Download a single image from URL
    /// </summary>
    public async Task<ImageDownloadResult> DownloadImageAsync(string url, string? fileName = null)
    {
        try
        {
            if (string.IsNullOrEmpty(url))
            {
                return new ImageDownloadResult
                {
                    Success = false,
                    Error = "URL is empty"
                };
            }

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return new ImageDownloadResult
                {
                    Success = false,
                    Error = $"HTTP {response.StatusCode}: {response.ReasonPhrase}"
                };
            }

            var content = await response.Content.ReadAsByteArrayAsync();

            // Determine file name
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = GetFileNameFromUrl(url) ?? $"image_{Guid.NewGuid()}.jpg";
            }

            // Ensure valid file extension
            if (!Path.HasExtension(fileName))
            {
                var contentType = response.Content.Headers.ContentType?.MediaType;
                var extension = GetExtensionFromContentType(contentType);
                fileName += extension;
            }

            return new ImageDownloadResult
            {
                Success = true,
                FileName = fileName,
                Content = content,
                ContentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg",
                Size = content.Length
            };
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

    /// <summary>
    /// Download multiple images and create a ZIP file
    /// </summary>
    public async Task<ZipDownloadResult> DownloadImagesAsZipAsync(List<ImageDownloadRequest> requests, string zipFileName = "images.zip")
    {
        var result = new ZipDownloadResult
        {
            ZipFileName = zipFileName,
            DownloadedImages = new List<ImageDownloadResult>()
        };

        try
        {
            var tempZipPath = Path.Combine(_downloadsDirectory, $"{Guid.NewGuid()}.zip");

            using (var zipArchive = ZipFile.Open(tempZipPath, ZipArchiveMode.Create))
            {
                var fileNameCounts = new Dictionary<string, int>();

                foreach (var request in requests)
                {
                    var downloadResult = await DownloadImageAsync(request.Url, request.FileName);
                    result.DownloadedImages.Add(downloadResult);

                    if (downloadResult.Success && downloadResult.Content != null)
                    {
                        // Handle duplicate file names
                        var finalFileName = downloadResult.FileName;
                        if (fileNameCounts.ContainsKey(finalFileName))
                        {
                            fileNameCounts[finalFileName]++;
                            var nameWithoutExt = Path.GetFileNameWithoutExtension(finalFileName);
                            var ext = Path.GetExtension(finalFileName);
                            finalFileName = $"{nameWithoutExt}_{fileNameCounts[finalFileName]}{ext}";
                        }
                        else
                        {
                            fileNameCounts[finalFileName] = 1;
                        }

                        // Add to ZIP
                        var entry = zipArchive.CreateEntry(finalFileName);
                        using (var entryStream = entry.Open())
                        {
                            await entryStream.WriteAsync(downloadResult.Content, 0, downloadResult.Content.Length);
                        }

                        result.SuccessCount++;
                    }
                    else
                    {
                        result.FailedCount++;
                    }
                }
            }

            // Read the ZIP file
            result.ZipContent = await File.ReadAllBytesAsync(tempZipPath);
            result.Success = true;

            // Clean up temp file
            File.Delete(tempZipPath);

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Error = ex.Message;
            return result;
        }
    }

    /// <summary>
    /// Download defect overlay images
    /// </summary>
    public async Task<ZipDownloadResult> DownloadDefectOverlayImagesAsync(List<DefectOverlayImageInfo> overlays, string defectId)
    {
        var requests = new List<ImageDownloadRequest>();

        foreach (var overlay in overlays)
        {
            if (!string.IsNullOrEmpty(overlay.DisplayUrl))
            {
                requests.Add(new ImageDownloadRequest
                {
                    Url = overlay.DisplayUrl,
                    FileName = $"defect_{defectId}_overlay_{overlay.OverlayId}.jpg"
                });
            }

            if (overlay.Media != null)
            {
                if (!string.IsNullOrEmpty(overlay.Media.DownloadUrl))
                {
                    var fileName = !string.IsNullOrEmpty(overlay.Media.FileName)
                        ? overlay.Media.FileName
                        : $"defect_{defectId}_media_{overlay.Media.Id}.jpg";

                    requests.Add(new ImageDownloadRequest
                    {
                        Url = overlay.Media.DownloadUrl,
                        FileName = fileName
                    });
                }

                if (!string.IsNullOrEmpty(overlay.Media.ThumbnailUrl))
                {
                    requests.Add(new ImageDownloadRequest
                    {
                        Url = overlay.Media.ThumbnailUrl,
                        FileName = $"defect_{defectId}_media_{overlay.Media.Id}_thumb.jpg"
                    });
                }
            }
        }

        if (requests.Count == 0)
        {
            return new ZipDownloadResult
            {
                Success = false,
                Error = "No image URLs found in overlays"
            };
        }

        return await DownloadImagesAsZipAsync(requests, $"defect_{defectId}_images.zip");
    }

    private string? GetFileNameFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            var fileName = Path.GetFileName(uri.LocalPath);
            return string.IsNullOrEmpty(fileName) ? null : fileName;
        }
        catch
        {
            return null;
        }
    }

    private string GetExtensionFromContentType(string? contentType)
    {
        return contentType?.ToLower() switch
        {
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/bmp" => ".bmp",
            "image/webp" => ".webp",
            "image/tiff" => ".tiff",
            _ => ".jpg"
        };
    }
}

/// <summary>
/// Result of a single image download
/// </summary>
public class ImageDownloadResult
{
    public bool Success { get; set; }
    public string? FileName { get; set; }
    public byte[]? Content { get; set; }
    public string? ContentType { get; set; }
    public long Size { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Result of downloading multiple images as a ZIP
/// </summary>
public class ZipDownloadResult
{
    public bool Success { get; set; }
    public string ZipFileName { get; set; } = string.Empty;
    public byte[]? ZipContent { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<ImageDownloadResult> DownloadedImages { get; set; } = new();
    public string? Error { get; set; }
}

/// <summary>
/// Request to download an image
/// </summary>
public class ImageDownloadRequest
{
    public string Url { get; set; } = string.Empty;
    public string? FileName { get; set; }
}

/// <summary>
/// Information about defect overlay images
/// </summary>
public class DefectOverlayImageInfo
{
    public string OverlayId { get; set; } = string.Empty;
    public string? DisplayUrl { get; set; }
    public MediaInfo? Media { get; set; }
}

/// <summary>
/// Media information for overlay
/// </summary>
public class MediaInfo
{
    public string? Id { get; set; }
    public string? FileName { get; set; }
    public string? DownloadUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? PreviewUrl { get; set; }
}
