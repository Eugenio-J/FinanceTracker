using FinanceTracker.Application.DTOs.Auth;
using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Helpers;
using FinanceTracker.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BC = BCrypt.Net.BCrypt;

namespace FinanceTracker.Application.Services;

public class AuthService : IAuthService
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IConfiguration _configuration;
	private readonly ILogger<AuthService> _logger;

	public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<AuthService> logger)
	{
		_unitOfWork = unitOfWork;
		_configuration = configuration;
		_logger = logger;
	}

	public async Task<Result<AuthResponseDTO>> LoginAsync(LoginDTO dto)
	{
		var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);

		if (user == null || !BC.Verify(dto.Password, user.PasswordHash))
		{
			_logger.LogWarning("Login failed for {Email}", dto.Email);
			return Result<AuthResponseDTO>.Unauthorized("Invalid email or password");
		}

		var token = GenerateJwtToken(user);
		var refreshToken = await CreateRefreshTokenEntity(user.Id);
		await _unitOfWork.SaveChangesAsync();

		var response = new AuthResponseDTO(token, refreshToken.Token, user.Email, user.FirstName, user.LastName);

		_logger.LogInformation("Login succeeded for {Email}, user {UserId}", user.Email, user.Id);
		return Result<AuthResponseDTO>.Success(response);
	}

	public async Task<Result<AuthResponseDTO>> RegisterAsync(RegisterDTO dto)
	{
		// Check if email exists
		if (await _unitOfWork.Users.EmailExistsAsync(dto.Email))
		{
			_logger.LogWarning("Registration failed - email already registered: {Email}", dto.Email);
			return Result<AuthResponseDTO>.Failure("Email already registered", 400);
		}

		var user = new User
		{
			Email = dto.Email,
			PasswordHash = BC.HashPassword(dto.Password),
			FirstName = dto.FirstName,
			LastName = dto.LastName
		};

		await _unitOfWork.Users.AddAsync(user);
		await _unitOfWork.SaveChangesAsync();

		var token = GenerateJwtToken(user);
		var refreshToken = await CreateRefreshTokenEntity(user.Id);
		await _unitOfWork.SaveChangesAsync();

		var response = new AuthResponseDTO(token, refreshToken.Token, user.Email, user.FirstName, user.LastName);

		_logger.LogInformation("User registered successfully: {Email}, user {UserId}", user.Email, user.Id);
		return Result<AuthResponseDTO>.Created(response);
	}

	public async Task<Result<AuthResponseDTO>> RefreshTokenAsync(string refreshToken)
	{

		var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);

		if (storedToken == null)
		{
			_logger.LogWarning("Refresh token not found");
			return Result<AuthResponseDTO>.Unauthorized("Invalid refresh token");
		}

		// Reuse detection: if token is revoked, someone may have stolen it
		if (storedToken.IsRevoked)
		{
			_logger.LogWarning("Reuse detected for token family {Family}, revoking all tokens", storedToken.Family);
			await _unitOfWork.RefreshTokens.RevokeAllByFamilyAsync(storedToken.Family);
			await _unitOfWork.SaveChangesAsync();
			return Result<AuthResponseDTO>.Unauthorized("Invalid refresh token");
		}

		if (storedToken.IsExpired)
		{
			_logger.LogWarning("Refresh token expired for user {UserId}", storedToken.UserId);
			return Result<AuthResponseDTO>.Unauthorized("Refresh token expired");
		}

		// Rotate: revoke current, create new in same family
		storedToken.RevokedAt = PhilippineDateTime.Now;
		var newRefreshToken = await CreateRefreshTokenEntity(storedToken.UserId, storedToken.Family);
		storedToken.ReplacedByToken = newRefreshToken.Token;
		await _unitOfWork.SaveChangesAsync();

		var user = await _unitOfWork.Users.GetByIdAsync(storedToken.UserId);
		if (user == null)
		{
			_logger.LogWarning("User {UserId} not found during token refresh", storedToken.UserId);
			return Result<AuthResponseDTO>.Unauthorized("User not found");
		}

		var jwt = GenerateJwtToken(user);
		var response = new AuthResponseDTO(jwt, newRefreshToken.Token, user.Email, user.FirstName, user.LastName);

		_logger.LogInformation("Token refreshed for user {UserId}", user.Id);
		return Result<AuthResponseDTO>.Success(response);
	}

	public async Task<Result> LogoutAsync(Guid userId)
	{
		await _unitOfWork.RefreshTokens.RevokeAllByUserIdAsync(userId);
		await _unitOfWork.SaveChangesAsync();

		_logger.LogInformation("All refresh tokens revoked for user {UserId}", userId);
		return Result.Success();
	}

	private static string GenerateRefreshTokenString()
	{
		var bytes = new byte[64];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(bytes);
		return Convert.ToBase64String(bytes);
	}

	private string HashToken(string token)
	{
		using var sha = SHA256.Create();
		var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
		return Convert.ToBase64String(bytes);
	}

	private async Task<RefreshToken> CreateRefreshTokenEntity(Guid userId, string? family = null)
	{
		var expirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationInDays"] ?? "7");

		var refreshTokenString = GenerateRefreshTokenString();	

		var refreshToken = new RefreshToken
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Token = HashToken(refreshTokenString),
			Family = family ?? Guid.NewGuid().ToString(),
			ExpiresAt = PhilippineDateTime.Now.AddMinutes(2),
			//ExpiresAt = PhilippineDateTime.Now.AddMinutes(2),
			CreatedAt = PhilippineDateTime.Now
		};

		await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
		return refreshToken;
	}

	private string GenerateJwtToken(User user)
	{
		var key = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));

		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new Claim(ClaimTypes.Email, user.Email),
			new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
		};

	var token = new JwtSecurityToken(
			issuer: _configuration["Jwt:Issuer"],
			audience: _configuration["Jwt:Audience"],
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(2),
			//expires: DateTime.UtcNow.AddMinutes(
			//	int.Parse(_configuration["Jwt:ExpirationInMinutes"]!)),
			signingCredentials: credentials
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}
