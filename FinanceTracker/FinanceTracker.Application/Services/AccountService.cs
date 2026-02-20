using AutoMapper;
using FinanceTracker.Application.DTOs.Account;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Services
{
	public class AccountService : IAccountService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly ILogger<AccountService> _logger;

		public AccountService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AccountService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_logger = logger;
		}

		public async Task<IEnumerable<AccountDto>> GetAllByUserIdAsync(Guid userId)
		{
			var accounts = await _unitOfWork.Accounts.GetByUserIdAsync(userId);
			return _mapper.Map<IEnumerable<AccountDto>>(accounts);
		}

		public async Task<AccountDto?> GetByIdAsync(Guid userId, Guid accountId)
		{
			var account = await _unitOfWork.Accounts.GetByIdWithTransactionsAsync(accountId);
			if (account == null || account.UserId != userId)
			{
				_logger.LogWarning("Account {AccountId} not found for user {UserId}", accountId, userId);
				return null;
			}

			return _mapper.Map<AccountDto>(account);
		}

		public async Task<AccountDto> CreateAsync(Guid userId, CreateAccountDto request)
		{
			if (!Enum.TryParse<AccountType>(request.AccountType, true, out var accountType))
				throw new ArgumentException($"Invalid account type: {request.AccountType}");

			var account = new Account
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				Name = request.Name,
				AccountType = accountType,
				CurrentBalance = request.InitialBalance,
				CreatedAt = DateTime.UtcNow
			};

			await _unitOfWork.Accounts.AddAsync(account);

			// Create initial deposit transaction if balance > 0
			if (request.InitialBalance > 0)
			{
				var transaction = new Transactions
				{
					Id = Guid.NewGuid(),
					AccountId = account.Id,
					Amount = request.InitialBalance,
					TransactionType = TransactionType.Deposit,
					Category = TransactionCategory.Adjustment,
					Description = "Initial deposit",
					Date = DateTime.UtcNow,
					CreatedAt = DateTime.UtcNow
				};
				await _unitOfWork.Transactions.AddAsync(transaction);
			}

			await _unitOfWork.SaveChangesAsync();

			_logger.LogInformation("Created account {AccountId} ({AccountType}) with balance {Balance} for user {UserId}",
				account.Id, accountType, request.InitialBalance, userId);

			account.Transactions = new List<Transactions>();
			return _mapper.Map<AccountDto>(account);
		}

		public async Task<AccountDto> UpdateAsync(Guid userId, Guid accountId, UpdateAccountDto request)
		{
			var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
			if (account == null || account.UserId != userId)
				throw new ArgumentException("Account not found.");

			account.Name = request.Name;
			account.UpdatedAt = DateTime.UtcNow;
			_unitOfWork.Accounts.Update(account);
			await _unitOfWork.SaveChangesAsync();

			_logger.LogInformation("Updated account {AccountId} for user {UserId}", accountId, userId);
			return _mapper.Map<AccountDto>(account);
		}

		public async Task<bool> DeleteAsync(Guid userId, Guid accountId)
		{
			var account = await _unitOfWork.Accounts.GetByIdAsync(accountId);
			if (account == null || account.UserId != userId)
			{
				_logger.LogWarning("Delete failed - account {AccountId} not found for user {UserId}", accountId, userId);
				return false;
			}

			_unitOfWork.Accounts.Delete(account);
			await _unitOfWork.SaveChangesAsync();
			_logger.LogInformation("Deleted account {AccountId} for user {UserId}", accountId, userId);
			return true;
		}

		public async Task<decimal> GetTotalBalanceAsync(Guid userId)
		{
			return await _unitOfWork.Accounts.GetTotalBalanceByUserIdAsync(userId);
		}
	}
}
