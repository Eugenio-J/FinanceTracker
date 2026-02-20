using FinanceTracker.Application.DTOs.Dashboard;
using FinanceTracker.Application.DTOs.Expense;
using FinanceTracker.Application.DTOs.SalaryCycle;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Application.Services;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinanceTracker.Tests.UnitTests.Services;

public class DashboardServiceTests
{
	private readonly Mock<IUnitOfWork> _unitOfWork;
	private readonly Mock<IExpenseService> _expenseService;
	private readonly Mock<ISalaryCycleService> _salaryCycleService;
	private readonly DashboardService _sut;
	private readonly Guid _userId = Guid.NewGuid();

	public DashboardServiceTests()
	{
		_unitOfWork = new Mock<IUnitOfWork>();
		_expenseService = new Mock<IExpenseService>();
		_salaryCycleService = new Mock<ISalaryCycleService>();
		_sut = new DashboardService(_unitOfWork.Object, _expenseService.Object, _salaryCycleService.Object, NullLogger<DashboardService>.Instance);
	}

	[Fact]
	public async Task GetDashboardAsync_ReturnsAggregatedDashboard()
	{
		var accountId = Guid.NewGuid();
		var accounts = new List<Account>
		{
			new Account { Id = accountId, UserId = _userId, Name = "Hub", AccountType = AccountType.Hub, CurrentBalance = 5000m }
		};
		var expenseSummary = new ExpenseSummaryDto(200m, new Dictionary<string, decimal> { { "Food", 200m } }, new Dictionary<string, decimal> { { "Hub", 200m } });
		var recentCycles = new List<SalaryCycleDto>
		{
			new SalaryCycleDto(Guid.NewGuid(), new DateTime(2026, 2, 1), 5000m, 4000m, "Completed", DateTime.UtcNow, DateTime.UtcNow, new List<SalaryDistributionDto>())
		};

		_unitOfWork.Setup(u => u.Accounts.GetTotalBalanceByUserIdAsync(_userId)).ReturnsAsync(5000m);
		_unitOfWork.Setup(u => u.Accounts.GetByUserIdAsync(_userId)).ReturnsAsync(accounts);
		_expenseService.Setup(s => s.GetMonthlySummaryAsync(_userId, It.IsAny<int>(), It.IsAny<int>()))
			.ReturnsAsync(expenseSummary);
		_salaryCycleService.Setup(s => s.GetNextPayDateAsync(_userId)).ReturnsAsync(new DateTime(2026, 2, 15));
		_salaryCycleService.Setup(s => s.GetRecentCyclesAsync(_userId, 6)).ReturnsAsync(recentCycles);

		var result = await _sut.GetDashboardAsync(_userId);

		result.TotalBalance.Should().Be(5000m);
		result.AccountBalances.Should().HaveCount(1);
		result.MonthToDateExpenses.TotalAmount.Should().Be(200m);
		result.RecentSalaryCycles.Should().HaveCount(1);
	}

	[Fact]
	public async Task GetDashboardAsync_ReturnsEmptyData_WhenNoData()
	{
		var expenseSummary = new ExpenseSummaryDto(0m, new Dictionary<string, decimal>(), new Dictionary<string, decimal>());

		_unitOfWork.Setup(u => u.Accounts.GetTotalBalanceByUserIdAsync(_userId)).ReturnsAsync(0m);
		_unitOfWork.Setup(u => u.Accounts.GetByUserIdAsync(_userId)).ReturnsAsync(new List<Account>());
		_expenseService.Setup(s => s.GetMonthlySummaryAsync(_userId, It.IsAny<int>(), It.IsAny<int>()))
			.ReturnsAsync(expenseSummary);
		_salaryCycleService.Setup(s => s.GetNextPayDateAsync(_userId)).ReturnsAsync((DateTime?)null);
		_salaryCycleService.Setup(s => s.GetRecentCyclesAsync(_userId, 6))
			.ReturnsAsync(new List<SalaryCycleDto>());

		var result = await _sut.GetDashboardAsync(_userId);

		result.TotalBalance.Should().Be(0m);
		result.AccountBalances.Should().BeEmpty();
		result.RecentSalaryCycles.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAccountBalancesAsync_ReturnsMappedBalances()
	{
		var accounts = new List<Account>
		{
			new Account { Id = Guid.NewGuid(), UserId = _userId, Name = "Hub", AccountType = AccountType.Hub, CurrentBalance = 3000m },
			new Account { Id = Guid.NewGuid(), UserId = _userId, Name = "Savings", AccountType = AccountType.Savings, CurrentBalance = 2000m }
		};
		_unitOfWork.Setup(u => u.Accounts.GetByUserIdAsync(_userId)).ReturnsAsync(accounts);

		var result = (await _sut.GetAccountBalancesAsync(_userId)).ToList();

		result.Should().HaveCount(2);
		result[0].Name.Should().Be("Hub");
		result[0].Balance.Should().Be(3000m);
		result[1].Name.Should().Be("Savings");
	}

	[Fact]
	public async Task GetAccountBalancesAsync_ReturnsEmpty()
	{
		_unitOfWork.Setup(u => u.Accounts.GetByUserIdAsync(_userId)).ReturnsAsync(new List<Account>());

		var result = await _sut.GetAccountBalancesAsync(_userId);

		result.Should().BeEmpty();
	}

	[Fact]
	public async Task GetSalaryCountdownAsync_ReturnsCountdown_WhenNextPayDateExists()
	{
		var nextPayDate = DateTime.UtcNow.Date.AddDays(7);
		_salaryCycleService.Setup(s => s.GetNextPayDateAsync(_userId)).ReturnsAsync(nextPayDate);

		var result = await _sut.GetSalaryCountdownAsync(_userId);

		result.NextPayDate.Should().Be(nextPayDate);
		result.DaysUntilPayday.Should().Be(7);
	}

	[Fact]
	public async Task GetSalaryCountdownAsync_ReturnsMinDateAndNegativeOne_WhenNoCycles()
	{
		_salaryCycleService.Setup(s => s.GetNextPayDateAsync(_userId)).ReturnsAsync((DateTime?)null);

		var result = await _sut.GetSalaryCountdownAsync(_userId);

		result.NextPayDate.Should().Be(DateTime.MinValue);
		result.DaysUntilPayday.Should().Be(-1);
	}

	[Fact]
	public async Task GetMonthToDateExpensesAsync_DelegatesToExpenseService()
	{
		var summary = new ExpenseSummaryDto(500m, new Dictionary<string, decimal> { { "Food", 500m } }, new Dictionary<string, decimal>());
		_expenseService.Setup(s => s.GetMonthlySummaryAsync(_userId, It.IsAny<int>(), It.IsAny<int>()))
			.ReturnsAsync(summary);

		var result = await _sut.GetMonthToDateExpensesAsync(_userId);

		result.TotalAmount.Should().Be(500m);
		_expenseService.Verify(s => s.GetMonthlySummaryAsync(_userId, It.IsAny<int>(), It.IsAny<int>()), Times.Once);
	}
}
