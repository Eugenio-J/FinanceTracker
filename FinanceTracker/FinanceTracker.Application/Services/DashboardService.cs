using System.Diagnostics;
using FinanceTracker.Application.DTOs.Dashboard;
using FinanceTracker.Application.DTOs.Expense;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Helpers;
using FinanceTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Services
{
	public class DashboardService : IDashboardService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IExpenseService _expenseService;
		private readonly ISalaryCycleService _salaryCycleService;
		private readonly ILogger<DashboardService> _logger;

		public DashboardService(
			IUnitOfWork unitOfWork,
			IExpenseService expenseService,
			ISalaryCycleService salaryCycleService,
			ILogger<DashboardService> logger)
		{
			_unitOfWork = unitOfWork;
			_expenseService = expenseService;
			_salaryCycleService = salaryCycleService;
			_logger = logger;
		}

		public async Task<DashboardDto> GetDashboardAsync(Guid userId)
		{
			var sw = Stopwatch.StartNew();

			var totalBalance = await _unitOfWork.Accounts.GetTotalBalanceByUserIdAsync(userId);
			var accountBalances = (await GetAccountBalancesAsync(userId)).ToList();
			var expenses = await GetMonthToDateExpensesAsync(userId);
			var salaryCountdown = await GetSalaryCountdownAsync(userId);
			var recentCycles = await _salaryCycleService.GetRecentCyclesAsync(userId, 6);

			var cycleHistory = recentCycles.Select(c => new SalaryCycleHistoryDto(
				c.Id, c.PayDate, c.NetSalary, c.Status
			)).ToList();

			sw.Stop();
			_logger.LogInformation("Dashboard aggregated for user {UserId} in {ElapsedMs}ms", userId, sw.ElapsedMilliseconds);

			return new DashboardDto(
				TotalBalance: totalBalance,
				AccountBalances: accountBalances,
				MonthToDateExpenses: expenses,
				SalaryCountdown: salaryCountdown,
				RecentSalaryCycles: cycleHistory
			);
		}

		public async Task<IEnumerable<AccountBalanceDto>> GetAccountBalancesAsync(Guid userId)
		{
			var accounts = await _unitOfWork.Accounts.GetByUserIdAsync(userId);
			return accounts.Select(a => new AccountBalanceDto(
				a.Id, a.Name, a.AccountType.ToString(), a.CurrentBalance
			));
		}

		public async Task<SalaryCountdownDto> GetSalaryCountdownAsync(Guid userId)
		{
			var nextPayDate = await _salaryCycleService.GetNextPayDateAsync(userId);

			if (nextPayDate == null)
				return new SalaryCountdownDto(DateTime.MinValue, -1);

			var daysUntil = (int)(nextPayDate.Value.Date - PhilippineDateTime.Now.Date).TotalDays;
			return new SalaryCountdownDto(nextPayDate.Value, daysUntil);
		}

		public async Task<ExpenseSummaryDto> GetMonthToDateExpensesAsync(Guid userId)
		{
			var now = PhilippineDateTime.Now;
			return await _expenseService.GetMonthlySummaryAsync(userId, now.Year, now.Month);
		}
	}
}
