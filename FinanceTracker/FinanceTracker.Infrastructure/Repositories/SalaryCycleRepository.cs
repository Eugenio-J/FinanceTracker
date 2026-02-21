using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Helpers;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Infrastructure.Repositories
{
	public class SalaryCycleRepository : Repository<SalaryCycle>, ISalaryCycleRepository
	{
		public SalaryCycleRepository(DataContext context) : base(context)
		{
		}

		public async Task<SalaryCycle?> GetByIdWithDistributionsAsync(Guid id)
		{
			return await _dbSet
				.Include(x => x.Distributions)
				.Where(s => s.Id == id)
				.FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<SalaryCycle>> GetByUserIdAsync(Guid userId, int count = 6)
		{
			return await _dbSet
				.Where(s => s.UserId == userId)
				.OrderByDescending(s => s.PayDate)
				.Take(count)
				.ToListAsync();
		}

		public async Task<SalaryCycle?> GetLatestByUserIdAsync(Guid userId)
		{
			return await _dbSet
				.Where(s => s.UserId == userId)
				.OrderByDescending(s => s.PayDate)
				.FirstOrDefaultAsync();
		}

		public async Task<DateTime?> GetNextPayDateAsync(Guid userId)
		{
			return await _dbSet
				.Where(s => s.UserId == userId && s.PayDate > PhilippineDateTime.Now)
				.OrderBy(s => s.PayDate)
				.Select(s => (DateTime?)s.PayDate)
				.FirstOrDefaultAsync();
		}
	}
}
	