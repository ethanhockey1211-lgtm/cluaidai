namespace Norta.Api.Services;

public interface IStorageService
{
    Task<(string UploadUrl, string PublicUrl)> GenerateSignedUploadUrlAsync(string fileName, string contentType);
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteFileAsync(string fileUrl);
}
