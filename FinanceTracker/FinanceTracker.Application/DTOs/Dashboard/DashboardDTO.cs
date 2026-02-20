using FinanceTracker.Application.DTOs.Account;
using FinanceTracker.Application.DTOs.Expense;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.DTOs.Dashboard
{
	public record DashboardDto(
		decimal TotalBalance,
		List<AccountBalanceDto> AccountBalances,
		ExpenseSummaryDto MonthToDateExpenses,
		SalaryCountdownDto SalaryCountdown,
		List<SalaryCycleHistoryDto> RecentSalaryCycles
	);

	public record AccountBalanceDto(
		Guid Id,
		string Name,
		string AccountType,
		decimal Balance
	);

	public record SalaryCountdownDto(
		DateTime NextPayDate,
		int DaysUntilPayday
	);

	public record SalaryCycleHistoryDto(
		Guid Id,
		DateTime PayDate,
		decimal NetSalary,
		string Status
	);
}
