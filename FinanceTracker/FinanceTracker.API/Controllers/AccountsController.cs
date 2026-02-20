using FinanceTracker.Application.DTOs.Account;
using FinanceTracker.Application.DTOs.Common;
using FinanceTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class AccountsController : ControllerBase
	{
		private readonly IAccountService _accountService;
		private readonly ILogger<AccountsController> _logger;

		public AccountsController(IAccountService accountService, ILogger<AccountsController> logger)
		{
			_accountService = accountService;
			_logger = logger;
		}

		private Guid GetUserId() =>
			Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var userId = GetUserId();
			_logger.LogInformation("Getting all accounts for user {UserId}", userId);
			var accounts = await _accountService.GetAllByUserIdAsync(userId);
			return Ok(Result<IEnumerable<AccountDto>>.Success(accounts));
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(Guid id)
		{
			var userId = GetUserId();
			_logger.LogInformation("Getting account {AccountId} for user {UserId}", id, userId);
			var account = await _accountService.GetByIdAsync(userId, id);
			if (account == null) return NotFound();
			return Ok(Result<AccountDto>.Success(account));
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateAccountDto dto)
		{
			var userId = GetUserId();
			_logger.LogInformation("Creating account for user {UserId}", userId);
			var account = await _accountService.CreateAsync(userId, dto);
			return StatusCode(201, Result<AccountDto>.Created(account));
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountDto dto)
		{
			var userId = GetUserId();
			_logger.LogInformation("Updating account {AccountId} for user {UserId}", id, userId);
			var account = await _accountService.UpdateAsync(userId, id, dto);
			return Ok(Result<AccountDto>.Success(account));
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var userId = GetUserId();
			_logger.LogInformation("Deleting account {AccountId} for user {UserId}", id, userId);
			var result = await _accountService.DeleteAsync(userId, id);
			if (!result) return NotFound();
			return NoContent();
		}

		[HttpGet("total-balance")]
		public async Task<IActionResult> GetTotalBalance()
		{
			var userId = GetUserId();
			_logger.LogInformation("Getting total balance for user {UserId}", userId);
			var balance = await _accountService.GetTotalBalanceAsync(userId);
			return Ok(Result<decimal>.Success(balance));
		}
	}
}
