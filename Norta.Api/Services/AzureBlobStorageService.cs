using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace Norta.Api.Services;

public class AzureBlobStorageService : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public AzureBlobStorageService(IConfiguration config)
    {
        var connectionString = config["Storage:AzureBlob:ConnectionString"]
            ?? throw new InvalidOperationException("Azure Blob connection string not configured");
        _blobServiceClient = new BlobServiceClient(connectionString);
        _containerName = config["Storage:AzureBlob:ContainerName"] ?? "norta-uploads";
    }

    public async Task<(string UploadUrl, string PublicUrl)> GenerateSignedUploadUrlAsync(string fileName, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobName = $"{Guid.NewGuid()}/{fileName}";
        var blobClient = containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        var publicUrl = blobClient.Uri.ToString();

        return (sasUri.ToString(), publicUrl);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobName = $"{Guid.NewGuid()}/{fileName}";
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(fileStream, new Azure.Storage.Blobs.Models.BlobHttpHeaders
        {
            ContentType = contentType
        });

        return blobClient.Uri.ToString();
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var uri = new Uri(fileUrl);
            var blobName = uri.Segments.Length > 2 ? string.Join("", uri.Segments[2..]) : uri.Segments[^1];
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
