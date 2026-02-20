using FinanceTracker.API.Controllers;
using FinanceTracker.Application.DTOs.Account;
using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinanceTracker.Tests.UnitTests.Controllers;

public class AccountsControllerTests
{
	private readonly Mock<IAccountService> _accountService;
	private readonly AccountsController _sut;
	private readonly Guid _userId = Guid.NewGuid();

	public AccountsControllerTests()
	{
		_accountService = new Mock<IAccountService>();
		_sut = new AccountsController(_accountService.Object, NullLogger<AccountsController>.Instance);
		TestHelpers.SetupControllerContext(_sut, _userId);
	}

	[Fact]
	public async Task GetAll_ReturnsOkWithAccounts()
	{
		var accounts = new List<AccountDto>
		{
			new AccountDto(Guid.NewGuid(), "Hub", "Hub", 1000m, DateTime.UtcNow, 0),
			new AccountDto(Guid.NewGuid(), "Savings", "Savings", 2000m, DateTime.UtcNow, 0)
		};
		_accountService.Setup(s => s.GetAllByUserIdAsync(_userId)).ReturnsAsync(accounts);

		var result = await _sut.GetAll();

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<IEnumerable<AccountDto>>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetById_ReturnsOk_WhenFound()
	{
		var id = Guid.NewGuid();
		var account = new AccountDto(id, "Hub", "Hub", 1000m, DateTime.UtcNow, 0);
		_accountService.Setup(s => s.GetByIdAsync(_userId, id)).ReturnsAsync(account);

		var result = await _sut.GetById(id);

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<AccountDto>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data!.Id.Should().Be(id);
	}

	[Fact]
	public async Task GetById_ReturnsNotFound_WhenNull()
	{
		_accountService.Setup(s => s.GetByIdAsync(_userId, It.IsAny<Guid>())).ReturnsAsync((AccountDto?)null);

		var result = await _sut.GetById(Guid.NewGuid());

		result.Should().BeOfType<NotFoundResult>();
	}

	[Fact]
	public async Task Create_ReturnsCreatedWithAccount()
	{
		var dto = new CreateAccountDto("New Account", "Hub", 500m);
		var created = new AccountDto(Guid.NewGuid(), "New Account", "Hub", 500m, DateTime.UtcNow, 0);
		_accountService.Setup(s => s.CreateAsync(_userId, dto)).ReturnsAsync(created);

		var result = await _sut.Create(dto);

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(201);
		var wrapper = objectResult.Value.Should().BeOfType<Result<AccountDto>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data!.Name.Should().Be("New Account");
	}

	[Fact]
	public async Task Update_ReturnsOkWithUpdatedAccount()
	{
		var id = Guid.NewGuid();
		var dto = new UpdateAccountDto("Updated Name");
		var updated = new AccountDto(id, "Updated Name", "Hub", 1000m, DateTime.UtcNow, 0);
		_accountService.Setup(s => s.UpdateAsync(_userId, id, dto)).ReturnsAsync(updated);

		var result = await _sut.Update(id, dto);

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<AccountDto>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data!.Name.Should().Be("Updated Name");
	}

	[Fact]
	public async Task Delete_ReturnsNoContent_WhenTrue()
	{
		var id = Guid.NewGuid();
		_accountService.Setup(s => s.DeleteAsync(_userId, id)).ReturnsAsync(true);

		var result = await _sut.Delete(id);

		result.Should().BeOfType<NoContentResult>();
	}

	[Fact]
	public async Task Delete_ReturnsNotFound_WhenFalse()
	{
		var id = Guid.NewGuid();
		_accountService.Setup(s => s.DeleteAsync(_userId, id)).ReturnsAsync(false);

		var result = await _sut.Delete(id);

		result.Should().BeOfType<NotFoundResult>();
	}

	[Fact]
	public async Task GetTotalBalance_ReturnsOkWithBalance()
	{
		_accountService.Setup(s => s.GetTotalBalanceAsync(_userId)).ReturnsAsync(5000m);

		var result = await _sut.GetTotalBalance();

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<decimal>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data.Should().Be(5000m);
	}
}
