using FinanceTracker.Application.DTOs.Auth;
using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly ILogger<AuthController> _logger;
		private readonly IConfiguration _configuration;
		private readonly IWebHostEnvironment _environment;

		public AuthController(IAuthService authService, ILogger<AuthController> logger,
			IConfiguration configuration, IWebHostEnvironment environment)
		{
			_authService = authService;
			_logger = logger;
			_configuration = configuration;
			_environment = environment;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDTO dto)
		{
			_logger.LogInformation("Login attempt for {Email}", dto.Email);
			var result = await _authService.LoginAsync(dto);
			if (!result.IsSuccess)
			{
				_logger.LogInformation("Login failed for {Email}", dto.Email);
				return StatusCode(result.StatusCode, result);
			}

			_logger.LogInformation("Login succeeded for {Email}", dto.Email);
			SetRefreshTokenCookie(result.Data!.RefreshToken);
			var response = result.Data with { RefreshToken = string.Empty };
			return StatusCode(result.StatusCode, Result<AuthResponseDTO>.Success(response));
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
		{
			_logger.LogInformation("Registration attempt for {Email}", dto.Email);
			var result = await _authService.RegisterAsync(dto);
			if (!result.IsSuccess)
			{
				_logger.LogInformation("Registration failed for {Email}", dto.Email);
				return StatusCode(result.StatusCode, result);
			}

			_logger.LogInformation("Registration succeeded for {Email}", dto.Email);
			SetRefreshTokenCookie(result.Data!.RefreshToken);
			var response = result.Data with { RefreshToken = string.Empty };
			return StatusCode(result.StatusCode, Result<AuthResponseDTO>.Created(response));
		}

		[HttpPost("refresh")]
		public async Task<IActionResult> Refresh()
		{
			var refreshToken = Request.Cookies["refreshToken"];
			if (string.IsNullOrEmpty(refreshToken))
				return StatusCode(401, Result<AuthResponseDTO>.Unauthorized("No refresh token"));

			var result = await _authService.RefreshTokenAsync(refreshToken);
			if (!result.IsSuccess)
			{
				ClearRefreshTokenCookie();
				return StatusCode(result.StatusCode, result);
			}

			SetRefreshTokenCookie(result.Data!.RefreshToken);
			var response = result.Data with { RefreshToken = string.Empty };
			return StatusCode(result.StatusCode, Result<AuthResponseDTO>.Success(response));
		}

		[Authorize]
		[HttpPost("logout")]
		public async Task<IActionResult> Logout()
		{
			var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var result = await _authService.LogoutAsync(userId);
			ClearRefreshTokenCookie();
			return StatusCode(result.StatusCode, result);
		}

		private void SetRefreshTokenCookie(string token)
		{
			var expirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationInDays"] ?? "7");
			Response.Cookies.Append("refreshToken", token, new CookieOptions
			{
				HttpOnly = true,
				Secure = !_environment.IsDevelopment(),
				SameSite = SameSiteMode.Strict,
				Path = "/api/auth",
				//Expires = DateTimeOffset.UtcNow.AddMinutes(2)
				Expires = DateTimeOffset.UtcNow.AddDays(expirationDays)
			});
		}

		private void ClearRefreshTokenCookie()
		{
			Response.Cookies.Append("refreshToken", string.Empty, new CookieOptions
			{
				HttpOnly = true,
				Secure = !_environment.IsDevelopment(),
				SameSite = SameSiteMode.Strict,
				Path = "/api/auth",
				Expires = PhilippineDateTime.Now.AddDays(-1)
			});
		}
	}
}
