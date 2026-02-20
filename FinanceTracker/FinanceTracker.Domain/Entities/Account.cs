using FinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace FinanceTracker.Domain.Entities
{
	public class Account
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string Name { get; set; } = string.Empty;
		public AccountType AccountType { get; set; }
		public decimal CurrentBalance { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		public User User { get; set; } = null!;
		public ICollection<Transactions> Transactions { get; set; } = new List<Transactions>();
		public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
		public ICollection<SalaryDistribution> SalaryDistributions { get; set; } = new List<SalaryDistribution>();
	}
}
