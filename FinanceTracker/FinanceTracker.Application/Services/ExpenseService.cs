using AutoMapper;
using FinanceTracker.Application.DTOs.Expense;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Services
{
	public class ExpenseService : IExpenseService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IValidator<CreateExpenseDto> _createValidator;
		private readonly ILogger<ExpenseService> _logger;

		public ExpenseService(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			IValidator<CreateExpenseDto> createValidator,
			ILogger<ExpenseService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_createValidator = createValidator;
			_logger = logger;
		}

		public async Task<IEnumerable<ExpenseDto>> GetExpensesByUserIdAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
		{
			var expenses = await _unitOfWork.Expenses.GetByUserIdAsync(userId, startDate, endDate);
			return _mapper.Map<IEnumerable<ExpenseDto>>(expenses);
		}

		public async Task<ExpenseDto> CreateExpenseAsync(Guid userId, CreateExpenseDto dto)
		{
			var validationResult = await _createValidator.ValidateAsync(dto);
			if (!validationResult.IsValid)
			{
				_logger.LogWarning("Expense validation failed for user {UserId}: {Errors}", userId, validationResult.Errors);
				throw new ValidationException(validationResult.Errors);
			}

			// Verify account belongs to user
			var account = await _unitOfWork.Accounts.GetByIdAsync(dto.AccountId);
			if (account == null || account.UserId != userId)
			{
				throw new ArgumentException("Invalid account.");
			}

			var expense = new Expense
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				AccountId = dto.AccountId,
				Amount = dto.Amount,
				Category = Enum.Parse<ExpenseCategory>(dto.Category, true),
				Description = dto.Description,
				Date = dto.Date,
				CreatedAt = DateTime.UtcNow
			};

			// Deduct from account balance
			account.CurrentBalance -= dto.Amount;
			account.UpdatedAt = DateTime.UtcNow;
			_unitOfWork.Accounts.Update(account);

			// Create transaction record
			var transaction = new Transactions
			{
				Id = Guid.NewGuid(),
				AccountId = dto.AccountId,
				Amount = dto.Amount,
				TransactionType = TransactionType.Withdrawal,
				Category = TransactionCategory.Expense,
				Description = $"Expense: {dto.Category} - {dto.Description}",
				Date = dto.Date,
				CreatedAt = DateTime.UtcNow
			};

			await _unitOfWork.Expenses.AddAsync(expense);
			await _unitOfWork.Transactions.AddAsync(transaction);
			await _unitOfWork.SaveChangesAsync();

			_logger.LogInformation("Created expense {ExpenseId} ({Category}, {Amount}) on account {AccountId} for user {UserId}",
				expense.Id, dto.Category, dto.Amount, dto.AccountId, userId);

			// Reload with navigation properties for mapping
			expense.Account = account;
			return _mapper.Map<ExpenseDto>(expense);
		}

		public async Task<ExpenseSummaryDto> GetMonthlySummaryAsync(Guid userId, int year, int month)
		{
			var byCategory = await _unitOfWork.Expenses.GetMonthlySummaryByCategoryAsync(userId, year, month);
			var byAccount = await _unitOfWork.Expenses.GetMonthlySummaryByAccountAsync(userId, year, month);

			var accounts = await _unitOfWork.Accounts.GetByUserIdAsync(userId);
			var accountNames = accounts.ToDictionary(a => a.Id.ToString(), a => a.Name);

			return new ExpenseSummaryDto(
				TotalAmount: byCategory.Values.Sum(),
				ByCategory: byCategory.ToDictionary(x => x.Key.ToString(), x => x.Value),
				ByAccount: byAccount.ToDictionary(x => accountNames.GetValueOrDefault(x.Key.ToString(), "Unknown"), x => x.Value)
			);
		}

		public async Task DeleteExpenseAsync(Guid userId, Guid expenseId)
		{
			var expense = await _unitOfWork.Expenses.GetByIdAsync(expenseId);
			if (expense == null || expense.UserId != userId)
			{
				_logger.LogWarning("Expense {ExpenseId} not found for user {UserId}", expenseId, userId);
				throw new ArgumentException("Expense not found.");
			}

			// Refund the account
			var account = await _unitOfWork.Accounts.GetByIdAsync(expense.AccountId);
			if (account != null)
			{
				account.CurrentBalance += expense.Amount;
				account.UpdatedAt = DateTime.UtcNow;
				_unitOfWork.Accounts.Update(account);
			}

			_unitOfWork.Expenses.Delete(expense);
			await _unitOfWork.SaveChangesAsync();

			_logger.LogInformation("Deleted expense {ExpenseId}, refunded {Amount} for user {UserId}", expenseId, expense.Amount, userId);
		}
	}
}
