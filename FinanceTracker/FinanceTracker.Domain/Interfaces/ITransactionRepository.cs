using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Domain.Interfaces
{
	public interface ITransactionRepository : IRepository<Transactions>
	{
		Task<IEnumerable<Transactions>> GetByAccountIdAsync(Guid accountId, int count = 20);
		Task<IEnumerable<Transactions>> GetFilteredAsync(
			Guid userId,
			Guid? accountId = null,
			TransactionType? transactionType = null,
			TransactionCategory? category = null,
			DateTime? startDate = null,
			DateTime? endDate = null,
			int pageNumber = 1,
			int pageSize = 20);
		Task<int> GetCountAsync(
			Guid userId,
			Guid? accountId = null,
			TransactionType? transactionType = null,
			TransactionCategory? category = null,
			DateTime? startDate = null,
			DateTime? endDate = null);
	}

}
