using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.DTOs.Transaction;
using FinanceTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class TransactionsController : ControllerBase
	{
		private readonly ITransactionService _transactionService;
		private readonly ILogger<TransactionsController> _logger;

		public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
		{
			_transactionService = transactionService;
			_logger = logger;
		}

		private Guid GetUserId() =>
			Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

		[HttpGet]
		public async Task<IActionResult> GetByUserId([FromQuery] TransactionFilterDto filter)
		{
			var userId = GetUserId();
			_logger.LogInformation("Getting transactions for user {UserId}, page {Page}", userId, filter.PageNumber);
			var result = await _transactionService.GetByUserIdAsync(userId, filter);
			return Ok(Result<TransactionPagedResultDto>.Success(result));
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var userId = GetUserId();
			_logger.LogInformation("Getting transaction {TransactionId} for user {UserId}", id, userId);
			var transaction = await _transactionService.GetByIdAsync(userId, id);
			if (transaction == null) return NotFound();
			return Ok(Result<TransactionDto>.Success(transaction));
		}

		[HttpGet("account/{accountId}")]
		public async Task<IActionResult> GetByAccountId(Guid accountId, [FromQuery] int count = 20)
		{
			var userId = GetUserId();
			_logger.LogInformation("Getting transactions for account {AccountId}, user {UserId}", accountId, userId);
			var transactions = await _transactionService.GetByAccountIdAsync(userId, accountId, count);
			return Ok(Result<IEnumerable<TransactionDto>>.Success(transactions));
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
		{
			var userId = GetUserId();
			_logger.LogInformation("Creating transaction for account {AccountId}, user {UserId}", dto.AccountId, userId);
			var transaction = await _transactionService.CreateAsync(userId, dto);
			return StatusCode(201, Result<TransactionDto>.Created(transaction));
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var userId = GetUserId();
			_logger.LogInformation("Deleting transaction {TransactionId} for user {UserId}", id, userId);
			var result = await _transactionService.DeleteAsync(userId, id);
			if (!result) return NotFound();
			return NoContent();
		}
	}
}
