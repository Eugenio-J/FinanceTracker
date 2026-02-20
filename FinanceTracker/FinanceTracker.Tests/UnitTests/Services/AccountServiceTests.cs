using AutoMapper;
using FinanceTracker.Application.DTOs.Account;
using FinanceTracker.Application.Mappings;
using FinanceTracker.Application.Services;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinanceTracker.Tests.UnitTests.Services;

public class AccountServiceTests
{
	private readonly Mock<IUnitOfWork> _unitOfWork;
	private readonly IMapper _mapper;
	private readonly AccountService _sut;
	private readonly Guid _userId = Guid.NewGuid();

	public AccountServiceTests()
	{
		_unitOfWork = new Mock<IUnitOfWork>();
		var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
		_mapper = config.CreateMapper();
		_sut = new AccountService(_unitOfWork.Object, _mapper, NullLogger<AccountService>.Instance);
	}

	private Account CreateAccount(AccountType type = AccountType.Hub, decimal balance = 1000m, Guid? id = null)
	{
		return new Account
		{
			Id = id ?? Guid.NewGuid(),
			UserId = _userId,
			Name = $"Test {type}",
			AccountType = type,
			CurrentBalance = balance,
			CreatedAt = DateTime.UtcNow,
			Transactions = new List<Transactions>()
		};
	}

	[Fact]
	public async Task GetAllByUserIdAsync_ReturnsAccounts()
	{
		var accounts = new List<Account> { CreateAccount(), CreateAccount(AccountType.Savings) };
		_unitOfWork.Setup(u => u.Accounts.GetByUserIdAsync(_userId)).ReturnsAsync(accounts);

		var result = await _sut.GetAllByUserIdAsync(_userId);

		result.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsAccount_WhenOwner()
	{
		var account = CreateAccount();
		_unitOfWork.Setup(u => u.Accounts.GetByIdWithTransactionsAsync(account.Id)).ReturnsAsync(account);

		var result = await _sut.GetByIdAsync(_userId, account.Id);

		result.Should().NotBeNull();
		result!.Id.Should().Be(account.Id);
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsNull_WhenNotOwner()
	{
		var account = CreateAccount();
		account.UserId = Guid.NewGuid(); // different user
		_unitOfWork.Setup(u => u.Accounts.GetByIdWithTransactionsAsync(account.Id)).ReturnsAsync(account);

		var result = await _sut.GetByIdAsync(_userId, account.Id);

		result.Should().BeNull();
	}

	[Fact]
	public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
	{
		_unitOfWork.Setup(u => u.Accounts.GetByIdWithTransactionsAsync(It.IsAny<Guid>())).ReturnsAsync((Account?)null);

		var result = await _sut.GetByIdAsync(_userId, Guid.NewGuid());

		result.Should().BeNull();
	}

	[Fact]
	public async Task CreateAsync_CreatesAccountWithInitialDeposit()
	{
		var dto = new CreateAccountDto("Savings", "Savings", 500m);
		_unitOfWork.Setup(u => u.Accounts.AddAsync(It.IsAny<Account>()))
			.ReturnsAsync((Account a) => a);
		_unitOfWork.Setup(u => u.Transactions.AddAsync(It.IsAny<Transactions>()))
			.ReturnsAsync((Transactions t) => t);

		var result = await _sut.CreateAsync(_userId, dto);

		result.Name.Should().Be("Savings");
		result.CurrentBalance.Should().Be(500m);
		_unitOfWork.Verify(u => u.Transactions.AddAsync(It.Is<Transactions>(
			t => t.Amount == 500m && t.TransactionType == TransactionType.Deposit)), Times.Once);
		_unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task CreateAsync_NoTransaction_WhenZeroBalance()
	{
		var dto = new CreateAccountDto("Hub", "Hub", 0m);
		_unitOfWork.Setup(u => u.Accounts.AddAsync(It.IsAny<Account>()))
			.ReturnsAsync((Account a) => a);

		var result = await _sut.CreateAsync(_userId, dto);

		result.CurrentBalance.Should().Be(0m);
		_unitOfWork.Verify(u => u.Transactions.AddAsync(It.IsAny<Transactions>()), Times.Never);
	}

	[Fact]
	public async Task CreateAsync_ThrowsOnInvalidAccountType()
	{
		var dto = new CreateAccountDto("Test", "InvalidType", 0m);

		var act = () => _sut.CreateAsync(_userId, dto);

		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("*Invalid account type*");
	}

	[Fact]
	public async Task UpdateAsync_UpdatesName()
	{
		var account = CreateAccount();
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);

		var result = await _sut.UpdateAsync(_userId, account.Id, new UpdateAccountDto("New Name"));

		result.Name.Should().Be("New Name");
		_unitOfWork.Verify(u => u.Accounts.Update(account), Times.Once);
		_unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task UpdateAsync_ThrowsOnNotFound()
	{
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Account?)null);

		var act = () => _sut.UpdateAsync(_userId, Guid.NewGuid(), new UpdateAccountDto("Name"));

		await act.Should().ThrowAsync<ArgumentException>();
	}

	[Fact]
	public async Task UpdateAsync_ThrowsOnNotOwner()
	{
		var account = CreateAccount();
		account.UserId = Guid.NewGuid();
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);

		var act = () => _sut.UpdateAsync(_userId, account.Id, new UpdateAccountDto("Name"));

		await act.Should().ThrowAsync<ArgumentException>();
	}

	[Fact]
	public async Task DeleteAsync_ReturnsTrue_WhenOwner()
	{
		var account = CreateAccount();
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);

		var result = await _sut.DeleteAsync(_userId, account.Id);

		result.Should().BeTrue();
		_unitOfWork.Verify(u => u.Accounts.Delete(account), Times.Once);
		_unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
	{
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Account?)null);

		var result = await _sut.DeleteAsync(_userId, Guid.NewGuid());

		result.Should().BeFalse();
	}

	[Fact]
	public async Task DeleteAsync_ReturnsFalse_WhenNotOwner()
	{
		var account = CreateAccount();
		account.UserId = Guid.NewGuid();
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);

		var result = await _sut.DeleteAsync(_userId, account.Id);

		result.Should().BeFalse();
	}

	[Fact]
	public async Task GetTotalBalanceAsync_DelegatesToRepository()
	{
		_unitOfWork.Setup(u => u.Accounts.GetTotalBalanceByUserIdAsync(_userId)).ReturnsAsync(5000m);

		var result = await _sut.GetTotalBalanceAsync(_userId);

		result.Should().Be(5000m);
	}
}
