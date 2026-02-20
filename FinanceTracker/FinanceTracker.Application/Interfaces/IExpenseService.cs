using FinanceTracker.Application.DTOs.Expense;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.Interfaces
{
	public interface IExpenseService
	{
		Task<IEnumerable<ExpenseDto>> GetExpensesByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
		Task<ExpenseDto> CreateExpenseAsync(Guid userId, CreateExpenseDto dto);
		Task<ExpenseSummaryDto> GetMonthlySummaryAsync(Guid userId, int year, int month);
		Task DeleteExpenseAsync(Guid userId, Guid expenseId);
	}
}
