using FinanceTracker.Application.DTOs.Account;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.Interfaces
{
	public interface IAccountService
	{
		Task<IEnumerable<AccountDto>> GetAllByUserIdAsync(Guid userId);
		Task<AccountDto?> GetByIdAsync(Guid userId, Guid accountId);
		Task<AccountDto> CreateAsync(Guid userId, CreateAccountDto request);
		Task<AccountDto> UpdateAsync(Guid userId, Guid accountId, UpdateAccountDto request);
		Task<bool> DeleteAsync(Guid userId, Guid accountId);
		Task<decimal> GetTotalBalanceAsync(Guid userId);
	}
}
