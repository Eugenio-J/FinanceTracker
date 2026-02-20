using AutoMapper;
using FinanceTracker.Application.DTOs.Transaction;
using FinanceTracker.Application.Mappings;
using FinanceTracker.Application.Services;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinanceTracker.Tests.UnitTests.Services;

public class TransactionServiceTests
{
	private readonly Mock<IUnitOfWork> _unitOfWork;
	private readonly IMapper _mapper;
	private readonly TransactionService _sut;
	private readonly Guid _userId = Guid.NewGuid();

	public TransactionServiceTests()
	{
		_unitOfWork = new Mock<IUnitOfWork>();
		var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
		_mapper = config.CreateMapper();
		_sut = new TransactionService(_unitOfWork.Object, _mapper, NullLogger<TransactionService>.Instance);
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
			CreatedAt = DateTime.UtcNow,
			Transactions = new List<Transactions>()
		};
	}

	private Transactions CreateTransaction(Guid accountId, TransactionType type = TransactionType.Deposit,
		decimal amount = 100m, Account? account = null)
	{
		return new Transactions
		{
			Id = Guid.NewGuid(),
			AccountId = accountId,
			Amount = amount,
			TransactionType = type,
			Category = TransactionCategory.Salary,
			Description = "Test transaction",
			Date = DateTime.UtcNow,
			CreatedAt = DateTime.UtcNow,
			Account = account ?? new Account { Id = accountId, Name = "Test Account", UserId = _userId }
		};
	}

	[Fact]
	public async Task GetByUserIdAsync_ReturnsPagedResult_WithDefaultFilter()
	{
		var account = CreateAccount();
		var transactions = new List<Transactions>
		{
			CreateTransaction(account.Id, account: account),
			CreateTransaction(account.Id, account: account)
		};
		var filter = new TransactionFilterDto();

		_unitOfWork.Setup(u => u.Transactions.GetFilteredAsync(
			_userId, null, null, null, null, null, 1, 20)).ReturnsAsync(transactions);
		_unitOfWork.Setup(u => u.Transactions.GetCountAsync(
			_userId, null, null, null, null, null)).ReturnsAsync(2);

		var result = await _sut.GetByUserIdAsync(_userId, filter);

		result.TotalCount.Should().Be(2);
		result.Items.Should().HaveCount(2);
		result.PageNumber.Should().Be(1);
		result.PageSize.Should().Be(20);
	}

	[Fact]
	public async Task GetByUserIdAsync_ParsesEnumFilters()
	{
		var filter = new TransactionFilterDto(TransactionType: "Deposit", Category: "Salary");

		_unitOfWork.Setup(u => u.Transactions.GetFilteredAsync(
			_userId, null, TransactionType.Deposit, TransactionCategory.Salary, null, null, 1, 20))
			.ReturnsAsync(new List<Transactions>());
		_unitOfWork.Setup(u => u.Transactions.GetCountAsync(
			_userId, null, TransactionType.Deposit, TransactionCategory.Salary, null, null))
			.ReturnsAsync(0);

		var result = await _sut.GetByUserIdAsync(_userId, filter);

		result.TotalCount.Should().Be(0);
		_unitOfWork.Verify(u => u.Transactions.GetFilteredAsync(
			_userId, null, TransactionType.Deposit, TransactionCategory.Salary, null, null, 1, 20), Times.Once);
	}

	[Fact]
	public async Task GetByUserIdAsync_ReturnsEmpty_WhenNoTransactions()
	{
		var filter = new TransactionFilterDto();
		_unitOfWork.Setup(u => u.Transactions.GetFilteredAsync(
			_userId, null, null, null, null, null, 1, 20)).ReturnsAsync(new List<Transactions>());
		_unitOfWork.Setup(u => u.Transactions.GetCountAsync(
			_userId, null, null, null, null, null)).ReturnsAsync(0);

		var result = await _sut.GetByUserIdAsync(_userId, filter);

		result.Items.Should().BeEmpty();
		result.TotalCount.Should().Be(0);
		result.TotalPages.Should().Be(0);
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsTransaction_WhenFoundAndOwned()
	{
		var account = CreateAccount();
		var transaction = CreateTransaction(account.Id);

		_unitOfWork.Setup(u => u.Transactions.GetByIdAsync(transaction.Id)).ReturnsAsync(transaction);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);

		var result = await _sut.GetByIdAsync(_userId, transaction.Id);

		result.Should().NotBeNull();
		result!.Id.Should().Be(transaction.Id);
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
	{
		_unitOfWork.Setup(u => u.Transactions.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Transactions?)null);

		var result = await _sut.GetByIdAsync(_userId, Guid.NewGuid());

		result.Should().BeNull();
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsNull_WhenNotOwned()
	{
		var account = CreateAccount();
		account.UserId = Guid.NewGuid(); // different user
		var transaction = CreateTransaction(account.Id);

		_unitOfWork.Setup(u => u.Transactions.GetByIdAsync(transaction.Id)).ReturnsAsync(transaction);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);

		var result = await _sut.GetByIdAsync(_userId, transaction.Id);

		result.Should().BeNull();
	}

	[Fact]
	public async Task GetByAccountIdAsync_ReturnsTransactions()
	{
		var account = CreateAccount();
		var transactions = new List<Transactions>
		{
			CreateTransaction(account.Id, account: account),
			CreateTransaction(account.Id, account: account)
		};

		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);
		_unitOfWork.Setup(u => u.Transactions.GetByAccountIdAsync(account.Id, 20)).ReturnsAsync(transactions);

		var result = await _sut.GetByAccountIdAsync(_userId, account.Id);

		result.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetByAccountIdAsync_ThrowsArgumentException_WhenAccountNotFound()
	{
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Account?)null);

		var act = () => _sut.GetByAccountIdAsync(_userId, Guid.NewGuid());

		await act.Should().ThrowAsync<ArgumentException>();
	}

	[Fact]
	public async Task GetByAccountIdAsync_ThrowsArgumentException_WhenNotOwned()
	{
		var account = CreateAccount();
		account.UserId = Guid.NewGuid();
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);

		var act = () => _sut.GetByAccountIdAsync(_userId, account.Id);

		await act.Should().ThrowAsync<ArgumentException>();
	}

	[Fact]
	public async Task CreateAsync_DepositIncreasesBalance()
	{
		var account = CreateAccount(balance: 1000m);
		var dto = new CreateTransactionDto(account.Id, 500m, "Deposit", "Salary", "Test", DateTime.UtcNow);

		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);
		_unitOfWork.Setup(u => u.Transactions.AddAsync(It.IsAny<Transactions>()))
			.ReturnsAsync((Transactions t) => t);

		var result = await _sut.CreateAsync(_userId, dto);

		account.CurrentBalance.Should().Be(1500m);
		result.Amount.Should().Be(500m);
		_unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task CreateAsync_WithdrawalDecreasesBalance()
	{
		var account = CreateAccount(balance: 1000m);
		var dto = new CreateTransactionDto(account.Id, 300m, "Withdrawal", "Expense", "Test", DateTime.UtcNow);

		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);
		_unitOfWork.Setup(u => u.Transactions.AddAsync(It.IsAny<Transactions>()))
			.ReturnsAsync((Transactions t) => t);

		await _sut.CreateAsync(_userId, dto);

		account.CurrentBalance.Should().Be(700m);
	}

	[Fact]
	public async Task CreateAsync_ThrowsOnInvalidTransactionType()
	{
		var account = CreateAccount();
		var dto = new CreateTransactionDto(account.Id, 100m, "InvalidType", "Salary", "Test", DateTime.UtcNow);

		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);

		var act = () => _sut.CreateAsync(_userId, dto);

		await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Invalid transaction type*");
	}

	[Fact]
	public async Task CreateAsync_ThrowsOnAccountNotFound()
	{
		var dto = new CreateTransactionDto(Guid.NewGuid(), 100m, "Deposit", "Salary", "Test", DateTime.UtcNow);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Account?)null);

		var act = () => _sut.CreateAsync(_userId, dto);

		await act.Should().ThrowAsync<ArgumentException>();
	}

	[Fact]
	public async Task DeleteAsync_ReversesDepositAndReturnsTrue()
	{
		var account = CreateAccount(balance: 1500m);
		var transaction = CreateTransaction(account.Id, TransactionType.Deposit, 500m);

		_unitOfWork.Setup(u => u.Transactions.GetByIdAsync(transaction.Id)).ReturnsAsync(transaction);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);

		var result = await _sut.DeleteAsync(_userId, transaction.Id);

		result.Should().BeTrue();
		account.CurrentBalance.Should().Be(1000m);
		_unitOfWork.Verify(u => u.Transactions.Delete(transaction), Times.Once);
	}

	[Fact]
	public async Task DeleteAsync_ReversesWithdrawalAndReturnsTrue()
	{
		var account = CreateAccount(balance: 700m);
		var transaction = CreateTransaction(account.Id, TransactionType.Withdrawal, 300m);

		_unitOfWork.Setup(u => u.Transactions.GetByIdAsync(transaction.Id)).ReturnsAsync(transaction);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);

		var result = await _sut.DeleteAsync(_userId, transaction.Id);

		result.Should().BeTrue();
		account.CurrentBalance.Should().Be(1000m);
	}

	[Fact]
	public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
	{
		_unitOfWork.Setup(u => u.Transactions.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Transactions?)null);

		var result = await _sut.DeleteAsync(_userId, Guid.NewGuid());

		result.Should().BeFalse();
	}

	[Fact]
	public async Task DeleteAsync_ReturnsFalse_WhenNotOwned()
	{
		var account = CreateAccount();
		account.UserId = Guid.NewGuid();
		var transaction = CreateTransaction(account.Id);

		_unitOfWork.Setup(u => u.Transactions.GetByIdAsync(transaction.Id)).ReturnsAsync(transaction);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);

		var result = await _sut.DeleteAsync(_userId, transaction.Id);

		result.Should().BeFalse();
	}
}
