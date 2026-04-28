using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/upload")]
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
