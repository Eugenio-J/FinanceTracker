using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Infrastructure.Repositories;
using FinanceTracker.Tests.Helpers;
using FluentAssertions;

namespace FinanceTracker.Tests.IntegrationTests.Repositories;

public class SalaryCycleRepositoryTests
{
	private readonly Guid _userId = Guid.NewGuid();

	private SalaryCycleRepository CreateRepository(out Infrastructure.Data.DataContext context)
	{
		context = TestHelpers.CreateInMemoryContext();
		return new SalaryCycleRepository(context);
	}

	[Fact]
	public async Task GetByUserIdAsync_ReturnsCyclesForUser()
	{
		var repo = CreateRepository(out var ctx);
		ctx.SalaryCycles.AddRange(
			TestHelpers.CreateTestSalaryCycle(_userId, DateTime.UtcNow.AddDays(-28)),
			TestHelpers.CreateTestSalaryCycle(_userId, DateTime.UtcNow.AddDays(-14)),
			TestHelpers.CreateTestSalaryCycle(Guid.NewGuid(), DateTime.UtcNow)
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetByUserIdAsync(_userId);

		result.Should().HaveCount(2);
	}

	[Fact]
	public async Task GetByUserIdAsync_OrdersByPayDateDescending()
	{
		var repo = CreateRepository(out var ctx);
		ctx.SalaryCycles.AddRange(
			TestHelpers.CreateTestSalaryCycle(_userId, new DateTime(2025, 1, 1)),
			TestHelpers.CreateTestSalaryCycle(_userId, new DateTime(2025, 3, 1)),
			TestHelpers.CreateTestSalaryCycle(_userId, new DateTime(2025, 2, 1))
		);
		await ctx.SaveChangesAsync();

		var result = (await repo.GetByUserIdAsync(_userId)).ToList();

		result.Should().BeInDescendingOrder(c => c.PayDate);
	}

	[Fact]
	public async Task GetByUserIdAsync_LimitsCount()
	{
		var repo = CreateRepository(out var ctx);
		for (int i = 0; i < 10; i++)
		{
			ctx.SalaryCycles.Add(TestHelpers.CreateTestSalaryCycle(_userId, DateTime.UtcNow.AddDays(-i * 14)));
		}
		await ctx.SaveChangesAsync();

		var result = await repo.GetByUserIdAsync(_userId, count: 3);

		result.Should().HaveCount(3);
	}

	[Fact]
	public async Task GetByIdWithDistributionsAsync_IncludesDistributions()
	{
		var repo = CreateRepository(out var ctx);
		var cycle = TestHelpers.CreateTestSalaryCycle(_userId);
		cycle.Distributions.Add(new SalaryDistribution
		{
			Id = Guid.NewGuid(),
			SalaryCycleId = cycle.Id,
			TargetAccountId = Guid.NewGuid(),
			Amount = 1000m,
			DistributionType = DistributionType.Fixed,
			OrderIndex = 0
		});
		ctx.SalaryCycles.Add(cycle);
		await ctx.SaveChangesAsync();

		var result = await repo.GetByIdWithDistributionsAsync(cycle.Id);

		result.Should().NotBeNull();
		result!.Distributions.Should().HaveCount(1);
	}

	[Fact]
	public async Task GetByIdWithDistributionsAsync_ReturnsNull_WhenNotFound()
	{
		var repo = CreateRepository(out _);

		var result = await repo.GetByIdWithDistributionsAsync(Guid.NewGuid());

		result.Should().BeNull();
	}

	[Fact]
	public async Task GetLatestByUserIdAsync_ReturnsLatestCycle()
	{
		var repo = CreateRepository(out var ctx);
		ctx.SalaryCycles.AddRange(
			TestHelpers.CreateTestSalaryCycle(_userId, new DateTime(2025, 1, 1)),
			TestHelpers.CreateTestSalaryCycle(_userId, new DateTime(2025, 3, 1)),
			TestHelpers.CreateTestSalaryCycle(_userId, new DateTime(2025, 2, 1))
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetLatestByUserIdAsync(_userId);

		result.Should().NotBeNull();
		result!.PayDate.Should().Be(new DateTime(2025, 3, 1));
	}

	[Fact]
	public async Task GetLatestByUserIdAsync_ReturnsNull_WhenNoCycles()
	{
		var repo = CreateRepository(out _);

		var result = await repo.GetLatestByUserIdAsync(Guid.NewGuid());

		result.Should().BeNull();
	}

	[Fact]
	public async Task GetNextPayDateAsync_ReturnsFuturePayDate()
	{
		var repo = CreateRepository(out var ctx);
		var futureDate = DateTime.UtcNow.AddDays(14);
		ctx.SalaryCycles.AddRange(
			TestHelpers.CreateTestSalaryCycle(_userId, DateTime.UtcNow.AddDays(-14)),
			TestHelpers.CreateTestSalaryCycle(_userId, futureDate),
			TestHelpers.CreateTestSalaryCycle(_userId, DateTime.UtcNow.AddDays(28))
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetNextPayDateAsync(_userId);

		result.Should().NotBeNull();
		result!.Value.Date.Should().Be(futureDate.Date);
	}

	[Fact]
	public async Task GetNextPayDateAsync_ReturnsNull_WhenNoFutureDates()
	{
		var repo = CreateRepository(out var ctx);
		ctx.SalaryCycles.Add(TestHelpers.CreateTestSalaryCycle(_userId, DateTime.UtcNow.AddDays(-14)));
		await ctx.SaveChangesAsync();

		var result = await repo.GetNextPayDateAsync(_userId);

		result.Should().BeNull();
	}

	[Fact]
	public async Task GetNextPayDateAsync_ReturnsEarliestFutureDate()
	{
		var repo = CreateRepository(out var ctx);
		var nearFuture = DateTime.UtcNow.AddDays(7);
		var farFuture = DateTime.UtcNow.AddDays(21);
		ctx.SalaryCycles.AddRange(
			TestHelpers.CreateTestSalaryCycle(_userId, farFuture),
			TestHelpers.CreateTestSalaryCycle(_userId, nearFuture)
		);
		await ctx.SaveChangesAsync();

		var result = await repo.GetNextPayDateAsync(_userId);

		result!.Value.Date.Should().Be(nearFuture.Date);
	}
}
