using Microsoft.Extensions.Configuration;
using Norta.Api.Models;
using Norta.Api.Services;
using Xunit;

namespace Norta.Tests.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestSecretKeyForJwtTokenGeneration123!",
                ["Jwt:Issuer"] = "norta.test",
                ["Jwt:Audience"] = "norta.test"
            })
            .Build();

        _jwtService = new JwtService(configuration);
    }

    [Fact]
    public void GenerateAccessToken_ReturnsValidToken()
    {
        // Arrange
        var user = new AppUser
        {
            Id = "user123",
            Email = "test@example.com",
            DisplayName = "Test User"
        };

        // Act
        var token = _jwtService.GenerateAccessToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsValidToken()
    {
        // Act
        var token = _jwtService.GenerateRefreshToken();

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task ValidateRefreshToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid_token";

        // Act
        var userId = await _jwtService.ValidateRefreshToken(invalidToken);

        // Assert
        Assert.Null(userId);
    }
}
