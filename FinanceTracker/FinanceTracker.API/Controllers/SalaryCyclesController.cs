using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.DTOs.SalaryCycle;
using FinanceTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class SalaryCyclesController : ControllerBase
	{
		private readonly ISalaryCycleService _salaryCycleService;
		private readonly ILogger<SalaryCyclesController> _logger;

		public SalaryCyclesController(ISalaryCycleService salaryCycleService, ILogger<SalaryCyclesController> logger)
		{
			_salaryCycleService = salaryCycleService;
			_logger = logger;
		}

		private Guid GetUserId() =>
			Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

		[HttpGet]
		public async Task<IActionResult> GetRecentCycles([FromQuery] int count = 6)
		{
			var userId = GetUserId();
			_logger.LogInformation("Getting recent salary cycles for user {UserId}", userId);
			var cycles = await _salaryCycleService.GetRecentCyclesAsync(userId, count);
			return Ok(Result<IEnumerable<SalaryCycleDto>>.Success(cycles));
		}

		[HttpPost]
		public async Task<IActionResult> CreateSalaryCycle([FromBody] CreateSalaryCycleDto dto)
		{
			var userId = GetUserId();
			_logger.LogInformation("Creating salary cycle for user {UserId}, pay date {PayDate}", userId, dto.PayDate);
			var cycle = await _salaryCycleService.CreateSalaryCycleAsync(userId, dto);
			return StatusCode(201, Result<SalaryCycleDto>.Created(cycle));
		}

		[HttpPost("{id}/execute")]
		public async Task<IActionResult> ExecuteDistributions(Guid id)
		{
			var userId = GetUserId();
			_logger.LogInformation("Executing distributions for cycle {CycleId}, user {UserId}", id, userId);
			var cycle = await _salaryCycleService.ExecuteDistributionsAsync(userId, id);
			return Ok(Result<SalaryCycleDto>.Success(cycle));
		}

		[HttpGet("next-payday")]
		public async Task<IActionResult> GetNextPayDate()
		{
			var userId = GetUserId();
			_logger.LogInformation("Getting next pay date for user {UserId}", userId);
			var nextPayDate = await _salaryCycleService.GetNextPayDateAsync(userId);
			return Ok(Result<DateTime?>.Success(nextPayDate));
		}
	}
}
