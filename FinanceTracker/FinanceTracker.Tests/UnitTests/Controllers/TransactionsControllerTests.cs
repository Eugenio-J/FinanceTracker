using FinanceTracker.API.Controllers;
using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.DTOs.Transaction;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinanceTracker.Tests.UnitTests.Controllers;

public class TransactionsControllerTests
{
	private readonly Mock<ITransactionService> _transactionService;
	private readonly TransactionsController _sut;
	private readonly Guid _userId = Guid.NewGuid();

	public TransactionsControllerTests()
	{
		_transactionService = new Mock<ITransactionService>();
		_sut = new TransactionsController(_transactionService.Object, NullLogger<TransactionsController>.Instance);
		TestHelpers.SetupControllerContext(_sut, _userId);
	}

	[Fact]
	public async Task GetByUserId_ReturnsOkWithPagedResult()
	{
		var filter = new TransactionFilterDto();
		var pagedResult = new TransactionPagedResultDto(
			new List<TransactionDto>(), 0, 1, 20, 0);
		_transactionService.Setup(s => s.GetByUserIdAsync(_userId, filter)).ReturnsAsync(pagedResult);

		var result = await _sut.GetByUserId(filter);

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<TransactionPagedResultDto>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task GetById_ReturnsOk_WhenFound()
	{
		var id = Guid.NewGuid();
		var transaction = new TransactionDto(id, Guid.NewGuid(), "Hub", 100m, "Deposit", "Salary", "Test", DateTime.UtcNow, null, DateTime.UtcNow);
		_transactionService.Setup(s => s.GetByIdAsync(_userId, id)).ReturnsAsync(transaction);

		var result = await _sut.GetById(id);

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<TransactionDto>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data!.Id.Should().Be(id);
	}

	[Fact]
	public async Task GetById_ReturnsNotFound_WhenNull()
	{
		_transactionService.Setup(s => s.GetByIdAsync(_userId, It.IsAny<Guid>()))
			.ReturnsAsync((TransactionDto?)null);

		var result = await _sut.GetById(Guid.NewGuid());

		result.Should().BeOfType<NotFoundResult>();
	}

	[Fact]
	public async Task GetByAccountId_ReturnsOkWithTransactions()
	{
		var accountId = Guid.NewGuid();
		var transactions = new List<TransactionDto>
		{
			new TransactionDto(Guid.NewGuid(), accountId, "Hub", 100m, "Deposit", "Salary", "Test", DateTime.UtcNow, null, DateTime.UtcNow)
		};
		_transactionService.Setup(s => s.GetByAccountIdAsync(_userId, accountId, 20)).ReturnsAsync(transactions);

		var result = await _sut.GetByAccountId(accountId);

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<IEnumerable<TransactionDto>>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data.Should().HaveCount(1);
	}

	[Fact]
	public async Task Create_ReturnsCreatedWithTransaction()
	{
		var dto = new CreateTransactionDto(Guid.NewGuid(), 100m, "Deposit", "Salary", "Test", DateTime.UtcNow);
		var created = new TransactionDto(Guid.NewGuid(), dto.AccountId, "Hub", 100m, "Deposit", "Salary", "Test", DateTime.UtcNow, null, DateTime.UtcNow);
		_transactionService.Setup(s => s.CreateAsync(_userId, dto)).ReturnsAsync(created);

		var result = await _sut.Create(dto);

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(201);
		var wrapper = objectResult.Value.Should().BeOfType<Result<TransactionDto>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task Delete_ReturnsNoContent_WhenTrue()
	{
		var id = Guid.NewGuid();
		_transactionService.Setup(s => s.DeleteAsync(_userId, id)).ReturnsAsync(true);

		var result = await _sut.Delete(id);

		result.Should().BeOfType<NoContentResult>();
	}

	[Fact]
	public async Task Delete_ReturnsNotFound_WhenFalse()
	{
		var id = Guid.NewGuid();
		_transactionService.Setup(s => s.DeleteAsync(_userId, id)).ReturnsAsync(false);

		var result = await _sut.Delete(id);

		result.Should().BeOfType<NotFoundResult>();
	}
}
