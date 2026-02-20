using FinanceTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Domain.Interfaces
{
	public interface IAccountRepository : IRepository<Account>
	{
		Task<IEnumerable<Account>> GetByUserIdAsync(Guid userId);
		Task<Account?> GetByIdWithTransactionsAsync(Guid id);
		Task<decimal> GetTotalBalanceByUserIdAsync(Guid userId);
	}
}
