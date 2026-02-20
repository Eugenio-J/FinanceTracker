using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Infrastructure.Repositories
{
	public class ExpenseRepository : Repository<Expense>, IExpenseRepository
	{
		public ExpenseRepository(DataContext context) : base(context) { }

		public async Task<IEnumerable<Expense>> GetByAccountIdAsync(Guid accountId, DateTime? startDate = null, DateTime? endDate = null)
		{
			return await _dbSet
				.Where(a => a.AccountId == accountId &&
					(!startDate.HasValue || a.Date >= startDate.Value) &&
					(!endDate.HasValue || a.Date <= endDate.Value))
				.ToListAsync();
		}

		public async Task<IEnumerable<Expense>> GetByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
		{
			return await _dbSet
				.Where(e => e.UserId == userId &&
					(!startDate.HasValue || e.Date >= startDate.Value) &&
					(!endDate.HasValue || e.Date <= endDate.Value))
				.ToListAsync();
		}

		public async Task<Dictionary<Guid, decimal>> GetMonthlySummaryByAccountAsync(Guid userId, int year, int month)
		{
			//return await _dbSet
			//	.Where(e => e.UserId == userId && e.Date.Year == year && e.Date.Month == month)
			//	.GroupBy(e => e.AccountId)
			//	.ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.Amount));

			var startDate = new DateTime(year, month, 1);
			var endDate = startDate.AddMonths(1);

			var data = await _dbSet
				.Where(e => e.UserId == userId &&
							e.Date >= startDate &&
							e.Date < endDate)
				.GroupBy(e => e.AccountId)
				.Select(g => new
				{
					Expense = g.Key,
					Total = g.Sum(e => e.Amount)
				})
				.ToListAsync();

			return data.ToDictionary(x => x.Expense, x => x.Total);
		}

		public async Task<Dictionary<ExpenseCategory, decimal>> GetMonthlySummaryByCategoryAsync(Guid userId, int year, int month)
		{
			//return await _dbSet
			//	.Where(e => e.UserId == userId && e.Date.Year == year && e.Date.Month == month)
			//	.GroupBy(e => e.Category)
			//	.ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.Amount));

			var startDate = new DateTime(year, month, 1);
			var endDate = startDate.AddMonths(1);

			var data = await _dbSet
				.Where(e => e.UserId == userId &&
							e.Date >= startDate &&
							e.Date < endDate)
				.GroupBy(e => e.Category)
				.Select(g => new
				{
					Category = g.Key,
					Total = g.Sum(e => e.Amount)
				})
				.ToListAsync();

			return data.ToDictionary(x => x.Category, x => x.Total);
		}	
	}
}
