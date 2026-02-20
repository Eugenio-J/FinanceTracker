using FinanceTracker.Application.DTOs.Transaction;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.Interfaces
{
	public interface ITransactionService
	{
		Task<TransactionPagedResultDto> GetByUserIdAsync(
			Guid userId, TransactionFilterDto filter);
		Task<TransactionDto?> GetByIdAsync(Guid userId, Guid transactionId);
		Task<IEnumerable<TransactionDto>> GetByAccountIdAsync(
			Guid userId, Guid accountId, int count = 20);
		Task<TransactionDto> CreateAsync(Guid userId, CreateTransactionDto request);
		Task<bool> DeleteAsync(Guid userId, Guid transactionId);
	}
}
