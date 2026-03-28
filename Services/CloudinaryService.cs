using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        var cloudName = config["Cloudinary:CloudName"];
        var apiKey = config["Cloudinary:ApiKey"];
        var apiSecret = config["Cloudinary:ApiSecret"];

        if (string.IsNullOrWhiteSpace(cloudName) ||
            string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(apiSecret))
        {
            throw new Exception("Cloudinary configuration missing or invalid");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }


    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new Exception("File is empty");

        using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "products",
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        // 🔴 VERY IMPORTANT CHECK
        if (result.Error != null)
        {
            throw new Exception($"Cloudinary error: {result.Error.Message}");
        }

        if (result.SecureUrl == null)
        {
            throw new Exception("Cloudinary upload failed: SecureUrl is null");
        }

        return result.SecureUrl.ToString();
    }

}
