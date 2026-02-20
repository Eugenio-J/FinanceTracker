using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Infrastructure.Repositories;
using FinanceTracker.Tests.Helpers;
using FluentAssertions;

namespace FinanceTracker.Tests.IntegrationTests.Repositories;

public class AccountRepositoryTests
{
	private readonly Guid _userId = Guid.NewGuid();

	private AccountRepository CreateRepository(out Infrastructure.Data.DataContext context)
	{
		context = TestHelpers.CreateInMemoryContext();
		return new AccountRepository(context);
	}

	[Fact]
	public async Task GetByUserIdAsync_ReturnsAccountsForUser()
	{
		var repo = CreateRepository(out var ctx);
		var account1 = TestHelpers.CreateTestAccount(_userId, AccountType.Hub);
		var account2 = TestHelpers.CreateTestAccount(_userId, AccountType.Savings);
		var otherAccount = TestHelpers.CreateTestAccount(Guid.NewGuid(), AccountType.Hub);

		ctx.Accounts.AddRange(account1, account2, otherAccount);
		await ctx.SaveChangesAsync();

		var result = await repo.GetByUserIdAsync(_userId);

		result.Should().HaveCount(2);
		result.Should().OnlyContain(a => a.UserId == _userId);
	}

	[Fact]
	public async Task GetByUserIdAsync_ReturnsEmpty_WhenNoAccounts()
	{
		var repo = CreateRepository(out _);

		var result = await repo.GetByUserIdAsync(Guid.NewGuid());

		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GetByUserIdAsync_OrdersByAccountType()
	{
		var repo = CreateRepository(out var ctx);
		ctx.Accounts.AddRange(
			TestHelpers.CreateTestAccount(_userId, AccountType.Savings),
			TestHelpers.CreateTestAccount(_userId, AccountType.Payroll),
			TestHelpers.CreateTestAccount(_userId, AccountType.Hub)
		);
		await ctx.SaveChangesAsync();

		var result = (await repo.GetByUserIdAsync(_userId)).ToList();

		result[0].AccountType.Should().Be(AccountType.Payroll);
		result[1].AccountType.Should().Be(AccountType.Hub);
		result[2].AccountType.Should().Be(AccountType.Savings);
	}

	[Fact]
	public async Task GetByIdWithTransactionsAsync_IncludesTransactions()
	{
		var repo = CreateRepository(out var ctx);
		var account = TestHelpers.CreateTestAccount(_userId);
		ctx.Accounts.Add(account);
		ctx.Transactions.AddRange(
			TestHelpers.CreateTestTransaction(account.Id, date: DateTime.UtcNow),
			TestHelpers.CreateTestTransaction(account.Id, date: DateTime.UtcNow.AddDays(-1))
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetByIdWithTransactionsAsync(account.Id);

		result.Should().NotBeNull();
		result!.Transactions.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetByIdWithTransactionsAsync_ReturnsNull_WhenNotFound()
	{
		var repo = CreateRepository(out _);

		var result = await repo.GetByIdWithTransactionsAsync(Guid.NewGuid());

		result.Should().BeNull();
	}

	[Fact]
	public async Task GetByIdWithTransactionsAsync_LimitsTo20Transactions()
	{
		var repo = CreateRepository(out var ctx);
		var account = TestHelpers.CreateTestAccount(_userId);
		ctx.Accounts.Add(account);
		for (int i = 0; i < 20; i++)
		{
			ctx.Transactions.Add(TestHelpers.CreateTestTransaction(
				account.Id, date: DateTime.UtcNow.AddDays(-i)));
		}
		await ctx.SaveChangesAsync();

		var result = await repo.GetByIdWithTransactionsAsync(account.Id);

		result!.Transactions.Should().HaveCount(20);
	}

	[Fact]
	public async Task GetTotalBalanceByUserIdAsync_ReturnsSumOfBalances()
	{
		var repo = CreateRepository(out var ctx);
		ctx.Accounts.AddRange(
			TestHelpers.CreateTestAccount(_userId, balance: 1000m),
			TestHelpers.CreateTestAccount(_userId, balance: 2500m)
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetTotalBalanceByUserIdAsync(_userId);

		result.Should().Be(3500m);
	}

	[Fact]
	public async Task GetTotalBalanceByUserIdAsync_ReturnsZero_WhenNoAccounts()
	{
		var repo = CreateRepository(out _);

		var result = await repo.GetTotalBalanceByUserIdAsync(Guid.NewGuid());

		result.Should().Be(0m);
	}

	[Fact]
	public async Task GetTotalBalanceByUserIdAsync_ExcludesOtherUsers()
	{
		var repo = CreateRepository(out var ctx);
		ctx.Accounts.AddRange(
			TestHelpers.CreateTestAccount(_userId, balance: 1000m),
			TestHelpers.CreateTestAccount(Guid.NewGuid(), balance: 9999m)
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetTotalBalanceByUserIdAsync(_userId);

		result.Should().Be(1000m);
	}
}
