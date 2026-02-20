using FinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Domain.Entities
{
	public class Transactions
	{
		public Guid Id { get; set; }
		public Guid AccountId { get; set; }
		public decimal Amount { get; set; }
		public TransactionType TransactionType { get; set; }
		public TransactionCategory Category { get; set; }
		public string? Description { get; set; }
		public DateTime Date { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// For transfers: reference to the other side
		public Guid? RelatedTransactionId { get; set; }
		public Transactions? RelatedTransaction { get; set; }

		// Navigation properties
		public Account Account { get; set; } = null!;
	}
}
