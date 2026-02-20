using FinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Domain.Entities
{
	public class Expense
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid AccountId { get; set; }
		public decimal Amount { get; set; }
		public ExpenseCategory Category { get; set; }
		public string? Description { get; set; }
		public DateTime Date { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Navigation properties
		public User User { get; set; } = null!;
		public Account Account { get; set; } = null!;
	}
}
