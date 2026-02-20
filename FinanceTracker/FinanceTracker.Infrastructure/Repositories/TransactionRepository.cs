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
	public class TransactionRepository : Repository<Transactions>, ITransactionRepository
	{
		public TransactionRepository(DataContext context) : base(context)
		{
		}

		public async Task<IEnumerable<Transactions>> GetByAccountIdAsync(Guid accountId, int count = 20)
		{
			return await _dbSet
				.Include(t => t.Account)
				.Where(t => t.AccountId == accountId)
				.OrderByDescending(t => t.Date)
				.Take(count)
				.ToListAsync();
		}

		public async Task<IEnumerable<Transactions>> GetFilteredAsync(
			Guid userId,
			Guid? accountId = null,
			TransactionType? transactionType = null,
			TransactionCategory? category = null,
			DateTime? startDate = null,
			DateTime? endDate = null,
			int pageNumber = 1,
			int pageSize = 20)
		{
			var query = BuildFilteredQuery(userId, accountId, transactionType, category, startDate, endDate);

			return await query
				.OrderByDescending(t => t.Date)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();
		}

		public async Task<int> GetCountAsync(
			Guid userId,
			Guid? accountId = null,
			TransactionType? transactionType = null,
			TransactionCategory? category = null,
			DateTime? startDate = null,
			DateTime? endDate = null)
		{
			var query = BuildFilteredQuery(userId, accountId, transactionType, category, startDate, endDate);
			return await query.CountAsync();
		}

		private IQueryable<Transactions> BuildFilteredQuery(
			Guid userId,
			Guid? accountId,
			TransactionType? transactionType,
			TransactionCategory? category,
			DateTime? startDate,
			DateTime? endDate)
		{
			var query = _dbSet
				.Include(t => t.Account)
				.Where(t => t.Account.UserId == userId);

			if (accountId.HasValue)
				query = query.Where(t => t.AccountId == accountId.Value);

			if (transactionType.HasValue)
				query = query.Where(t => t.TransactionType == transactionType.Value);

			if (category.HasValue)
				query = query.Where(t => t.Category == category.Value);

			if (startDate.HasValue)
				query = query.Where(t => t.Date >= startDate.Value);

			if (endDate.HasValue)
				query = query.Where(t => t.Date <= endDate.Value);

			return query;
		}
	}
}
