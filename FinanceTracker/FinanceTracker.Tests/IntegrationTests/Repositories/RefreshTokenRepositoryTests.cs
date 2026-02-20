using FinanceTracker.Infrastructure.Repositories;
using FinanceTracker.Tests.Helpers;
using FluentAssertions;

namespace FinanceTracker.Tests.IntegrationTests.Repositories;

public class RefreshTokenRepositoryTests
{
	[Fact]
	public async Task GetByTokenAsync_ReturnsToken_WhenExists()
	{
		using var context = TestHelpers.CreateInMemoryContext();
		var user = TestHelpers.CreateTestUser();
		context.Users.Add(user);
		var refreshToken = TestHelpers.CreateTestRefreshToken(user.Id);
		context.RefreshTokens.Add(refreshToken);
		await context.SaveChangesAsync();

		var repo = new RefreshTokenRepository(context);
		var result = await repo.GetByTokenAsync(refreshToken.Token);

		result.Should().NotBeNull();
		result!.Id.Should().Be(refreshToken.Id);
		result.UserId.Should().Be(user.Id);
	}

	[Fact]
	public async Task GetByTokenAsync_ReturnsNull_WhenNotExists()
	{
		using var context = TestHelpers.CreateInMemoryContext();
		var repo = new RefreshTokenRepository(context);

		var result = await repo.GetByTokenAsync("nonexistent-token");

		result.Should().BeNull();
	}

	[Fact]
	public async Task GetActiveByFamilyAsync_ReturnsOnlyActiveTokens()
	{
		using var context = TestHelpers.CreateInMemoryContext();
		var user = TestHelpers.CreateTestUser();
		context.Users.Add(user);

		var family = Guid.NewGuid().ToString();
		var activeToken = TestHelpers.CreateTestRefreshToken(user.Id, family: family);
		var revokedToken = TestHelpers.CreateTestRefreshToken(user.Id, family: family, isRevoked: true);
		var expiredToken = TestHelpers.CreateTestRefreshToken(user.Id, family: family, isExpired: true);

		context.RefreshTokens.AddRange(activeToken, revokedToken, expiredToken);
		await context.SaveChangesAsync();

		var repo = new RefreshTokenRepository(context);
		var result = (await repo.GetActiveByFamilyAsync(family)).ToList();

		result.Should().HaveCount(1);
		result[0].Id.Should().Be(activeToken.Id);
	}

	[Fact]
	public async Task RevokeAllByFamilyAsync_RevokesAllTokensInFamily()
	{
		using var context = TestHelpers.CreateInMemoryContext();
		var user = TestHelpers.CreateTestUser();
		context.Users.Add(user);

		var family = Guid.NewGuid().ToString();
		var token1 = TestHelpers.CreateTestRefreshToken(user.Id, family: family);
		var token2 = TestHelpers.CreateTestRefreshToken(user.Id, family: family);
		var otherFamilyToken = TestHelpers.CreateTestRefreshToken(user.Id);

		context.RefreshTokens.AddRange(token1, token2, otherFamilyToken);
		await context.SaveChangesAsync();

		var repo = new RefreshTokenRepository(context);
		await repo.RevokeAllByFamilyAsync(family);
		await context.SaveChangesAsync();

		token1.RevokedAt.Should().NotBeNull();
		token2.RevokedAt.Should().NotBeNull();
		otherFamilyToken.RevokedAt.Should().BeNull();
	}

	[Fact]
	public async Task RevokeAllByUserIdAsync_RevokesAllUserTokens()
	{
		using var context = TestHelpers.CreateInMemoryContext();
		var user = TestHelpers.CreateTestUser();
		var otherUser = TestHelpers.CreateTestUser(Guid.NewGuid());
		context.Users.AddRange(user, otherUser);

		var userToken1 = TestHelpers.CreateTestRefreshToken(user.Id);
		var userToken2 = TestHelpers.CreateTestRefreshToken(user.Id);
		var otherUserToken = TestHelpers.CreateTestRefreshToken(otherUser.Id);

		context.RefreshTokens.AddRange(userToken1, userToken2, otherUserToken);
		await context.SaveChangesAsync();

		var repo = new RefreshTokenRepository(context);
		await repo.RevokeAllByUserIdAsync(user.Id);
		await context.SaveChangesAsync();

		userToken1.RevokedAt.Should().NotBeNull();
		userToken2.RevokedAt.Should().NotBeNull();
		otherUserToken.RevokedAt.Should().BeNull();
	}
}
