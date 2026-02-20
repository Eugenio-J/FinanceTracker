using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.DTOs.Expense;
using FinanceTracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ExpensesController : ControllerBase
	{
		private readonly IExpenseService _expenseService;
		private readonly ILogger<ExpensesController> _logger;

		public ExpensesController(IExpenseService expenseService, ILogger<ExpensesController> logger)
		{
			_expenseService = expenseService;
			_logger = logger;
		}

		private Guid GetUserId() =>
			Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

		[HttpGet]
		public async Task<IActionResult> GetExpenses(
			[FromQuery] DateTime? startDate = null,
			[FromQuery] DateTime? endDate = null)
		{
			var userId = GetUserId();
			_logger.LogInformation("Getting expenses for user {UserId}", userId);
			var expenses = await _expenseService.GetExpensesByUserIdAsync(
				userId, startDate, endDate);
			return Ok(Result<IEnumerable<ExpenseDto>>.Success(expenses));
		}

		[HttpPost]
		public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDto dto)
		{
			var userId = GetUserId();
			_logger.LogInformation("Creating expense for user {UserId}", userId);
			var expense = await _expenseService.CreateExpenseAsync(userId, dto);
			return StatusCode(201, Result<ExpenseDto>.Created(expense));
		}

		[HttpGet("summary")]
		public async Task<IActionResult> GetMonthlySummary(
			[FromQuery] int? year = null,
			[FromQuery] int? month = null)
		{
			var userId = GetUserId();
			var now = DateTime.UtcNow;
			_logger.LogInformation("Getting expense summary for user {UserId}, {Year}-{Month}", userId, year ?? now.Year, month ?? now.Month);
			var summary = await _expenseService.GetMonthlySummaryAsync(
				userId, year ?? now.Year, month ?? now.Month);
			return Ok(Result<ExpenseSummaryDto>.Success(summary));
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteExpense(Guid id)
		{
			var userId = GetUserId();
			_logger.LogInformation("Deleting expense {ExpenseId} for user {UserId}", id, userId);
			await _expenseService.DeleteExpenseAsync(userId, id);
			return NoContent();
		}
	}
}
