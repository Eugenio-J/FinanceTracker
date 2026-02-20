using FinanceTracker.Application.DTOs.Dashboard;
using FinanceTracker.Application.DTOs.Expense;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.Interfaces
{
	public interface IDashboardService
	{
		Task<DashboardDto> GetDashboardAsync(Guid userId);
		Task<IEnumerable<AccountBalanceDto>> GetAccountBalancesAsync(Guid userId);
		Task<SalaryCountdownDto> GetSalaryCountdownAsync(Guid userId);
		Task<ExpenseSummaryDto> GetMonthToDateExpensesAsync(Guid userId);
	}
}
