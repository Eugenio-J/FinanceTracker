using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.DTOs.Dashboard;
using FinanceTracker.Application.DTOs.Expense;
using FinanceTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class DashboardController : ControllerBase
	{
		private readonly IDashboardService _dashboardService;
		private readonly ILogger<DashboardController> _logger;

		public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
		{
			_dashboardService = dashboardService;
			_logger = logger;
		}

		private Guid GetUserId() =>
			Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

		[HttpGet("get-dashboard")]
		public async Task<IActionResult> GetDashboard()
		{
			var userId = GetUserId();
			_logger.LogInformation("Getting dashboard for user {UserId}", userId);
			var dashboard = await _dashboardService.GetDashboardAsync(userId);
			return Ok(Result<DashboardDto>.Success(dashboard));
		}

		[HttpGet("balances")]
		public async Task<IActionResult> GetAccountBalances()
		{
			var userId = GetUserId();
			_logger.LogInformation("Getting account balances for user {UserId}", userId);
			var balances = await _dashboardService.GetAccountBalancesAsync(userId);
			return Ok(Result<IEnumerable<AccountBalanceDto>>.Success(balances));
		}

		[HttpGet("salary-countdown")]
		public async Task<IActionResult> GetSalaryCountdown()
		{
			var userId = GetUserId();
			_logger.LogInformation("Getting salary countdown for user {UserId}", userId);
			var countdown = await _dashboardService.GetSalaryCountdownAsync(userId);
			return Ok(Result<SalaryCountdownDto>.Success(countdown));
		}

		[HttpGet("expenses")]
		public async Task<IActionResult> GetMonthToDateExpenses()
		{
			var userId = GetUserId();
			_logger.LogInformation("Getting month-to-date expenses for user {UserId}", userId);
			var expenses = await _dashboardService.GetMonthToDateExpensesAsync(userId);
			return Ok(Result<ExpenseSummaryDto>.Success(expenses));
		}
	}
}
