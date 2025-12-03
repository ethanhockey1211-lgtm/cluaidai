using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Norta.Api.DTOs;
using Norta.Api.Models;
using Norta.Api.Services;

namespace Norta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtService _jwtService;

    public AuthController(UserManager<AppUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "User with this email already exists" });
        }

        var user = new AppUser
        {
            Email = request.Email,
            UserName = request.Email,
            DisplayName = request.DisplayName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Failed to create user", errors = result.Errors });
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token (in production, store in database)
        if (_jwtService is JwtService jwtServiceImpl)
        {
            jwtServiceImpl.StoreRefreshToken(refreshToken, user.Id, DateTime.UtcNow.AddDays(30));
        }

        return Ok(new AuthResponse(
            user.Id,
            user.Email!,
            user.DisplayName,
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddHours(1)
        ));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Store refresh token
        if (_jwtService is JwtService jwtServiceImpl)
        {
            jwtServiceImpl.StoreRefreshToken(refreshToken, user.Id, DateTime.UtcNow.AddDays(30));
        }

        return Ok(new AuthResponse(
            user.Id,
            user.Email!,
            user.DisplayName,
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddHours(1)
        ));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshToken(RefreshTokenRequest request)
    {
        var userId = await _jwtService.ValidateRefreshToken(request.RefreshToken);
        if (userId == null)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized(new { message = "User not found" });
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Store new refresh token
        if (_jwtService is JwtService jwtServiceImpl)
        {
            jwtServiceImpl.StoreRefreshToken(newRefreshToken, user.Id, DateTime.UtcNow.AddDays(30));
        }

        return Ok(new AuthResponse(
            user.Id,
            user.Email!,
            user.DisplayName,
            accessToken,
            newRefreshToken,
            DateTime.UtcNow.AddHours(1)
        ));
    }
}
