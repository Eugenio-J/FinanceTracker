using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Domain.Interfaces
{
	public interface IUnitOfWork : IDisposable
	{
		IUserRepository Users { get; }
		IAccountRepository Accounts { get; }
		ITransactionRepository Transactions { get; }
		ISalaryCycleRepository SalaryCycles { get; }
		IExpenseRepository Expenses { get; }
		IRefreshTokenRepository RefreshTokens { get; }

		Task<int> SaveChangesAsync();
		Task BeginTransactionAsync();
		Task CommitTransactionAsync();
		Task RollbackTransactionAsync();
	}
}
