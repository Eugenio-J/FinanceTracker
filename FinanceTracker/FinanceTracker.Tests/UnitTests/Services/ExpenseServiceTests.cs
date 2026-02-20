using AutoMapper;
using FinanceTracker.Application.DTOs.Expense;
using FinanceTracker.Application.Mappings;
using FinanceTracker.Application.Services;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Interfaces;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinanceTracker.Tests.UnitTests.Services;

public class ExpenseServiceTests
{
	private readonly Mock<IUnitOfWork> _unitOfWork;
	private readonly IMapper _mapper;
	private readonly Mock<IValidator<CreateExpenseDto>> _validator;
	private readonly ExpenseService _sut;
	private readonly Guid _userId = Guid.NewGuid();

	public ExpenseServiceTests()
	{
		_unitOfWork = new Mock<IUnitOfWork>();
		var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
		_mapper = config.CreateMapper();
		_validator = new Mock<IValidator<CreateExpenseDto>>();
		_validator.Setup(v => v.ValidateAsync(It.IsAny<CreateExpenseDto>(), default))
			.ReturnsAsync(new ValidationResult());
		_sut = new ExpenseService(_unitOfWork.Object, _mapper, _validator.Object, NullLogger<ExpenseService>.Instance);
	}

	private Account CreateAccount(decimal balance = 1000m, Guid? id = null)
	{
		return new Account
		{
			Id = id ?? Guid.NewGuid(),
			UserId = _userId,
			Name = "Test Account",
			AccountType = AccountType.Hub,
			CurrentBalance = balance,
			CreatedAt = DateTime.UtcNow
		};
	}

	private Expense CreateExpense(Guid accountId, Account? account = null, decimal amount = 50m)
	{
		return new Expense
		{
			Id = Guid.NewGuid(),
			UserId = _userId,
			AccountId = accountId,
			Amount = amount,
			Category = ExpenseCategory.Food,
			Description = "Test expense",
			Date = DateTime.UtcNow,
			CreatedAt = DateTime.UtcNow,
			Account = account ?? new Account { Id = accountId, Name = "Test Account", UserId = _userId }
		};
	}

	[Fact]
	public async Task GetExpensesByUserIdAsync_ReturnsExpenses()
	{
		var account = CreateAccount();
		var expenses = new List<Expense>
		{
			CreateExpense(account.Id, account),
			CreateExpense(account.Id, account)
		};
		_unitOfWork.Setup(u => u.Expenses.GetByUserIdAsync(_userId, null, null)).ReturnsAsync(expenses);

		var result = await _sut.GetExpensesByUserIdAsync(_userId);

		result.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetExpensesByUserIdAsync_PassesDateFilters()
	{
		var start = new DateTime(2026, 1, 1);
		var end = new DateTime(2026, 1, 31);
		_unitOfWork.Setup(u => u.Expenses.GetByUserIdAsync(_userId, start, end))
			.ReturnsAsync(new List<Expense>());

		await _sut.GetExpensesByUserIdAsync(_userId, start, end);

		_unitOfWork.Verify(u => u.Expenses.GetByUserIdAsync(_userId, start, end), Times.Once);
	}

	[Fact]
	public async Task GetExpensesByUserIdAsync_ReturnsEmpty()
	{
		_unitOfWork.Setup(u => u.Expenses.GetByUserIdAsync(_userId, null, null))
			.ReturnsAsync(new List<Expense>());

		var result = await _sut.GetExpensesByUserIdAsync(_userId);

		result.Should().BeEmpty();
	}

	[Fact]
	public async Task CreateExpenseAsync_DeductsBalanceAndCreatesTransaction()
	{
		var account = CreateAccount(balance: 1000m);
		var dto = new CreateExpenseDto(account.Id, 200m, "Food", "Lunch", DateTime.UtcNow);

		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);
		_unitOfWork.Setup(u => u.Expenses.AddAsync(It.IsAny<Expense>()))
			.ReturnsAsync((Expense e) => e);
		_unitOfWork.Setup(u => u.Transactions.AddAsync(It.IsAny<Transactions>()))
			.ReturnsAsync((Transactions t) => t);

		var result = await _sut.CreateExpenseAsync(_userId, dto);

		account.CurrentBalance.Should().Be(800m);
		result.Amount.Should().Be(200m);
		_unitOfWork.Verify(u => u.Transactions.AddAsync(It.Is<Transactions>(
			t => t.TransactionType == TransactionType.Withdrawal && t.Amount == 200m)), Times.Once);
		_unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task CreateExpenseAsync_ThrowsValidationException_WhenInvalid()
	{
		var dto = new CreateExpenseDto(Guid.NewGuid(), -1m, "Food", "Test", DateTime.UtcNow);
		_validator.Setup(v => v.ValidateAsync(dto, default))
			.ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Amount", "Must be positive") }));

		var act = () => _sut.CreateExpenseAsync(_userId, dto);

		await act.Should().ThrowAsync<ValidationException>();
	}

	[Fact]
	public async Task CreateExpenseAsync_ThrowsArgumentException_WhenAccountNotFound()
	{
		var dto = new CreateExpenseDto(Guid.NewGuid(), 100m, "Food", "Test", DateTime.UtcNow);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Account?)null);

		var act = () => _sut.CreateExpenseAsync(_userId, dto);

		await act.Should().ThrowAsync<ArgumentException>();
	}

	[Fact]
	public async Task CreateExpenseAsync_ThrowsArgumentException_WhenAccountNotOwned()
	{
		var account = CreateAccount();
		account.UserId = Guid.NewGuid();
		var dto = new CreateExpenseDto(account.Id, 100m, "Food", "Test", DateTime.UtcNow);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);

		var act = () => _sut.CreateExpenseAsync(_userId, dto);

		await act.Should().ThrowAsync<ArgumentException>();
	}

	[Fact]
	public async Task GetMonthlySummaryAsync_ReturnsAggregatedSummary()
	{
		var accountId = Guid.NewGuid();
		var byCategory = new Dictionary<ExpenseCategory, decimal>
		{
			{ ExpenseCategory.Food, 300m },
			{ ExpenseCategory.Transport, 150m }
		};
		var byAccount = new Dictionary<Guid, decimal>
		{
			{ accountId, 450m }
		};
		var accounts = new List<Account>
		{
			new Account { Id = accountId, UserId = _userId, Name = "Main", AccountType = AccountType.Hub }
		};

		_unitOfWork.Setup(u => u.Expenses.GetMonthlySummaryByCategoryAsync(_userId, 2026, 1)).ReturnsAsync(byCategory);
		_unitOfWork.Setup(u => u.Expenses.GetMonthlySummaryByAccountAsync(_userId, 2026, 1)).ReturnsAsync(byAccount);
		_unitOfWork.Setup(u => u.Accounts.GetByUserIdAsync(_userId)).ReturnsAsync(accounts);

		var result = await _sut.GetMonthlySummaryAsync(_userId, 2026, 1);

		result.TotalAmount.Should().Be(450m);
		result.ByCategory.Should().ContainKey("Food").WhoseValue.Should().Be(300m);
		result.ByAccount.Should().ContainKey("Main").WhoseValue.Should().Be(450m);
	}

	[Fact]
	public async Task GetMonthlySummaryAsync_ReturnsZero_WhenNoExpenses()
	{
		_unitOfWork.Setup(u => u.Expenses.GetMonthlySummaryByCategoryAsync(_userId, 2026, 1))
			.ReturnsAsync(new Dictionary<ExpenseCategory, decimal>());
		_unitOfWork.Setup(u => u.Expenses.GetMonthlySummaryByAccountAsync(_userId, 2026, 1))
			.ReturnsAsync(new Dictionary<Guid, decimal>());
		_unitOfWork.Setup(u => u.Accounts.GetByUserIdAsync(_userId))
			.ReturnsAsync(new List<Account>());

		var result = await _sut.GetMonthlySummaryAsync(_userId, 2026, 1);

		result.TotalAmount.Should().Be(0m);
		result.ByCategory.Should().BeEmpty();
		result.ByAccount.Should().BeEmpty();
	}

	[Fact]
	public async Task DeleteExpenseAsync_RefundsBalanceAndDeletes()
	{
		var account = CreateAccount(balance: 800m);
		var expense = CreateExpense(account.Id, account, amount: 200m);

		_unitOfWork.Setup(u => u.Expenses.GetByIdAsync(expense.Id)).ReturnsAsync(expense);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);

		await _sut.DeleteExpenseAsync(_userId, expense.Id);

		account.CurrentBalance.Should().Be(1000m);
		_unitOfWork.Verify(u => u.Expenses.Delete(expense), Times.Once);
		_unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task DeleteExpenseAsync_ThrowsArgumentException_WhenNotFound()
	{
		_unitOfWork.Setup(u => u.Expenses.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Expense?)null);

		var act = () => _sut.DeleteExpenseAsync(_userId, Guid.NewGuid());

		await act.Should().ThrowAsync<ArgumentException>();
	}
}
