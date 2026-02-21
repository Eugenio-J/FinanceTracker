using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Domain.Entities
{
	public class SalaryCycle
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public DateTime PayDate { get; set; }
		public decimal GrossSalary { get; set; }
		public decimal NetSalary { get; set; }
		public SalaryCycleStatus Status { get; set; } = SalaryCycleStatus.Pending;
		public DateTime CreatedAt { get; set; } = PhilippineDateTime.Now;
		public DateTime? CompletedAt { get; set; }

		// Navigation properties
		public User User { get; set; } = null!;
		public ICollection<SalaryDistribution> Distributions { get; set; } = new List<SalaryDistribution>();
	}
}
