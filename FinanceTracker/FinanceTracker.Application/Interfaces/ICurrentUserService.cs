using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.Interfaces
{
	public interface ICurrentUserService
	{
		int? UserId { get; }
		bool IsAuthenticated { get; }
	}
}
