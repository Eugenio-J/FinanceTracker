using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Infrastructure.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly DataContext _context;
		private IDbContextTransaction? _transaction;

		public IUserRepository Users { get; }
		public IAccountRepository Accounts { get; }
		public ITransactionRepository Transactions { get; }
		public ISalaryCycleRepository SalaryCycles { get; }
		public IExpenseRepository Expenses { get; }
		public IRefreshTokenRepository RefreshTokens { get; }

		public UnitOfWork(
			DataContext context,
			IUserRepository users,
			IAccountRepository accounts,
			ITransactionRepository transactions,
			ISalaryCycleRepository salaryCycles,
			IExpenseRepository expenses,
			IRefreshTokenRepository refreshTokens)
		{
			_context = context;
			Users = users;
			Accounts = accounts;
			Transactions = transactions;
			SalaryCycles = salaryCycles;
			Expenses = expenses;
			RefreshTokens = refreshTokens;
		}

		public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

		public async Task BeginTransactionAsync()
		{
			_transaction = await _context.Database.BeginTransactionAsync();
		}

		public async Task CommitTransactionAsync()
		{
			if (_transaction != null)
			{
				await _transaction.CommitAsync();
				await _transaction.DisposeAsync();
				_transaction = null;
			}
		}

		public async Task RollbackTransactionAsync()
		{
			if (_transaction != null)
			{
				await _transaction.RollbackAsync();
				await _transaction.DisposeAsync();
				_transaction = null;
			}
		}

		public void Dispose()
		{
			_transaction?.Dispose();
			_context.Dispose();
		}
	}
}
