using Microsoft.AspNetCore.Mvc;
using HuvrWebApp.Services;

namespace HuvrWebApp.Controllers
{
    public class ImageController : Controller
    {
        private readonly IHuvrService _huvrService;
        private readonly ImageDownloadService _imageDownloadService;

        public ImageController(IHuvrService huvrService, ImageDownloadService imageDownloadService)
        {
            _huvrService = huvrService;
            _imageDownloadService = imageDownloadService;
        }

        private HuvrApiClient.HuvrApiClient? GetClientFromSession()
        {
            var sessionId = HttpContext.Session.GetString("SessionId");
            if (sessionId == null)
            {
                return null;
            }
            return _huvrService.GetClient(sessionId);
        }

        /// <summary>
        /// Download all images for a defect's overlays as a ZIP file
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DownloadDefectImages([FromBody] DownloadDefectImagesRequest request)
        {
            var client = GetClientFromSession();
            if (client == null)
            {
                return Unauthorized(new { error = "Not authenticated" });
            }

            try
            {
                // Fetch the defect with overlays
                var defect = await client.GetDefectAsync(request.DefectId);
                if (defect == null)
                {
                    return NotFound(new { error = "Defect not found" });
                }

                if (defect.Overlays == null || defect.Overlays.Count == 0)
                {
                    return BadRequest(new { error = "No overlays found for this defect" });
                }

                // Build list of overlay image info
                var overlayImageInfos = new List<DefectOverlayImageInfo>();
                foreach (var overlay in defect.Overlays)
                {
                    var overlayInfo = new DefectOverlayImageInfo
                    {
                        OverlayId = overlay.Id ?? "",
                        DisplayUrl = overlay.DisplayUrl
                    };

                    if (overlay.Media != null)
                    {
                        overlayInfo.Media = new MediaInfo
                        {
                            Id = overlay.Media.Id,
                            FileName = overlay.Media.FileName,
                            DownloadUrl = overlay.Media.DownloadUrl,
                            ThumbnailUrl = overlay.Media.ThumbnailUrl,
                            PreviewUrl = overlay.Media.PreviewUrl
                        };
                    }

                    overlayImageInfos.Add(overlayInfo);
                }

                // Download images as ZIP
                var result = await _imageDownloadService.DownloadDefectOverlayImagesAsync(
                    overlayImageInfos,
                    request.DefectId
                );

                if (!result.Success)
                {
                    return BadRequest(new { error = result.Error });
                }

                // Return ZIP file
                return File(
                    result.ZipContent!,
                    "application/zip",
                    result.ZipFileName
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Download a single image from a URL
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DownloadImageFromUrl([FromBody] DownloadImageRequest request)
        {
            var client = GetClientFromSession();
            if (client == null)
            {
                return Unauthorized(new { error = "Not authenticated" });
            }

            try
            {
                var result = await _imageDownloadService.DownloadImageAsync(request.Url, request.FileName);

                if (!result.Success)
                {
                    return BadRequest(new { error = result.Error });
                }

                return File(
                    result.Content!,
                    result.ContentType!,
                    result.FileName!
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Download multiple images as a ZIP file
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DownloadMultipleImages([FromBody] DownloadMultipleImagesRequest request)
        {
            var client = GetClientFromSession();
            if (client == null)
            {
                return Unauthorized(new { error = "Not authenticated" });
            }

            try
            {
                var result = await _imageDownloadService.DownloadImagesAsZipAsync(
                    request.ImageRequests,
                    request.ZipFileName ?? "images.zip"
                );

                if (!result.Success)
                {
                    return BadRequest(new { error = result.Error });
                }

                return File(
                    result.ZipContent!,
                    "application/zip",
                    result.ZipFileName
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Request models for the endpoints

    public class DownloadDefectImagesRequest
    {
        public string DefectId { get; set; } = string.Empty;
    }

    public class DownloadImageRequest
    {
        public string Url { get; set; } = string.Empty;
        public string? FileName { get; set; }
    }

    public class DownloadMultipleImagesRequest
    {
        public List<ImageDownloadRequest> ImageRequests { get; set; } = new();
        public string? ZipFileName { get; set; }
    }
}
