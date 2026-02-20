using System;
using System.Collections.Generic;
using System.Text;
using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Domain.Interfaces
{
	public interface IUserRepository : IRepository<User>
	{
		Task<User?> GetByEmailAsync(string email);
		Task<bool> EmailExistsAsync(string email);
	}
}
