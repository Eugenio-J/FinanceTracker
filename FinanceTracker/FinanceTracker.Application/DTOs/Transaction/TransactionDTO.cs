using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.DTOs.Transaction
{

	public record TransactionDto(
	Guid Id,
	Guid AccountId,
	string AccountName,
	decimal Amount,
	string TransactionType,
	string Category,
	string? Description,
	DateTime Date,
	Guid? RelatedTransactionId,
	DateTime CreatedAt
);
	public record CreateTransactionDto(
	Guid AccountId,
	decimal Amount,
	string TransactionType,
	string Category,
	string? Description,
	DateTime Date
);
	public record TransactionFilterDto(
	Guid? AccountId = null,
	string? TransactionType = null,
	string? Category = null,
	DateTime? StartDate = null,
	DateTime? EndDate = null,
	int PageNumber = 1,
	int PageSize = 20
);
	public record TransactionPagedResultDto(
	IEnumerable<TransactionDto> Items,
	int TotalCount,
	int PageNumber,
	int PageSize,
	int TotalPages
);
}
