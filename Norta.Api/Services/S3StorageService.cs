using Amazon.S3;
using Amazon.S3.Model;

namespace Norta.Api.Services;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3StorageService(IConfiguration config)
    {
        var accessKey = config["Storage:S3:AccessKey"] ?? throw new InvalidOperationException("S3 access key not configured");
        var secretKey = config["Storage:S3:SecretKey"] ?? throw new InvalidOperationException("S3 secret key not configured");
        var region = config["Storage:S3:Region"] ?? "us-east-1";

        _s3Client = new AmazonS3Client(accessKey, secretKey, Amazon.RegionEndpoint.GetBySystemName(region));
        _bucketName = config["Storage:S3:BucketName"] ?? "norta-uploads";
    }

    public Task<(string UploadUrl, string PublicUrl)> GenerateSignedUploadUrlAsync(string fileName, string contentType)
    {
        var key = $"{Guid.NewGuid()}/{fileName}";

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(30),
            ContentType = contentType
        };

        var uploadUrl = _s3Client.GetPreSignedURL(request);
        var publicUrl = $"https://{_bucketName}.s3.amazonaws.com/{key}";

        return Task.FromResult((uploadUrl, publicUrl));
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var key = $"{Guid.NewGuid()}/{fileName}";

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(request);
        return $"https://{_bucketName}.s3.amazonaws.com/{key}";
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var key = uri.AbsolutePath.TrimStart('/');

            await _s3Client.DeleteObjectAsync(_bucketName, key);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
