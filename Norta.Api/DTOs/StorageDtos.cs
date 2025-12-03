using System.ComponentModel.DataAnnotations;

namespace Norta.Api.DTOs;

public record SignedUrlRequest(
    [Required] string FileName,
    [Required] string ContentType
);

public record SignedUrlResponse(
    string UploadUrl,
    string PublicUrl
);

public record UploadResponse(
    string Url
);
