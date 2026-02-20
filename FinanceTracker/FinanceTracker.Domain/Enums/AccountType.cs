using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Domain.Enums
{
	public enum AccountType
	{
		Payroll = 0,      // Unionbank
		Hub = 1,          // Maribank
		Parking = 2,      // GCash
		Savings = 3,      // Maya
		CashHolding = 4   // BPI
	}
}
