using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> UploadVideo([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No video selected");

        // Validate file type
        var allowedVideoTypes = new[] { "video/mp4", "video/avi", "video/mov", "video/wmv", "video/flv", "video/webm" };
        if (!allowedVideoTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(new { message = "Invalid video format. Supported formats: MP4, AVI, MOV, WMV, FLV, WebM" });
        }

        // Validate file size (max 50MB)
        if (file.Length > 50 * 1024 * 1024)
        {
            return BadRequest(new { message = "Video file size must be less than 50MB" });
        }

        try
        {
            var url = await _cloudinary.UploadVideoAsync(file);
            return Ok(new { videoUrl = url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Video upload failed. FileName={FileName}, ContentType={ContentType}, Length={Length}", file.FileName, file.ContentType, file.Length);
            return StatusCode(500, new { message = "Video upload failed", detail = ex.Message });
        }
    }
}
