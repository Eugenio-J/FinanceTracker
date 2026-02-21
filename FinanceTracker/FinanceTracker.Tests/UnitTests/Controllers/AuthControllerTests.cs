using FinanceTracker.API.Controllers;
using FinanceTracker.Application.DTOs.Auth;
using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;

namespace FinanceTracker.Tests.UnitTests.Controllers;

public class AuthControllerTests
{
	private readonly Mock<IAuthService> _authService;
	private readonly AuthController _sut;

	public AuthControllerTests()
	{
		_authService = new Mock<IAuthService>();
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["Jwt:RefreshTokenExpirationInDays"] = "7"
			})
			.Build();
		var environment = new Mock<IWebHostEnvironment>();
		environment.Setup(e => e.EnvironmentName).Returns("Development");
		_sut = new AuthController(_authService.Object, NullLogger<AuthController>.Instance,
			configuration, environment.Object);
		_sut.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext()
		};
	}

	[Fact]
	public async Task Login_ReturnsOkWithToken_WhenSuccess()
	{
		var response = new AuthResponseDTO("jwt-token", "refresh-token", "test@test.com", "Test", "User");
		_authService.Setup(s => s.LoginAsync(It.IsAny<LoginDTO>()))
			.ReturnsAsync(Result<AuthResponseDTO>.Success(response));

		var result = await _sut.Login(new LoginDTO("test@test.com", "password123"));

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(200);
	}

	[Fact]
	public async Task Login_SetsHttpOnlyCookie_WhenSuccess()
	{
		var response = new AuthResponseDTO("jwt-token", "refresh-token", "test@test.com", "Test", "User");
		_authService.Setup(s => s.LoginAsync(It.IsAny<LoginDTO>()))
			.ReturnsAsync(Result<AuthResponseDTO>.Success(response));

		await _sut.Login(new LoginDTO("test@test.com", "password123"));

		var setCookie = _sut.HttpContext.Response.Headers["Set-Cookie"].ToString();
		setCookie.Should().Contain("refreshToken=refresh-token");
		setCookie.Should().Contain("httponly");
		setCookie.Should().Contain("path=/api/auth");
	}

	[Fact]
	public async Task Login_ReturnsEmptyRefreshTokenInBody_WhenSuccess()
	{
		var response = new AuthResponseDTO("jwt-token", "refresh-token", "test@test.com", "Test", "User");
		_authService.Setup(s => s.LoginAsync(It.IsAny<LoginDTO>()))
			.ReturnsAsync(Result<AuthResponseDTO>.Success(response));

		var result = await _sut.Login(new LoginDTO("test@test.com", "password123"));

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		var resultData = objectResult.Value.Should().BeOfType<Result<AuthResponseDTO>>().Subject;
		resultData.Data!.RefreshToken.Should().BeEmpty();
	}

	[Fact]
	public async Task Login_ReturnsUnauthorized_WhenFailed()
	{
		_authService.Setup(s => s.LoginAsync(It.IsAny<LoginDTO>()))
			.ReturnsAsync(Result<AuthResponseDTO>.Unauthorized("Invalid email or password"));

		var result = await _sut.Login(new LoginDTO("bad@test.com", "wrong"));

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(401);
	}

	[Fact]
	public async Task Register_ReturnsCreated_WhenSuccess()
	{
		var response = new AuthResponseDTO("jwt-token", "refresh-token", "new@test.com", "New", "User");
		_authService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDTO>()))
			.ReturnsAsync(Result<AuthResponseDTO>.Created(response));

		var result = await _sut.Register(new RegisterDTO("new@test.com", "password123", "New", "User"));

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(201);
	}

	[Fact]
	public async Task Register_SetsHttpOnlyCookie_WhenSuccess()
	{
		var response = new AuthResponseDTO("jwt-token", "refresh-token", "new@test.com", "New", "User");
		_authService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDTO>()))
			.ReturnsAsync(Result<AuthResponseDTO>.Created(response));

		await _sut.Register(new RegisterDTO("new@test.com", "password123", "New", "User"));

		var setCookie = _sut.HttpContext.Response.Headers["Set-Cookie"].ToString();
		setCookie.Should().Contain("refreshToken=refresh-token");
		setCookie.Should().Contain("httponly");
		setCookie.Should().Contain("path=/api/auth");
	}

	[Fact]
	public async Task Register_ReturnsEmptyRefreshTokenInBody_WhenSuccess()
	{
		var response = new AuthResponseDTO("jwt-token", "refresh-token", "new@test.com", "New", "User");
		_authService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDTO>()))
			.ReturnsAsync(Result<AuthResponseDTO>.Created(response));

		var result = await _sut.Register(new RegisterDTO("new@test.com", "password123", "New", "User"));

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		var resultData = objectResult.Value.Should().BeOfType<Result<AuthResponseDTO>>().Subject;
		resultData.Data!.RefreshToken.Should().BeEmpty();
	}

	[Fact]
	public async Task Register_ReturnsBadRequest_WhenDuplicateEmail()
	{
		_authService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDTO>()))
			.ReturnsAsync(Result<AuthResponseDTO>.Failure("Email already registered", 400));

		var result = await _sut.Register(new RegisterDTO("existing@test.com", "password123", "Test", "User"));

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(400);
	}

	[Fact]
	public async Task Refresh_ReturnsOkWithNewToken_WhenCookieValid()
	{
		var response = new AuthResponseDTO("new-jwt", "new-refresh", "test@test.com", "Test", "User");
		_authService.Setup(s => s.RefreshTokenAsync("old-refresh-token"))
			.ReturnsAsync(Result<AuthResponseDTO>.Success(response));

		_sut.HttpContext.Request.Headers["Cookie"] = "refreshToken=old-refresh-token";

		var result = await _sut.Refresh();

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(200);
		var resultData = objectResult.Value.Should().BeOfType<Result<AuthResponseDTO>>().Subject;
		resultData.Data!.Token.Should().Be("new-jwt");
		resultData.Data.RefreshToken.Should().BeEmpty();
	}

	[Fact]
	public async Task Refresh_SetsNewCookie_WhenSuccess()
	{
		var response = new AuthResponseDTO("new-jwt", "new-refresh", "test@test.com", "Test", "User");
		_authService.Setup(s => s.RefreshTokenAsync("old-refresh-token"))
			.ReturnsAsync(Result<AuthResponseDTO>.Success(response));

		_sut.HttpContext.Request.Headers["Cookie"] = "refreshToken=old-refresh-token";

		await _sut.Refresh();

		var setCookie = _sut.HttpContext.Response.Headers["Set-Cookie"].ToString();
		setCookie.Should().Contain("refreshToken=new-refresh");
	}

	[Fact]
	public async Task Refresh_ReturnsUnauthorized_WhenNoCookie()
	{
		var result = await _sut.Refresh();

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(401);
	}

	[Fact]
	public async Task Refresh_ReturnsUnauthorized_WhenFailed()
	{
		_authService.Setup(s => s.RefreshTokenAsync("bad-token"))
			.ReturnsAsync(Result<AuthResponseDTO>.Unauthorized("Invalid refresh token"));

		_sut.HttpContext.Request.Headers["Cookie"] = "refreshToken=bad-token";

		var result = await _sut.Refresh();

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(401);
	}

	[Fact]
	public async Task Refresh_ReturnsUnauthorized_AndClearsCookie_WhenRefreshTokenExpired()
	{
		// When both access token and refresh token are expired,
		// the client sends the expired refresh token via cookie and should get 401 + cleared cookie
		_authService.Setup(s => s.RefreshTokenAsync("expired-refresh-token"))
			.ReturnsAsync(Result<AuthResponseDTO>.Unauthorized("Refresh token expired"));

		_sut.HttpContext.Request.Headers["Cookie"] = "refreshToken=expired-refresh-token";

		var result = await _sut.Refresh();

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(401);
		var resultData = objectResult.Value.Should().BeOfType<Result<AuthResponseDTO>>().Subject;
		resultData.Error.Should().Be("Refresh token expired");

		// Controller should clear the cookie on refresh failure
		var setCookie = _sut.HttpContext.Response.Headers["Set-Cookie"].ToString();
		setCookie.Should().Contain("refreshToken=");
		setCookie.Should().Contain("expires=");
	}

	[Fact]
	public async Task Logout_ReturnsOk_AndClearsCookie()
	{
		var userId = Guid.NewGuid();
		_authService.Setup(s => s.LogoutAsync(userId))
			.ReturnsAsync(Result.Success());

		_sut.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext
			{
				User = new ClaimsPrincipal(new ClaimsIdentity(new[]
				{
					new Claim(ClaimTypes.NameIdentifier, userId.ToString())
				}, "TestAuth"))
			}
		};

		var result = await _sut.Logout();

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(200);

		var setCookie = _sut.HttpContext.Response.Headers["Set-Cookie"].ToString();
		setCookie.Should().Contain("refreshToken=");
		setCookie.Should().Contain("expires=");
	}
}
