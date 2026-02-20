using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Domain.Interfaces
{
	public interface IExpenseRepository : IRepository<Expense>
	{
		Task<IEnumerable<Expense>> GetByUserIdAsync(
			Guid userId, DateTime? startDate = null, DateTime? endDate = null);
		Task<IEnumerable<Expense>> GetByAccountIdAsync(
			Guid accountId, DateTime? startDate = null, DateTime? endDate = null);
		Task<Dictionary<ExpenseCategory, decimal>> GetMonthlySummaryByCategoryAsync(
			Guid userId, int year, int month);
		Task<Dictionary<Guid, decimal>> GetMonthlySummaryByAccountAsync(
			Guid userId, int year, int month);
	}
}
