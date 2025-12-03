namespace Norta.Api.Services;

public class LocalStorageService : IStorageService
{
    private readonly string _uploadsPath;
    private readonly string _baseUrl;

    public LocalStorageService(IConfiguration config, IWebHostEnvironment env)
    {
        _uploadsPath = Path.Combine(env.WebRootPath, "uploads");
        Directory.CreateDirectory(_uploadsPath);
        _baseUrl = config["Storage:Local:BaseUrl"] ?? "https://localhost:5001";
    }

    public Task<(string UploadUrl, string PublicUrl)> GenerateSignedUploadUrlAsync(string fileName, string contentType)
    {
        // For local storage, we'll use direct upload instead of signed URLs
        var fileId = Guid.NewGuid().ToString();
        var extension = Path.GetExtension(fileName);
        var storedFileName = $"{fileId}{extension}";
        var publicUrl = $"{_baseUrl}/uploads/{storedFileName}";

        // Return the same URL for both (client will POST to API endpoint instead)
        return Task.FromResult((publicUrl, publicUrl));
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var fileId = Guid.NewGuid().ToString();
        var extension = Path.GetExtension(fileName);
        var storedFileName = $"{fileId}{extension}";
        var filePath = Path.Combine(_uploadsPath, storedFileName);

        using var fileWriteStream = File.Create(filePath);
        await fileStream.CopyToAsync(fileWriteStream);

        return $"{_baseUrl}/uploads/{storedFileName}";
    }

    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);
            var filePath = Path.Combine(_uploadsPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
