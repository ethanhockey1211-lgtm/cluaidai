using Norta.Api.Models;

namespace Norta.Api.Services;

public interface IJwtService
{
    string GenerateAccessToken(AppUser user);
    string GenerateRefreshToken();
    Task<string?> ValidateRefreshToken(string refreshToken);
}
