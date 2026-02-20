using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Infrastructure.Repositories
{
	public class AccountRepository : Repository<Account>, IAccountRepository
	{
		public AccountRepository(DataContext context) : base(context) { }

		public async Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId)
		{
			return await _dbSet
				.Where(a => a.UserId == userId)
				.OrderBy(a => a.AccountType)
				.ToListAsync();
		}

		public async Task<Account?> GetByIdWithTransactionsAsync(Guid id)
		{
			return await _dbSet
				.Include(a => a.Transactions.OrderByDescending(t => t.Date).Take(20))
				.FirstOrDefaultAsync(a => a.Id == id);
		}

		public async Task<decimal> GetTotalBalanceByUserIdAsync(Guid userId)
		{
			return await _dbSet
				.Where(a => a.UserId == userId)
				.SumAsync(a => a.CurrentBalance);
		}
	}
}
