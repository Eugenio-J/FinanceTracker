using AutoMapper;
using FinanceTracker.Application.DTOs.SalaryCycle;
using FinanceTracker.Application.Mappings;
using FinanceTracker.Application.Services;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinanceTracker.Tests.UnitTests.Services;

public class SalaryCycleServiceTests
{
	private readonly Mock<IUnitOfWork> _unitOfWork;
	private readonly IMapper _mapper;
	private readonly SalaryCycleService _sut;
	private readonly Guid _userId = Guid.NewGuid();

	public SalaryCycleServiceTests()
	{
		_unitOfWork = new Mock<IUnitOfWork>();
		var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
		_mapper = config.CreateMapper();
		_sut = new SalaryCycleService(_unitOfWork.Object, _mapper, NullLogger<SalaryCycleService>.Instance);
	}

	private Account CreateAccount(decimal balance = 0m, Guid? id = null)
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

	private SalaryCycle CreateCycle(SalaryCycleStatus status = SalaryCycleStatus.Pending,
		List<SalaryDistribution>? distributions = null)
	{
		return new SalaryCycle
		{
			Id = Guid.NewGuid(),
			UserId = _userId,
			PayDate = new DateTime(2026, 2, 1),
			GrossSalary = 5000m,
			NetSalary = 4000m,
			Status = status,
			CreatedAt = DateTime.UtcNow,
			Distributions = distributions ?? new List<SalaryDistribution>()
		};
	}

	[Fact]
	public async Task GetRecentCyclesAsync_ReturnsMappedCycles()
	{
		var cycles = new List<SalaryCycle> { CreateCycle(), CreateCycle() };
		_unitOfWork.Setup(u => u.SalaryCycles.GetByUserIdAsync(_userId, 6)).ReturnsAsync(cycles);

		var result = await _sut.GetRecentCyclesAsync(_userId);

		result.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetRecentCyclesAsync_ReturnsEmpty()
	{
		_unitOfWork.Setup(u => u.SalaryCycles.GetByUserIdAsync(_userId, 6))
			.ReturnsAsync(new List<SalaryCycle>());

		var result = await _sut.GetRecentCyclesAsync(_userId);

		result.Should().BeEmpty();
	}

	[Fact]
	public async Task CreateSalaryCycleAsync_CreatesCycleWithDistributions()
	{
		var accountId = Guid.NewGuid();
		var dto = new CreateSalaryCycleDto(
			new DateTime(2026, 3, 1), 5000m, 4000m,
			new List<CreateDistributionDto>
			{
				new(accountId, 2000m, "Fixed", 0),
				new(accountId, 50m, "Percentage", 1)
			});

		_unitOfWork.Setup(u => u.SalaryCycles.AddAsync(It.IsAny<SalaryCycle>()))
			.ReturnsAsync((SalaryCycle c) => c);

		var result = await _sut.CreateSalaryCycleAsync(_userId, dto);

		result.Distributions.Should().HaveCount(2);
		result.Status.Should().Be("Pending");
		_unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
	}

	[Fact]
	public async Task CreateSalaryCycleAsync_ThrowsOnInvalidDistributionType()
	{
		var dto = new CreateSalaryCycleDto(
			new DateTime(2026, 3, 1), 5000m, 4000m,
			new List<CreateDistributionDto>
			{
				new(Guid.NewGuid(), 2000m, "InvalidType", 0)
			});

		var act = () => _sut.CreateSalaryCycleAsync(_userId, dto);

		await act.Should().ThrowAsync<ArgumentException>();
	}

	[Fact]
	public async Task ExecuteDistributionsAsync_DistributesFixedAmounts()
	{
		var account = CreateAccount(balance: 0m);
		var cycle = CreateCycle(distributions: new List<SalaryDistribution>
		{
			new SalaryDistribution
			{
				Id = Guid.NewGuid(),
				TargetAccountId = account.Id,
				Amount = 1000m,
				DistributionType = DistributionType.Fixed,
				OrderIndex = 0,
				TargetAccount = account
			}
		});

		_unitOfWork.Setup(u => u.SalaryCycles.GetByIdWithDistributionsAsync(cycle.Id)).ReturnsAsync(cycle);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);
		_unitOfWork.Setup(u => u.Transactions.AddAsync(It.IsAny<Transactions>()))
			.ReturnsAsync((Transactions t) => t);

		var result = await _sut.ExecuteDistributionsAsync(_userId, cycle.Id);

		account.CurrentBalance.Should().Be(1000m);
		result.Status.Should().Be("Completed");
		_unitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
	}

	[Fact]
	public async Task ExecuteDistributionsAsync_DistributesPercentageAmounts()
	{
		var account = CreateAccount(balance: 0m);
		var cycle = CreateCycle(distributions: new List<SalaryDistribution>
		{
			new SalaryDistribution
			{
				Id = Guid.NewGuid(),
				TargetAccountId = account.Id,
				Amount = 25m, // 25% of 4000 = 1000
				DistributionType = DistributionType.Percentage,
				OrderIndex = 0,
				TargetAccount = account
			}
		});

		_unitOfWork.Setup(u => u.SalaryCycles.GetByIdWithDistributionsAsync(cycle.Id)).ReturnsAsync(cycle);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);
		_unitOfWork.Setup(u => u.Transactions.AddAsync(It.IsAny<Transactions>()))
			.ReturnsAsync((Transactions t) => t);

		await _sut.ExecuteDistributionsAsync(_userId, cycle.Id);

		account.CurrentBalance.Should().Be(1000m);
	}

	[Fact]
	public async Task ExecuteDistributionsAsync_DistributesRemainder()
	{
		var account1 = CreateAccount(balance: 0m);
		var account2 = CreateAccount(balance: 0m);
		var cycle = CreateCycle(distributions: new List<SalaryDistribution>
		{
			new SalaryDistribution
			{
				Id = Guid.NewGuid(),
				TargetAccountId = account1.Id,
				Amount = 1000m,
				DistributionType = DistributionType.Fixed,
				OrderIndex = 0,
				TargetAccount = account1
			},
			new SalaryDistribution
			{
				Id = Guid.NewGuid(),
				TargetAccountId = account2.Id,
				Amount = 0m,
				DistributionType = DistributionType.Remainder,
				OrderIndex = 1,
				TargetAccount = account2
			}
		});

		_unitOfWork.Setup(u => u.SalaryCycles.GetByIdWithDistributionsAsync(cycle.Id)).ReturnsAsync(cycle);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account1.Id)).ReturnsAsync(account1);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account2.Id)).ReturnsAsync(account2);
		_unitOfWork.Setup(u => u.Transactions.AddAsync(It.IsAny<Transactions>()))
			.ReturnsAsync((Transactions t) => t);

		await _sut.ExecuteDistributionsAsync(_userId, cycle.Id);

		account1.CurrentBalance.Should().Be(1000m);
		account2.CurrentBalance.Should().Be(3000m); // 4000 - 1000
	}

	[Fact]
	public async Task ExecuteDistributionsAsync_CapsFixedAtRemainingSalary()
	{
		var account = CreateAccount(balance: 0m);
		var cycle = CreateCycle(distributions: new List<SalaryDistribution>
		{
			new SalaryDistribution
			{
				Id = Guid.NewGuid(),
				TargetAccountId = account.Id,
				Amount = 5000m, // more than net salary of 4000
				DistributionType = DistributionType.Fixed,
				OrderIndex = 0,
				TargetAccount = account
			}
		});

		_unitOfWork.Setup(u => u.SalaryCycles.GetByIdWithDistributionsAsync(cycle.Id)).ReturnsAsync(cycle);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(account.Id)).ReturnsAsync(account);
		_unitOfWork.Setup(u => u.Transactions.AddAsync(It.IsAny<Transactions>()))
			.ReturnsAsync((Transactions t) => t);

		await _sut.ExecuteDistributionsAsync(_userId, cycle.Id);

		account.CurrentBalance.Should().Be(4000m); // capped at net salary
	}

	[Fact]
	public async Task ExecuteDistributionsAsync_SetsCycleToCompleted()
	{
		var cycle = CreateCycle(distributions: new List<SalaryDistribution>());

		_unitOfWork.Setup(u => u.SalaryCycles.GetByIdWithDistributionsAsync(cycle.Id)).ReturnsAsync(cycle);

		var result = await _sut.ExecuteDistributionsAsync(_userId, cycle.Id);

		result.Status.Should().Be("Completed");
		cycle.CompletedAt.Should().NotBeNull();
	}

	[Fact]
	public async Task ExecuteDistributionsAsync_ThrowsInvalidOperationException_WhenAlreadyCompleted()
	{
		var cycle = CreateCycle(status: SalaryCycleStatus.Completed);

		_unitOfWork.Setup(u => u.SalaryCycles.GetByIdWithDistributionsAsync(cycle.Id)).ReturnsAsync(cycle);

		var act = () => _sut.ExecuteDistributionsAsync(_userId, cycle.Id);

		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*already completed*");
		_unitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
	}

	[Fact]
	public async Task ExecuteDistributionsAsync_ThrowsArgumentException_WhenNotFound()
	{
		_unitOfWork.Setup(u => u.SalaryCycles.GetByIdWithDistributionsAsync(It.IsAny<Guid>()))
			.ReturnsAsync((SalaryCycle?)null);

		var act = () => _sut.ExecuteDistributionsAsync(_userId, Guid.NewGuid());

		await act.Should().ThrowAsync<ArgumentException>();
		_unitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
	}

	[Fact]
	public async Task ExecuteDistributionsAsync_RollsBackOnError()
	{
		var cycle = CreateCycle(distributions: new List<SalaryDistribution>
		{
			new SalaryDistribution
			{
				Id = Guid.NewGuid(),
				TargetAccountId = Guid.NewGuid(),
				Amount = 1000m,
				DistributionType = DistributionType.Fixed,
				OrderIndex = 0,
				TargetAccount = new Account { Name = "Test" }
			}
		});

		_unitOfWork.Setup(u => u.SalaryCycles.GetByIdWithDistributionsAsync(cycle.Id)).ReturnsAsync(cycle);
		_unitOfWork.Setup(u => u.Accounts.GetByIdAsync(It.IsAny<Guid>()))
			.ThrowsAsync(new Exception("DB error"));

		var act = () => _sut.ExecuteDistributionsAsync(_userId, cycle.Id);

		await act.Should().ThrowAsync<Exception>();
		_unitOfWork.Verify(u => u.RollbackTransactionAsync(), Times.Once);
	}

	[Fact]
	public async Task GetNextPayDateAsync_ReturnsPayDatePlus14Days()
	{
		var payDate = new DateTime(2026, 2, 1);
		var cycle = new SalaryCycle
		{
			Id = Guid.NewGuid(),
			UserId = _userId,
			PayDate = payDate,
			GrossSalary = 5000m,
			NetSalary = 4000m,
			Status = SalaryCycleStatus.Completed,
			CreatedAt = DateTime.UtcNow
		};

		_unitOfWork.Setup(u => u.SalaryCycles.GetLatestByUserIdAsync(_userId)).ReturnsAsync(cycle);

		var result = await _sut.GetNextPayDateAsync(_userId);

		result.Should().Be(payDate.AddDays(14));
	}

	[Fact]
	public async Task GetNextPayDateAsync_ReturnsNull_WhenNoCycles()
	{
		_unitOfWork.Setup(u => u.SalaryCycles.GetLatestByUserIdAsync(_userId))
			.ReturnsAsync((SalaryCycle?)null);

		var result = await _sut.GetNextPayDateAsync(_userId);

		result.Should().BeNull();
	}
}
