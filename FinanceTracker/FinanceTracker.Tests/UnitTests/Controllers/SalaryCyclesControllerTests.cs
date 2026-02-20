using FinanceTracker.API.Controllers;
using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.DTOs.SalaryCycle;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FinanceTracker.Tests.UnitTests.Controllers;

public class SalaryCyclesControllerTests
{
	private readonly Mock<ISalaryCycleService> _salaryCycleService;
	private readonly SalaryCyclesController _sut;
	private readonly Guid _userId = Guid.NewGuid();

	public SalaryCyclesControllerTests()
	{
		_salaryCycleService = new Mock<ISalaryCycleService>();
		_sut = new SalaryCyclesController(_salaryCycleService.Object, NullLogger<SalaryCyclesController>.Instance);
		TestHelpers.SetupControllerContext(_sut, _userId);
	}

	[Fact]
	public async Task GetRecentCycles_ReturnsOkWithCycles()
	{
		var cycles = new List<SalaryCycleDto>
		{
			new SalaryCycleDto(Guid.NewGuid(), DateTime.UtcNow, 5000m, 4000m, "Completed", DateTime.UtcNow, DateTime.UtcNow, new List<SalaryDistributionDto>())
		};
		_salaryCycleService.Setup(s => s.GetRecentCyclesAsync(_userId, 6)).ReturnsAsync(cycles);

		var result = await _sut.GetRecentCycles();

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<IEnumerable<SalaryCycleDto>>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data.Should().HaveCount(1);
	}

	[Fact]
	public async Task CreateSalaryCycle_ReturnsCreatedWithCycle()
	{
		var dto = new CreateSalaryCycleDto(DateTime.UtcNow, 5000m, 4000m, new List<CreateDistributionDto>());
		var created = new SalaryCycleDto(Guid.NewGuid(), DateTime.UtcNow, 5000m, 4000m, "Pending", DateTime.UtcNow, null, new List<SalaryDistributionDto>());
		_salaryCycleService.Setup(s => s.CreateSalaryCycleAsync(_userId, dto)).ReturnsAsync(created);

		var result = await _sut.CreateSalaryCycle(dto);

		var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
		objectResult.StatusCode.Should().Be(201);
		var wrapper = objectResult.Value.Should().BeOfType<Result<SalaryCycleDto>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task ExecuteDistributions_ReturnsOkWithCycle()
	{
		var cycleId = Guid.NewGuid();
		var executed = new SalaryCycleDto(cycleId, DateTime.UtcNow, 5000m, 4000m, "Completed", DateTime.UtcNow, DateTime.UtcNow, new List<SalaryDistributionDto>());
		_salaryCycleService.Setup(s => s.ExecuteDistributionsAsync(_userId, cycleId)).ReturnsAsync(executed);

		var result = await _sut.ExecuteDistributions(cycleId);

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<SalaryCycleDto>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data!.Status.Should().Be("Completed");
	}

	[Fact]
	public async Task GetNextPayDate_ReturnsOkWithDate()
	{
		var nextPayDate = new DateTime(2026, 3, 1);
		_salaryCycleService.Setup(s => s.GetNextPayDateAsync(_userId)).ReturnsAsync(nextPayDate);

		var result = await _sut.GetNextPayDate();

		var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
		var wrapper = okResult.Value.Should().BeOfType<Result<DateTime?>>().Subject;
		wrapper.IsSuccess.Should().BeTrue();
		wrapper.Data.Should().Be(nextPayDate);
	}
}
