using FinanceTracker.API.Controllers;
using FinanceTracker.Application.DTOs.Auth;
using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinanceTracker.Tests.UnitTests.Controllers;

public class AuthControllerTests
{
	private readonly Mock<IAuthService> _authService;
	private readonly AuthController _sut;

	public AuthControllerTests()
	{
		_authService = new Mock<IAuthService>();
		_sut = new AuthController(_authService.Object, NullLogger<AuthController>.Instance);
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
	public async Task Register_ReturnsBadRequest_WhenDuplicateEmail()
	{
		_authService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDTO>()))
			.ReturnsAsync(Result<AuthResponseDTO>.Failure("Email already registered", 400));

		var result = await _sut.Register(new RegisterDTO("existing@test.com", "password123", "Test", "User"));

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(400);
	}

	[Fact]
	public async Task Refresh_ReturnsOkWithNewTokens_WhenSuccess()
	{
		var response = new AuthResponseDTO("new-jwt", "new-refresh", "test@test.com", "Test", "User");
		_authService.Setup(s => s.RefreshTokenAsync("old-refresh-token"))
			.ReturnsAsync(Result<AuthResponseDTO>.Success(response));

		var result = await _sut.Refresh(new RefreshTokenDTO("old-refresh-token"));

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(200);
		var resultData = objectResult.Value.Should().BeOfType<Result<AuthResponseDTO>>().Subject;
		resultData.Data!.Token.Should().Be("new-jwt");
		resultData.Data.RefreshToken.Should().Be("new-refresh");
	}

	[Fact]
	public async Task Refresh_ReturnsUnauthorized_WhenFailed()
	{
		_authService.Setup(s => s.RefreshTokenAsync("bad-token"))
			.ReturnsAsync(Result<AuthResponseDTO>.Unauthorized("Invalid refresh token"));

		var result = await _sut.Refresh(new RefreshTokenDTO("bad-token"));

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(401);
	}
}
