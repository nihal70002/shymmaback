using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

[ApiController]
[Route("api/upload")]
[Authorize(Roles = "Admin")]
public class UploadController : ControllerBase
{
    private readonly ICloudinaryService _cloudinary;
    private readonly ILogger<UploadController> _logger;

    public UploadController(ICloudinaryService cloudinary, ILogger<UploadController> logger)
    {
        _cloudinary = cloudinary;
        _logger = logger;
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file selected");

        try
        {
            var url = await _cloudinary.UploadImageAsync(file);
            return Ok(new { imageUrl = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image upload failed. FileName={FileName}, ContentType={ContentType}, Length={Length}", file.FileName, file.ContentType, file.Length);
            return StatusCode(500, new { message = "Image upload failed", detail = ex.Message });
        }
    }

    [HttpPost("video")]
    [RequestTimeout(300)] // 5 minutes timeout
    public async Task<IActionResult> UploadVideo([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No video selected" });

        // More flexible file type validation
        var allowedVideoTypes = new[] { 
            "video/mp4", "video/avi", "video/mov", "video/wmv", 
            "video/flv", "video/webm", "video/quicktime", "video/x-msvideo" 
        };
        
        if (!allowedVideoTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(new { 
                message = "Invalid video format. Supported formats: MP4, AVI, MOV, WMV, FLV, WebM",
                receivedType = file.ContentType,
                supportedTypes = allowedVideoTypes
            });
        }

        // Increased file size limit (max 100MB)
        if (file.Length > 100 * 1024 * 1024)
        {
            return BadRequest(new { 
                message = "Video file size must be less than 100MB",
                receivedSize = file.Length,
                maxSize = 100 * 1024 * 1024
            });
        }

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("Starting video upload. FileName={FileName}, Size={Size}MB", 
                file.FileName, file.Length / (1024 * 1024));

            var url = await _cloudinary.UploadVideoAsync(file);
            
            stopwatch.Stop();
            _logger.LogInformation("Video upload completed. FileName={FileName}, Duration={Duration}s", 
                file.FileName, stopwatch.Elapsed.TotalSeconds);

            return Ok(new { 
                videoUrl = url,
                uploadTime = stopwatch.Elapsed.TotalSeconds,
                fileSize = file.Length
            });
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Video upload timed out. FileName={FileName}", file.FileName);
            return StatusCode(408, new { 
                message = "Upload timed out. Please try again with a smaller file or check your internet connection.",
                timeout = "5 minutes"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Video upload failed. FileName={FileName}, ContentType={ContentType}, Length={Length}", 
                file.FileName, file.ContentType, file.Length);
            return StatusCode(500, new { 
                message = "Video upload failed", 
                detail = ex.Message,
                fileName = file.FileName
            });
        }
    }
}
