using FinanceTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Domain.Interfaces
{
	public interface ISalaryCycleRepository : IRepository<SalaryCycle>
	{
		Task<IEnumerable<SalaryCycle>> GetByUserIdAsync(Guid userId, int count = 6);
		Task<SalaryCycle?> GetByIdWithDistributionsAsync(Guid id);
		Task<SalaryCycle?> GetLatestByUserIdAsync(Guid userId);
		Task<DateTime?> GetNextPayDateAsync(Guid userId);
	}
}
