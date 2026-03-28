using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly ICloudinaryService _cloudinary;

    public UploadController(ICloudinaryService cloudinary)
    {
        _cloudinary = cloudinary;
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null)
            return BadRequest("No file selected");

        var url = await _cloudinary.UploadImageAsync(file);
        return Ok(new { imageUrl = url });
    }
}
