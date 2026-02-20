using AutoMapper;
using FinanceTracker.Application.DTOs.Transaction;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Services
{
	public class TransactionService : ITransactionService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly ILogger<TransactionService> _logger;

		public TransactionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TransactionService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_logger = logger;
		}

		public async Task<TransactionPagedResultDto> GetByUserIdAsync(Guid userId, TransactionFilterDto filter)
		{
			TransactionType? transactionType = null;
			if (!string.IsNullOrEmpty(filter.TransactionType))
				transactionType = Enum.Parse<TransactionType>(filter.TransactionType, true);

			TransactionCategory? category = null;
			if (!string.IsNullOrEmpty(filter.Category))
				category = Enum.Parse<TransactionCategory>(filter.Category, true);

			var transactions = await _unitOfWork.Transactions.GetFilteredAsync(
				userId,
				filter.AccountId,
				transactionType,
				category,
				filter.StartDate,
				filter.EndDate,
				filter.PageNumber,
				filter.PageSize);

			var totalCount = await _unitOfWork.Transactions.GetCountAsync(
				userId,
				filter.AccountId,
				transactionType,
				category,
				filter.StartDate,
				filter.EndDate);

			var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

			return new TransactionPagedResultDto(
				Items: _mapper.Map<IEnumerable<TransactionDto>>(transactions),
				TotalCount: totalCount,
				PageNumber: filter.PageNumber,
				PageSize: filter.PageSize,
				TotalPages: totalPages
			);
		}

		public async Task<TransactionDto?> GetByIdAsync(Guid userId, Guid transactionId)
		{
			var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId);
			if (transaction == null) return null;

			var account = await _unitOfWork.Accounts.GetByIdAsync(transaction.AccountId);
			if (account == null || account.UserId != userId)
			{
				_logger.LogWarning("Transaction {TransactionId} not found for user {UserId}", transactionId, userId);
				return null;
			}

			transaction.Account = account;
			return _mapper.Map<TransactionDto>(transaction);
		}

		public async Task<IEnumerable<TransactionDto>> GetByAccountIdAsync(Guid userId, Guid accountId, int count = 20)
		{
			var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
			if (account == null || account.UserId != userId)
				throw new ArgumentException("Account not found.");

			var transactions = await _unitOfWork.Transactions.GetByAccountIdAsync(accountId, count);
			return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
		}

		public async Task<TransactionDto> CreateAsync(Guid userId, CreateTransactionDto request)
		{
			var account = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId);
			if (account == null || account.UserId != userId)
				throw new ArgumentException("Account not found.");

			if (!Enum.TryParse<TransactionType>(request.TransactionType, true, out var transactionType))
				throw new ArgumentException($"Invalid transaction type: {request.TransactionType}");

			if (!Enum.TryParse<TransactionCategory>(request.Category, true, out var category))
				throw new ArgumentException($"Invalid category: {request.Category}");

			var transaction = new Transactions
			{
				Id = Guid.NewGuid(),
				AccountId = request.AccountId,
				Amount = request.Amount,
				TransactionType = transactionType,
				Category = category,
				Description = request.Description,
				Date = request.Date,
				CreatedAt = DateTime.UtcNow
			};

			// Update account balance
			switch (transactionType)
			{
				case TransactionType.Deposit:
				case TransactionType.TransferIn:
					account.CurrentBalance += request.Amount;
					break;
				case TransactionType.Withdrawal:
				case TransactionType.TransferOut:
					account.CurrentBalance -= request.Amount;
					break;
			}

			account.UpdatedAt = DateTime.UtcNow;
			_unitOfWork.Accounts.Update(account);
			await _unitOfWork.Transactions.AddAsync(transaction);
			await _unitOfWork.SaveChangesAsync();

			_logger.LogInformation("Created {TransactionType} transaction {TransactionId} of {Amount} on account {AccountId} for user {UserId}",
				transactionType, transaction.Id, request.Amount, request.AccountId, userId);

			transaction.Account = account;
			return _mapper.Map<TransactionDto>(transaction);
		}

		public async Task<bool> DeleteAsync(Guid userId, Guid transactionId)
		{
			var transaction = await _unitOfWork.Transactions.GetByIdAsync(transactionId);
			if (transaction == null) return false;

			var account = await _unitOfWork.Accounts.GetByIdAsync(transaction.AccountId);
			if (account == null || account.UserId != userId) return false;

			// Reverse the balance change
			switch (transaction.TransactionType)
			{
				case TransactionType.Deposit:
				case TransactionType.TransferIn:
					account.CurrentBalance -= transaction.Amount;
					break;
				case TransactionType.Withdrawal:
				case TransactionType.TransferOut:
					account.CurrentBalance += transaction.Amount;
					break;
			}

			account.UpdatedAt = DateTime.UtcNow;
			_unitOfWork.Accounts.Update(account);
			_unitOfWork.Transactions.Delete(transaction);
			await _unitOfWork.SaveChangesAsync();

			_logger.LogInformation("Deleted transaction {TransactionId}, reversed {Amount} on account {AccountId}", transactionId, transaction.Amount, transaction.AccountId);
			return true;
		}
	}
}
