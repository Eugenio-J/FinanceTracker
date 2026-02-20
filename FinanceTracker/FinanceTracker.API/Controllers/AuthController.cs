using FinanceTracker.Application.DTOs.Auth;
using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly ILogger<AuthController> _logger;

		public AuthController(IAuthService authService, ILogger<AuthController> logger)
		{
			_authService = authService;
			_logger = logger;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDTO dto)
		{
			_logger.LogInformation("Login attempt for {Email}", dto.Email);
			var result = await _authService.LoginAsync(dto);
			if (!result.IsSuccess)
				_logger.LogInformation("Login failed for {Email}", dto.Email);
			else
				_logger.LogInformation("Login succeeded for {Email}", dto.Email);
			return StatusCode(result.StatusCode, result);
		}

		[HttpPost("refresh")]
		public async Task<IActionResult> Refresh([FromBody] RefreshTokenDTO dto)
		{
			var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
			return StatusCode(result.StatusCode, result);
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
		{
			_logger.LogInformation("Registration attempt for {Email}", dto.Email);
			var result = await _authService.RegisterAsync(dto);
			if (result.IsSuccess)
				_logger.LogInformation("Registration succeeded for {Email}", dto.Email);
			else
				_logger.LogInformation("Registration failed for {Email}", dto.Email);
			return StatusCode(result.StatusCode, result);
		}
	}
}
