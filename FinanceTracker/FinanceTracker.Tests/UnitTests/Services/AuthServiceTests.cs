using FinanceTracker.Application.DTOs.Auth;
using FinanceTracker.Application.Services;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BC = BCrypt.Net.BCrypt;

namespace FinanceTracker.Tests.UnitTests.Services;

public class AuthServiceTests
{
	private readonly Mock<IUnitOfWork> _unitOfWork;
	private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo;
	private readonly IConfiguration _configuration;
	private readonly AuthService _sut;

	public AuthServiceTests()
	{
		_unitOfWork = new Mock<IUnitOfWork>();
		_refreshTokenRepo = new Mock<IRefreshTokenRepository>();
		_unitOfWork.Setup(u => u.RefreshTokens).Returns(_refreshTokenRepo.Object);
		_refreshTokenRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
			.ReturnsAsync((RefreshToken rt) => rt);

		_configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Jwt:Secret"] = "ThisIsASecretKeyForTestingPurposesOnly1234567890",
				["Jwt:Issuer"] = "TestIssuer",
				["Jwt:Audience"] = "TestAudience",
				["Jwt:ExpirationInMinutes"] = "60",
				["Jwt:RefreshTokenExpirationInDays"] = "7"
			})
			.Build();
		_sut = new AuthService(_unitOfWork.Object, _configuration, NullLogger<AuthService>.Instance);
	}

	private User CreateUser(string email = "test@example.com", string password = "password123")
	{
		return new User
		{
			Id = Guid.NewGuid(),
			Email = email,
			PasswordHash = BC.HashPassword(password),
			FirstName = "Test",
			LastName = "User",
			CreatedAt = DateTime.UtcNow
		};
	}

	[Fact]
	public async Task LoginAsync_ReturnsSuccess_WhenCredentialsValid()
	{
		var user = CreateUser();
		_unitOfWork.Setup(u => u.Users.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

		var result = await _sut.LoginAsync(new LoginDTO("test@example.com", "password123"));

		result.IsSuccess.Should().BeTrue();
		result.StatusCode.Should().Be(200);
		result.Data.Should().NotBeNull();
		result.Data!.Email.Should().Be("test@example.com");
		result.Data.Token.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task LoginAsync_ReturnsUnauthorized_WhenEmailNotFound()
	{
		_unitOfWork.Setup(u => u.Users.GetByEmailAsync("notfound@example.com")).ReturnsAsync((User?)null);

		var result = await _sut.LoginAsync(new LoginDTO("notfound@example.com", "password123"));

		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(401);
	}

	[Fact]
	public async Task LoginAsync_ReturnsUnauthorized_WhenPasswordWrong()
	{
		var user = CreateUser();
		_unitOfWork.Setup(u => u.Users.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

		var result = await _sut.LoginAsync(new LoginDTO("test@example.com", "wrongpassword"));

		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(401);
	}

	[Fact]
	public async Task RegisterAsync_ReturnsCreated_WhenEmailNew()
	{
		_unitOfWork.Setup(u => u.Users.EmailExistsAsync("new@example.com")).ReturnsAsync(false);
		_unitOfWork.Setup(u => u.Users.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

		var result = await _sut.RegisterAsync(new RegisterDTO("new@example.com", "password123", "New", "User"));

		result.IsSuccess.Should().BeTrue();
		result.StatusCode.Should().Be(201);
		result.Data.Should().NotBeNull();
		result.Data!.Email.Should().Be("new@example.com");
		_unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
	}

	[Fact]
	public async Task RegisterAsync_ReturnsFailure_WhenEmailDuplicate()
	{
		_unitOfWork.Setup(u => u.Users.EmailExistsAsync("existing@example.com")).ReturnsAsync(true);

		var result = await _sut.RegisterAsync(new RegisterDTO("existing@example.com", "password123", "Test", "User"));

		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(400);
	}

	[Fact]
	public async Task LoginAsync_ReturnsTokenWithCorrectClaims()
	{
		var user = CreateUser();
		_unitOfWork.Setup(u => u.Users.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

		var result = await _sut.LoginAsync(new LoginDTO("test@example.com", "password123"));

		var handler = new JwtSecurityTokenHandler();
		var jwt = handler.ReadJwtToken(result.Data!.Token);

		jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value.Should().Be(user.Id.ToString());
		jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value.Should().Be(user.Email);
		jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value.Should().Be($"{user.FirstName} {user.LastName}");
	}

	[Fact]
	public async Task RegisterAsync_HashesPassword()
	{
		_unitOfWork.Setup(u => u.Users.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
		User? capturedUser = null;
		_unitOfWork.Setup(u => u.Users.AddAsync(It.IsAny<User>()))
			.Callback<User>(u => capturedUser = u)
			.ReturnsAsync((User u) => u);

		await _sut.RegisterAsync(new RegisterDTO("hash@example.com", "password123", "Test", "User"));

		capturedUser.Should().NotBeNull();
		capturedUser!.PasswordHash.Should().NotBe("password123");
		BC.Verify("password123", capturedUser.PasswordHash).Should().BeTrue();
	}

	[Fact]
	public async Task LoginAsync_ReturnsRefreshToken_WhenCredentialsValid()
	{
		var user = CreateUser();
		_unitOfWork.Setup(u => u.Users.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

		var result = await _sut.LoginAsync(new LoginDTO("test@example.com", "password123"));

		result.IsSuccess.Should().BeTrue();
		result.Data!.RefreshToken.Should().NotBeNullOrEmpty();
		_refreshTokenRepo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
		_unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task RegisterAsync_ReturnsRefreshToken_WhenEmailNew()
	{
		_unitOfWork.Setup(u => u.Users.EmailExistsAsync("new@example.com")).ReturnsAsync(false);
		_unitOfWork.Setup(u => u.Users.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

		var result = await _sut.RegisterAsync(new RegisterDTO("new@example.com", "password123", "New", "User"));

		result.IsSuccess.Should().BeTrue();
		result.Data!.RefreshToken.Should().NotBeNullOrEmpty();
		_refreshTokenRepo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
	}

	[Fact]
	public async Task RefreshTokenAsync_ReturnsSuccess_WhenTokenIsValid()
	{
		var user = CreateUser();
		var token = TestHelpers.CreateTestRefreshToken(user.Id);
		_refreshTokenRepo.Setup(r => r.GetByTokenAsync(token.Token)).ReturnsAsync(token);
		_unitOfWork.Setup(u => u.Users.GetByIdAsync(user.Id)).ReturnsAsync(user);

		var result = await _sut.RefreshTokenAsync(token.Token);

		result.IsSuccess.Should().BeTrue();
		result.Data.Should().NotBeNull();
		result.Data!.Token.Should().NotBeNullOrEmpty();
		result.Data.RefreshToken.Should().NotBeNullOrEmpty();
		result.Data.Email.Should().Be(user.Email);
	}

	[Fact]
	public async Task RefreshTokenAsync_RotatesToken_WhenTokenIsValid()
	{
		var user = CreateUser();
		var family = Guid.NewGuid().ToString();
		var token = TestHelpers.CreateTestRefreshToken(user.Id, family: family);
		_refreshTokenRepo.Setup(r => r.GetByTokenAsync(token.Token)).ReturnsAsync(token);
		_unitOfWork.Setup(u => u.Users.GetByIdAsync(user.Id)).ReturnsAsync(user);

		RefreshToken? capturedNewToken = null;
		_refreshTokenRepo.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
			.Callback<RefreshToken>(rt => capturedNewToken = rt)
			.ReturnsAsync((RefreshToken rt) => rt);

		var result = await _sut.RefreshTokenAsync(token.Token);

		result.IsSuccess.Should().BeTrue();
		token.RevokedAt.Should().NotBeNull();
		token.ReplacedByToken.Should().NotBeNullOrEmpty();
		capturedNewToken.Should().NotBeNull();
		capturedNewToken!.Family.Should().Be(family);
		_unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task RefreshTokenAsync_ReturnsUnauthorized_WhenTokenNotFound()
	{
		_refreshTokenRepo.Setup(r => r.GetByTokenAsync("nonexistent")).ReturnsAsync((RefreshToken?)null);

		var result = await _sut.RefreshTokenAsync("nonexistent");

		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(401);
	}

	[Fact]
	public async Task RefreshTokenAsync_ReturnsUnauthorized_WhenTokenExpired()
	{
		var token = TestHelpers.CreateTestRefreshToken(Guid.NewGuid(), isExpired: true);
		_refreshTokenRepo.Setup(r => r.GetByTokenAsync(token.Token)).ReturnsAsync(token);

		var result = await _sut.RefreshTokenAsync(token.Token);

		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(401);
	}

	[Fact]
	public async Task RefreshTokenAsync_RevokesEntireFamily_WhenRevokedTokenReused()
	{
		var family = Guid.NewGuid().ToString();
		var token = TestHelpers.CreateTestRefreshToken(Guid.NewGuid(), family: family, isRevoked: true);
		_refreshTokenRepo.Setup(r => r.GetByTokenAsync(token.Token)).ReturnsAsync(token);

		var result = await _sut.RefreshTokenAsync(token.Token);

		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(401);
		_refreshTokenRepo.Verify(r => r.RevokeAllByFamilyAsync(family), Times.Once);
		_unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task RefreshTokenAsync_ReturnsUnauthorized_WhenUserNotFound()
	{
		var userId = Guid.NewGuid();
		var token = TestHelpers.CreateTestRefreshToken(userId);
		_refreshTokenRepo.Setup(r => r.GetByTokenAsync(token.Token)).ReturnsAsync(token);
		_unitOfWork.Setup(u => u.Users.GetByIdAsync(userId)).ReturnsAsync((User?)null);

		var result = await _sut.RefreshTokenAsync(token.Token);

		result.IsSuccess.Should().BeFalse();
		result.StatusCode.Should().Be(401);
	}
}
