using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Domain.Enums
{
	public enum DistributionType
	{
		Fixed = 0,        // Fixed amount
		Percentage = 1,   // Percentage of salary
		Remainder = 2     // Whatever is left
	}
}
