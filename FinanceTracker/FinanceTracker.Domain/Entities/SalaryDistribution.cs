using FinanceTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Domain.Entities
{
	public class SalaryDistribution
	{
		public Guid Id { get; set; }
		public Guid SalaryCycleId { get; set; }
		public Guid TargetAccountId { get; set; }
		public decimal Amount { get; set; }
		public DistributionType DistributionType { get; set; }
		public bool IsExecuted { get; set; } = false;
		public DateTime? ExecutedAt { get; set; }
		public int OrderIndex { get; set; } // For sequential distribution

		// Navigation properties
		public SalaryCycle SalaryCycle { get; set; } = null!;
		public Account TargetAccount { get; set; } = null!;
	}
}
