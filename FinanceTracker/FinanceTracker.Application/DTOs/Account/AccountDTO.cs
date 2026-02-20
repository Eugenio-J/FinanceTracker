using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.DTOs.Account
{
	// Account DTOs
	public record AccountDto(
	Guid Id,
	string Name,
	string AccountType,
	decimal CurrentBalance,
	DateTime CreatedAt,
	int TransactionCount
	);

	public record CreateAccountDto(
		string Name,
		string AccountType,
		decimal InitialBalance = 0
	);

	public record UpdateAccountDto(
		string Name
	);
}
