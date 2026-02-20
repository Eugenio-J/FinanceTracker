using FinanceTracker.API.Controllers;
using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.DTOs.Expense;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinanceTracker.Tests.UnitTests.Controllers;

public class ExpensesControllerTests
{
	private readonly Mock<IExpenseService> _expenseService;
	private readonly ExpensesController _sut;
	private readonly Guid _userId = Guid.NewGuid();

	public ExpensesControllerTests()
	{
		_expenseService = new Mock<IExpenseService>();
		_sut = new ExpensesController(_expenseService.Object, NullLogger<ExpensesController>.Instance);
		TestHelpers.SetupControllerContext(_sut, _userId);
	}

	[Fact]
	public async Task GetExpenses_ReturnsOkWithExpenses()
	{
		var expenses = new List<ExpenseDto>
		{
			new ExpenseDto(Guid.NewGuid(), Guid.NewGuid(), "Hub", 50m, "Food", "Lunch", DateTime.UtcNow, DateTime.UtcNow)
		};
		_expenseService.Setup(s => s.GetExpensesByUserIdAsync(_userId, null, null)).ReturnsAsync(expenses);

		var result = await _sut.GetExpenses();

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<IEnumerable<ExpenseDto>>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data.Should().HaveCount(1);
	}

	[Fact]
	public async Task CreateExpense_ReturnsCreatedWithExpense()
	{
		var dto = new CreateExpenseDto(Guid.NewGuid(), 100m, "Food", "Dinner", DateTime.UtcNow);
		var created = new ExpenseDto(Guid.NewGuid(), dto.AccountId, "Hub", 100m, "Food", "Dinner", DateTime.UtcNow, DateTime.UtcNow);
		_expenseService.Setup(s => s.CreateExpenseAsync(_userId, dto)).ReturnsAsync(created);

		var result = await _sut.CreateExpense(dto);

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(201);
		var wrapper = objectResult.Value.Should().BeOfType<Result<ExpenseDto>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task GetMonthlySummary_ReturnsOkWithSummary()
	{
		var summary = new ExpenseSummaryDto(500m, new Dictionary<string, decimal> { { "Food", 500m } }, new Dictionary<string, decimal>());
		_expenseService.Setup(s => s.GetMonthlySummaryAsync(_userId, 2026, 1)).ReturnsAsync(summary);

		var result = await _sut.GetMonthlySummary(2026, 1);

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<ExpenseSummaryDto>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data!.TotalAmount.Should().Be(500m);
	}

	[Fact]
	public async Task GetMonthlySummary_UsesCurrentDateDefaults()
	{
		var now = DateTime.UtcNow;
		var summary = new ExpenseSummaryDto(0m, new Dictionary<string, decimal>(), new Dictionary<string, decimal>());
		_expenseService.Setup(s => s.GetMonthlySummaryAsync(_userId, now.Year, now.Month)).ReturnsAsync(summary);

		var result = await _sut.GetMonthlySummary(null, null);

		result.Should().BeOfType<OkObjectResult>();
		_expenseService.Verify(s => s.GetMonthlySummaryAsync(_userId, now.Year, now.Month), Times.Once);
	}

	[Fact]
	public async Task DeleteExpense_ReturnsNoContent()
	{
		var id = Guid.NewGuid();
		_expenseService.Setup(s => s.DeleteExpenseAsync(_userId, id)).Returns(Task.CompletedTask);

		var result = await _sut.DeleteExpense(id);

		result.Should().BeOfType<NoContentResult>();
	}
}
