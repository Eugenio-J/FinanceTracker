using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Infrastructure.Repositories;
using FinanceTracker.Tests.Helpers;
using FluentAssertions;

namespace FinanceTracker.Tests.IntegrationTests.Repositories;

public class TransactionRepositoryTests
{
	private readonly Guid _userId = Guid.NewGuid();

	private TransactionRepository CreateRepository(out Infrastructure.Data.DataContext context)
	{
		context = TestHelpers.CreateInMemoryContext();
		return new TransactionRepository(context);
	}

	private Account SeedAccountWithTransactions(Infrastructure.Data.DataContext ctx,
		int count = 5, Guid? userId = null)
	{
		var uid = userId ?? _userId;
		var user = new User
		{
			Id = uid,
			Email = $"{uid}@test.com",
			PasswordHash = "hash",
			FirstName = "Test",
			LastName = "User"
		};
		ctx.Users.Add(user);

		var account = TestHelpers.CreateTestAccount(uid);
		ctx.Accounts.Add(account);

		for (int i = 0; i < count; i++)
		{
			var tx = TestHelpers.CreateTestTransaction(account.Id,
				amount: (i + 1) * 100m,
				date: DateTime.UtcNow.AddDays(-i));
			ctx.Transactions.Add(tx);
		}

		ctx.SaveChanges();
		return account;
	}

	[Fact]
	public async Task GetByAccountIdAsync_ReturnsTransactionsForAccount()
	{
		var repo = CreateRepository(out var ctx);
		var account = SeedAccountWithTransactions(ctx, 3);

		var result = await repo.GetByAccountIdAsync(account.Id);

		result.Should().HaveCount(3);
		result.Should().OnlyContain(t => t.AccountId == account.Id);
	}

	[Fact]
	public async Task GetByAccountIdAsync_OrdersByDateDescending()
	{
		var repo = CreateRepository(out var ctx);
		var account = SeedAccountWithTransactions(ctx, 5);

		var result = (await repo.GetByAccountIdAsync(account.Id)).ToList();

		result.Should().BeInDescendingOrder(t => t.Date);
	}

	[Fact]
	public async Task GetByAccountIdAsync_LimitsCount()
	{
		var repo = CreateRepository(out var ctx);
		var account = SeedAccountWithTransactions(ctx, 10);

		var result = await repo.GetByAccountIdAsync(account.Id, count: 3);

		result.Should().HaveCount(3);
	}

	[Fact]
	public async Task GetByAccountIdAsync_IncludesAccount()
	{
		var repo = CreateRepository(out var ctx);
		var account = SeedAccountWithTransactions(ctx, 1);

		var result = (await repo.GetByAccountIdAsync(account.Id)).First();

		result.Account.Should().NotBeNull();
		result.Account.Id.Should().Be(account.Id);
	}

	[Fact]
	public async Task GetFilteredAsync_FiltersByUserId()
	{
		var repo = CreateRepository(out var ctx);
		var account = SeedAccountWithTransactions(ctx, 3);
		var otherUserId = Guid.NewGuid();
		SeedAccountWithTransactions(ctx, 2, otherUserId);

		var result = await repo.GetFilteredAsync(_userId);

		result.Should().HaveCount(3);
	}

	[Fact]
	public async Task GetFilteredAsync_FiltersByAccountId()
	{
		var repo = CreateRepository(out var ctx);
		var account = SeedAccountWithTransactions(ctx, 3);

		// Add another account for same user
		var account2 = TestHelpers.CreateTestAccount(_userId);
		ctx.Accounts.Add(account2);
		ctx.Transactions.Add(TestHelpers.CreateTestTransaction(account2.Id));
		await ctx.SaveChangesAsync();

		var result = await repo.GetFilteredAsync(_userId, accountId: account.Id);

		result.Should().HaveCount(3);
	}

	[Fact]
	public async Task GetFilteredAsync_FiltersByTransactionType()
	{
		var repo = CreateRepository(out var ctx);
		var user = new User { Id = _userId, Email = "t@t.com", PasswordHash = "h", FirstName = "T", LastName = "U" };
		ctx.Users.Add(user);
		var account = TestHelpers.CreateTestAccount(_userId);
		ctx.Accounts.Add(account);
		ctx.Transactions.AddRange(
			TestHelpers.CreateTestTransaction(account.Id, type: TransactionType.Deposit),
			TestHelpers.CreateTestTransaction(account.Id, type: TransactionType.Deposit),
			TestHelpers.CreateTestTransaction(account.Id, type: TransactionType.Withdrawal)
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetFilteredAsync(_userId, transactionType: TransactionType.Deposit);

		result.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetFilteredAsync_FiltersByCategory()
	{
		var repo = CreateRepository(out var ctx);
		var user = new User { Id = _userId, Email = "t@t.com", PasswordHash = "h", FirstName = "T", LastName = "U" };
		ctx.Users.Add(user);
		var account = TestHelpers.CreateTestAccount(_userId);
		ctx.Accounts.Add(account);
		ctx.Transactions.AddRange(
			TestHelpers.CreateTestTransaction(account.Id, category: TransactionCategory.Salary),
			TestHelpers.CreateTestTransaction(account.Id, category: TransactionCategory.Expense),
			TestHelpers.CreateTestTransaction(account.Id, category: TransactionCategory.Expense)
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetFilteredAsync(_userId, category: TransactionCategory.Expense);

		result.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetFilteredAsync_FiltersByDateRange()
	{
		var repo = CreateRepository(out var ctx);
		var user = new User { Id = _userId, Email = "t@t.com", PasswordHash = "h", FirstName = "T", LastName = "U" };
		ctx.Users.Add(user);
		var account = TestHelpers.CreateTestAccount(_userId);
		ctx.Accounts.Add(account);
		ctx.Transactions.AddRange(
			TestHelpers.CreateTestTransaction(account.Id, date: new DateTime(2025, 1, 15)),
			TestHelpers.CreateTestTransaction(account.Id, date: new DateTime(2025, 2, 15)),
			TestHelpers.CreateTestTransaction(account.Id, date: new DateTime(2025, 3, 15))
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetFilteredAsync(_userId,
			startDate: new DateTime(2025, 2, 1),
			endDate: new DateTime(2025, 2, 28));

		result.Should().HaveCount(1);
	}

	[Fact]
	public async Task GetFilteredAsync_AppliesPaging()
	{
		var repo = CreateRepository(out var ctx);
		var account = SeedAccountWithTransactions(ctx, 10);

		var page1 = await repo.GetFilteredAsync(_userId, pageNumber: 1, pageSize: 3);
		var page2 = await repo.GetFilteredAsync(_userId, pageNumber: 2, pageSize: 3);

		page1.Should().HaveCount(3);
		page2.Should().HaveCount(3);
		page1.Should().NotIntersectWith(page2);
	}

	[Fact]
	public async Task GetCountAsync_ReturnsCorrectCount()
	{
		var repo = CreateRepository(out var ctx);
		SeedAccountWithTransactions(ctx, 5);

		var result = await repo.GetCountAsync(_userId);

		result.Should().Be(5);
	}

	[Fact]
	public async Task GetCountAsync_AppliesFilters()
	{
		var repo = CreateRepository(out var ctx);
		var user = new User { Id = _userId, Email = "t@t.com", PasswordHash = "h", FirstName = "T", LastName = "U" };
		ctx.Users.Add(user);
		var account = TestHelpers.CreateTestAccount(_userId);
		ctx.Accounts.Add(account);
		ctx.Transactions.AddRange(
			TestHelpers.CreateTestTransaction(account.Id, type: TransactionType.Deposit),
			TestHelpers.CreateTestTransaction(account.Id, type: TransactionType.Deposit),
			TestHelpers.CreateTestTransaction(account.Id, type: TransactionType.Withdrawal)
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetCountAsync(_userId, transactionType: TransactionType.Deposit);

		result.Should().Be(2);
	}

	[Fact]
	public async Task GetCountAsync_ReturnsZero_WhenNoMatches()
	{
		var repo = CreateRepository(out _);

		var result = await repo.GetCountAsync(Guid.NewGuid());

		result.Should().Be(0);
	}
}
