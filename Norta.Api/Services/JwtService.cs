using Microsoft.IdentityModel.Tokens;
using Norta.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Norta.Api.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;
    private readonly Dictionary<string, (string userId, DateTime expiry)> _refreshTokens = new();

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["Jwt:Key"] ?? "NortaSecretKeyChangeInProduction123!"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("displayName", user.DisplayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddHours(1);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "norta.app",
            audience: _config["Jwt:Audience"] ?? "norta.app",
            claims: claims,
            expires: expiry,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public Task<string?> ValidateRefreshToken(string refreshToken)
    {
        // In production, store refresh tokens in database with expiry
        if (_refreshTokens.TryGetValue(refreshToken, out var tokenData))
        {
            if (tokenData.expiry > DateTime.UtcNow)
            {
                return Task.FromResult<string?>(tokenData.userId);
            }
            _refreshTokens.Remove(refreshToken);
        }
        return Task.FromResult<string?>(null);
    }

    public void StoreRefreshToken(string refreshToken, string userId, DateTime expiry)
    {
        _refreshTokens[refreshToken] = (userId, expiry);
    }
}
