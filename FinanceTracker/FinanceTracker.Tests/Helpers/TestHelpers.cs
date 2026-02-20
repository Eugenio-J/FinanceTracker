using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceTracker.Tests.Helpers;

public static class TestHelpers
{
	public static DataContext CreateInMemoryContext(string? dbName = null)
	{
		var options = new DbContextOptionsBuilder<DataContext>()
			.UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
			.Options;

		return new DataContext(options);
	}

	public static Guid TestUserId { get; } = Guid.NewGuid();
	public static Guid OtherUserId { get; } = Guid.NewGuid();

	public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId)
	{
		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
			new Claim(ClaimTypes.Email, "test@test.com")
		};
		return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
	}

	public static void SetupControllerContext(ControllerBase controller, Guid userId)
	{
		controller.ControllerContext = new ControllerContext
		{
			HttpContext = new DefaultHttpContext
			{
				User = CreateClaimsPrincipal(userId)
			}
		};
	}

	public static User CreateTestUser(Guid? id = null)
	{
		return new User
		{
			Id = id ?? TestUserId,
			Email = "test@example.com",
			PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
			FirstName = "Test",
			LastName = "User",
			CreatedAt = DateTime.UtcNow
		};
	}

	public static Account CreateTestAccount(Guid? userId = null, AccountType type = AccountType.Hub, decimal balance = 1000m, Guid? id = null)
	{
		return new Account
		{
			Id = id ?? Guid.NewGuid(),
			UserId = userId ?? TestUserId,
			Name = $"Test {type}",
			AccountType = type,
			CurrentBalance = balance,
			CreatedAt = DateTime.UtcNow
		};
	}

	public static Transactions CreateTestTransaction(Guid accountId, decimal amount = 100m,
		TransactionType type = TransactionType.Deposit,
		TransactionCategory category = TransactionCategory.Salary,
		DateTime? date = null)
	{
		return new Transactions
		{
			Id = Guid.NewGuid(),
			AccountId = accountId,
			Amount = amount,
			TransactionType = type,
			Category = category,
			Description = "Test transaction",
			Date = date ?? DateTime.UtcNow,
			CreatedAt = DateTime.UtcNow
		};
	}

	public static Expense CreateTestExpense(Guid userId, Guid accountId, decimal amount = 50m,
		ExpenseCategory category = ExpenseCategory.Food, DateTime? date = null)
	{
		return new Expense
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			AccountId = accountId,
			Amount = amount,
			Category = category,
			Description = "Test expense",
			Date = date ?? DateTime.UtcNow,
			CreatedAt = DateTime.UtcNow
		};
	}

	public static RefreshToken CreateTestRefreshToken(Guid userId, string? family = null,
		bool isRevoked = false, bool isExpired = false)
	{
		return new RefreshToken
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
			Family = family ?? Guid.NewGuid().ToString(),
			ExpiresAt = isExpired ? DateTime.UtcNow.AddDays(-1) : DateTime.UtcNow.AddDays(7),
			CreatedAt = DateTime.UtcNow,
			RevokedAt = isRevoked ? DateTime.UtcNow : null
		};
	}

	public static SalaryCycle CreateTestSalaryCycle(Guid userId, DateTime? payDate = null,
		SalaryCycleStatus status = SalaryCycleStatus.Pending)
	{
		return new SalaryCycle
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			PayDate = payDate ?? DateTime.UtcNow.AddDays(14),
			GrossSalary = 5000m,
			NetSalary = 4000m,
			Status = status,
			CreatedAt = DateTime.UtcNow
		};
	}
}
