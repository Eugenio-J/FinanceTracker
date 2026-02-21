using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace FinanceTracker.Domain.Entities
{
	public class User
	{
		public Guid Id { get; set; }
		public string Email { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public UserRole Role { get; set; } = UserRole.User;
		public DateTime CreatedAt { get; set; } = PhilippineDateTime.Now;
		public DateTime? UpdatedAt { get; set; }

		// Navigation properties
		public ICollection<Account> Accounts { get; set; } = new List<Account>();
		public ICollection<SalaryCycle> SalaryCycles { get; set; } = new List<SalaryCycle>();
		public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
		public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
	}
}
