using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.DTOs.Expense
{
	// Expense DTOs
	public record ExpenseDto(
	Guid Id,
	Guid AccountId,
	string AccountName,
	decimal Amount,
	string Category,
	string? Description,
	DateTime Date,
	DateTime CreatedAt
);

	public record CreateExpenseDto(
		Guid AccountId,
		decimal Amount,
		string Category,
		string? Description,
		DateTime Date
	);

	public record ExpenseSummaryDto(
		decimal TotalAmount,
		Dictionary<string, decimal> ByCategory,
		Dictionary<string, decimal> ByAccount
	);
}
