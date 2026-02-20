using FinanceTracker.API.Controllers;
using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.DTOs.Dashboard;
using FinanceTracker.Application.DTOs.Expense;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinanceTracker.Tests.UnitTests.Controllers;

public class DashboardControllerTests
{
	private readonly Mock<IDashboardService> _dashboardService;
	private readonly DashboardController _sut;
	private readonly Guid _userId = Guid.NewGuid();

	public DashboardControllerTests()
	{
		_dashboardService = new Mock<IDashboardService>();
		_sut = new DashboardController(_dashboardService.Object, NullLogger<DashboardController>.Instance);
		TestHelpers.SetupControllerContext(_sut, _userId);
	}

	[Fact]
	public async Task GetDashboard_ReturnsOkWithDashboard()
	{
		var dashboard = new DashboardDto(
			5000m,
			new List<AccountBalanceDto>(),
			new ExpenseSummaryDto(0m, new Dictionary<string, decimal>(), new Dictionary<string, decimal>()),
			new SalaryCountdownDto(DateTime.UtcNow.AddDays(7), 7),
			new List<SalaryCycleHistoryDto>()
		);
		_dashboardService.Setup(s => s.GetDashboardAsync(_userId)).ReturnsAsync(dashboard);

		var result = await _sut.GetDashboard();

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<DashboardDto>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data!.TotalBalance.Should().Be(5000m);
	}

	[Fact]
	public async Task GetAccountBalances_ReturnsOkWithBalances()
	{
		var balances = new List<AccountBalanceDto>
		{
			new AccountBalanceDto(Guid.NewGuid(), "Hub", "Hub", 3000m)
		};
		_dashboardService.Setup(s => s.GetAccountBalancesAsync(_userId)).ReturnsAsync(balances);

		var result = await _sut.GetAccountBalances();

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<IEnumerable<AccountBalanceDto>>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data.Should().HaveCount(1);
	}

	[Fact]
	public async Task GetSalaryCountdown_ReturnsOkWithCountdown()
	{
		var countdown = new SalaryCountdownDto(DateTime.UtcNow.AddDays(7), 7);
		_dashboardService.Setup(s => s.GetSalaryCountdownAsync(_userId)).ReturnsAsync(countdown);

		var result = await _sut.GetSalaryCountdown();

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<SalaryCountdownDto>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data!.DaysUntilPayday.Should().Be(7);
	}

	[Fact]
	public async Task GetMonthToDateExpenses_ReturnsOkWithExpenses()
	{
		var expenses = new ExpenseSummaryDto(500m, new Dictionary<string, decimal> { { "Food", 500m } }, new Dictionary<string, decimal>());
		_dashboardService.Setup(s => s.GetMonthToDateExpensesAsync(_userId)).ReturnsAsync(expenses);

		var result = await _sut.GetMonthToDateExpenses();

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<ExpenseSummaryDto>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data!.TotalAmount.Should().Be(500m);
	}
}
