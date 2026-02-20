using FinanceTracker.Domain.Enums;
using FinanceTracker.Infrastructure.Repositories;
using FinanceTracker.Tests.Helpers;
using FluentAssertions;

namespace FinanceTracker.Tests.IntegrationTests.Repositories;

public class ExpenseRepositoryTests
{
	private readonly Guid _userId = Guid.NewGuid();
	private readonly Guid _accountId = Guid.NewGuid();

	private ExpenseRepository CreateRepository(out Infrastructure.Data.DataContext context)
	{
		context = TestHelpers.CreateInMemoryContext();
		return new ExpenseRepository(context);
	}

	[Fact]
	public async Task GetByUserIdAsync_ReturnsExpensesForUser()
	{
		var repo = CreateRepository(out var ctx);
		ctx.Expenses.AddRange(
			TestHelpers.CreateTestExpense(_userId, _accountId),
			TestHelpers.CreateTestExpense(_userId, _accountId),
			TestHelpers.CreateTestExpense(Guid.NewGuid(), _accountId)
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetByUserIdAsync(_userId);

		result.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetByUserIdAsync_FiltersBy_StartDate()
	{
		var repo = CreateRepository(out var ctx);
		ctx.Expenses.AddRange(
			TestHelpers.CreateTestExpense(_userId, _accountId, date: new DateTime(2026, 1, 15)),
			TestHelpers.CreateTestExpense(_userId, _accountId, date: new DateTime(2026, 2, 15)),
			TestHelpers.CreateTestExpense(_userId, _accountId, date: new DateTime(2026, 3, 15))
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetByUserIdAsync(_userId, startDate: new DateTime(2026, 2, 1));

		result.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetByUserIdAsync_FiltersBy_EndDate()
	{
		var repo = CreateRepository(out var ctx);
		ctx.Expenses.AddRange(
			TestHelpers.CreateTestExpense(_userId, _accountId, date: new DateTime(2026, 1, 15)),
			TestHelpers.CreateTestExpense(_userId, _accountId, date: new DateTime(2026, 2, 15)),
			TestHelpers.CreateTestExpense(_userId, _accountId, date: new DateTime(2026, 3, 15))
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetByUserIdAsync(_userId, endDate: new DateTime(2026, 2, 28));

		result.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetByUserIdAsync_FiltersBy_DateRange()
	{
		var repo = CreateRepository(out var ctx);
		ctx.Expenses.AddRange(
			TestHelpers.CreateTestExpense(_userId, _accountId, date: new DateTime(2026, 1, 15)),
			TestHelpers.CreateTestExpense(_userId, _accountId, date: new DateTime(2026, 2, 15)),
			TestHelpers.CreateTestExpense(_userId, _accountId, date: new DateTime(2026, 3, 15))
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetByUserIdAsync(_userId,
			startDate: new DateTime(2026, 2, 1),
			endDate: new DateTime(2026, 2, 28));

		result.Should().HaveCount(1);
	}

	[Fact]
	public async Task GetByAccountIdAsync_ReturnsExpensesForAccount()
	{
		var repo = CreateRepository(out var ctx);
		var otherAccountId = Guid.NewGuid();
		ctx.Expenses.AddRange(
			TestHelpers.CreateTestExpense(_userId, _accountId),
			TestHelpers.CreateTestExpense(_userId, _accountId),
			TestHelpers.CreateTestExpense(_userId, otherAccountId)
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetByAccountIdAsync(_accountId);

		result.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetByAccountIdAsync_FiltersBy_DateRange()
	{
		var repo = CreateRepository(out var ctx);
		ctx.Expenses.AddRange(
			TestHelpers.CreateTestExpense(_userId, _accountId, date: new DateTime(2026, 1, 15)),
			TestHelpers.CreateTestExpense(_userId, _accountId, date: new DateTime(2026, 3, 15))
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetByAccountIdAsync(_accountId,
			startDate: new DateTime(2026, 2, 1),
			endDate: new DateTime(2026, 2, 28));

		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GetMonthlySummaryByCategoryAsync_GroupsAndSums()
	{
		var repo = CreateRepository(out var ctx);
		ctx.Expenses.AddRange(
			TestHelpers.CreateTestExpense(_userId, _accountId, 100m, ExpenseCategory.Food, new DateTime(2026, 2, 1)),
			TestHelpers.CreateTestExpense(_userId, _accountId, 200m, ExpenseCategory.Food, new DateTime(2026, 2, 15)),
			TestHelpers.CreateTestExpense(_userId, _accountId, 50m, ExpenseCategory.Transport, new DateTime(2026, 2, 10)),
			TestHelpers.CreateTestExpense(_userId, _accountId, 75m, ExpenseCategory.Food, new DateTime(2026, 3, 1)) // different month
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetMonthlySummaryByCategoryAsync(_userId, 2026, 2);

		result.Should().HaveCount(2);
		result[ExpenseCategory.Food].Should().Be(300m);
		result[ExpenseCategory.Transport].Should().Be(50m);
	}

	[Fact]
	public async Task GetMonthlySummaryByCategoryAsync_ReturnsEmpty_WhenNoExpenses()
	{
		var repo = CreateRepository(out _);

		var result = await repo.GetMonthlySummaryByCategoryAsync(_userId, 2026, 2);

		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GetMonthlySummaryByCategoryAsync_ExcludesOtherUsers()
	{
		var repo = CreateRepository(out var ctx);
		ctx.Expenses.AddRange(
			TestHelpers.CreateTestExpense(_userId, _accountId, 100m, ExpenseCategory.Food, new DateTime(2026, 2, 1)),
			TestHelpers.CreateTestExpense(Guid.NewGuid(), _accountId, 999m, ExpenseCategory.Food, new DateTime(2026, 2, 1))
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetMonthlySummaryByCategoryAsync(_userId, 2026, 2);

		result[ExpenseCategory.Food].Should().Be(100m);
	}

	[Fact]
	public async Task GetMonthlySummaryByAccountAsync_GroupsAndSums()
	{
		var repo = CreateRepository(out var ctx);
		var accountId2 = Guid.NewGuid();
		ctx.Expenses.AddRange(
			TestHelpers.CreateTestExpense(_userId, _accountId, 100m, date: new DateTime(2026, 2, 1)),
			TestHelpers.CreateTestExpense(_userId, _accountId, 200m, date: new DateTime(2026, 2, 15)),
			TestHelpers.CreateTestExpense(_userId, accountId2, 150m, date: new DateTime(2026, 2, 10))
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetMonthlySummaryByAccountAsync(_userId, 2026, 2);

		result.Should().HaveCount(2);
		result[_accountId].Should().Be(300m);
		result[accountId2].Should().Be(150m);
	}

	[Fact]
	public async Task GetMonthlySummaryByAccountAsync_ReturnsEmpty_WhenNoExpenses()
	{
		var repo = CreateRepository(out _);

		var result = await repo.GetMonthlySummaryByAccountAsync(_userId, 2026, 2);

		result.Should().BeEmpty();
	}
}
