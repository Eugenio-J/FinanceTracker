using FinanceTracker.Domain.Entities;
using FinanceTracker.Infrastructure.Repositories;
using FinanceTracker.Tests.Helpers;
using FluentAssertions;

namespace FinanceTracker.Tests.IntegrationTests.Repositories;

public class UserRepositoryTests
{
	private UserRepository CreateRepository(out Infrastructure.Data.DataContext context)
	{
		context = TestHelpers.CreateInMemoryContext();
		return new UserRepository(context);
	}

	[Fact]
	public async Task GetByEmailAsync_ReturnsUser_WhenExists()
	{
		var repo = CreateRepository(out var ctx);
		var user = new User
		{
			Id = Guid.NewGuid(),
			Email = "test@example.com",
			PasswordHash = "hash",
			FirstName = "Test",
			LastName = "User"
		};
		ctx.Users.Add(user);
		await ctx.SaveChangesAsync();

		var result = await repo.GetByEmailAsync("test@example.com");

		result.Should().NotBeNull();
		result!.Email.Should().Be("test@example.com");
	}

	[Fact]
	public async Task GetByEmailAsync_ReturnsNull_WhenNotFound()
	{
		var repo = CreateRepository(out _);

		var result = await repo.GetByEmailAsync("notfound@example.com");

		result.Should().BeNull();
	}

	[Fact]
	public async Task EmailExistsAsync_ReturnsTrue_WhenExists()
	{
		var repo = CreateRepository(out var ctx);
		ctx.Users.Add(new User
		{
			Id = Guid.NewGuid(),
			Email = "exists@example.com",
			PasswordHash = "hash",
			FirstName = "Test",
			LastName = "User"
		});
		await ctx.SaveChangesAsync();

		var result = await repo.EmailExistsAsync("exists@example.com");

		result.Should().BeTrue();
	}

	[Fact]
	public async Task EmailExistsAsync_ReturnsFalse_WhenNotExists()
	{
		var repo = CreateRepository(out _);

		var result = await repo.EmailExistsAsync("noone@example.com");

		result.Should().BeFalse();
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsUser_WhenExists()
	{
		var repo = CreateRepository(out var ctx);
		var user = new User
		{
			Id = Guid.NewGuid(),
			Email = "test@example.com",
			PasswordHash = "hash",
			FirstName = "Test",
			LastName = "User"
		};
		ctx.Users.Add(user);
		await ctx.SaveChangesAsync();

		var result = await repo.GetByIdAsync(user.Id);

		result.Should().NotBeNull();
		result!.Id.Should().Be(user.Id);
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
	{
		var repo = CreateRepository(out _);

		var result = await repo.GetByIdAsync(Guid.NewGuid());

		result.Should().BeNull();
	}

	[Fact]
	public async Task AddAsync_AddsUser()
	{
		var repo = CreateRepository(out var ctx);
		var user = new User
		{
			Id = Guid.NewGuid(),
			Email = "new@example.com",
			PasswordHash = "hash",
			FirstName = "New",
			LastName = "User"
		};

		await repo.AddAsync(user);
		await ctx.SaveChangesAsync();

		var result = await repo.GetByEmailAsync("new@example.com");
		result.Should().NotBeNull();
	}

	[Fact]
	public async Task Delete_RemovesUser()
	{
		var repo = CreateRepository(out var ctx);
		var user = new User
		{
			Id = Guid.NewGuid(),
			Email = "delete@example.com",
			PasswordHash = "hash",
			FirstName = "Del",
			LastName = "User"
		};
		ctx.Users.Add(user);
		await ctx.SaveChangesAsync();

		repo.Delete(user);
		await ctx.SaveChangesAsync();

		var result = await repo.GetByIdAsync(user.Id);
		result.Should().BeNull();
	}
}
