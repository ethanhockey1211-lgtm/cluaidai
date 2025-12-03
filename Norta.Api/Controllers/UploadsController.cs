using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Norta.Api.DTOs;
using Norta.Api.Services;

namespace Norta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadsController : ControllerBase
{
    private readonly IStorageService _storageService;

    public UploadsController(IStorageService storageService)
    {
        _storageService = storageService;
    }

    [HttpPost("signed-url")]
    public async Task<ActionResult<SignedUrlResponse>> GetSignedUrl([FromBody] SignedUrlRequest request)
    {
        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(request.ContentType.ToLower()))
        {
            return BadRequest(new { message = "Invalid file type. Only images are allowed." });
        }

        var (uploadUrl, publicUrl) = await _storageService.GenerateSignedUploadUrlAsync(
            request.FileName,
            request.ContentType
        );

        return Ok(new SignedUrlResponse(uploadUrl, publicUrl));
    }

    [HttpPost("direct")]
    [RequestSizeLimit(10_000_000)] // 10MB
    public async Task<ActionResult<UploadResponse>> DirectUpload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType.ToLower()))
        {
            return BadRequest(new { message = "Invalid file type. Only images are allowed." });
        }

        // Validate file size (max 10MB)
        if (file.Length > 10_000_000)
        {
            return BadRequest(new { message = "File too large. Maximum size is 10MB." });
        }

        using var stream = file.OpenReadStream();
        var url = await _storageService.UploadFileAsync(stream, file.FileName, file.ContentType);

        return Ok(new UploadResponse(url));
    }
}
