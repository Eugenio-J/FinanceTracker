using Microsoft.EntityFrameworkCore;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Infrastructure.Data;

namespace FinanceTracker.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
	public UserRepository(DataContext context) : base(context)
	{
	}

	public async Task<User?> GetByEmailAsync(string email)
	{
		return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
	}

	public async Task<bool> EmailExistsAsync(string email)
	{
		return await _dbSet.AnyAsync(u => u.Email == email);
	}
}