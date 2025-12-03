using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Norta.Api.Controllers;
using Norta.Api.DTOs;
using Norta.Api.Models;
using Norta.Api.Services;
using Xunit;

namespace Norta.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var userStoreMock = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        _jwtServiceMock = new Mock<IJwtService>();
        _controller = new AuthController(_userManagerMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var request = new RegisterRequest(
            "test@example.com",
            "Password123!",
            "Test User"
        );

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((AppUser?)null);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _jwtServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<AppUser>()))
            .Returns("test_access_token");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("test_refresh_token");

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal("test_access_token", response.AccessToken);
        Assert.Equal("test_refresh_token", response.RefreshToken);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest(
            "existing@example.com",
            "Password123!",
            "Test User"
        );

        var existingUser = new AppUser { Email = request.Email };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _controller.Register(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkResult()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "Password123!");
        var user = new AppUser
        {
            Id = "user123",
            Email = request.Email,
            DisplayName = "Test User"
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(true);

        _jwtServiceMock
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("test_access_token");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("test_refresh_token");

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuthResponse>(okResult.Value);
        Assert.Equal(user.Id, response.UserId);
        Assert.Equal("test_access_token", response.AccessToken);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "WrongPassword");
        var user = new AppUser { Email = request.Email };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Login(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
}
