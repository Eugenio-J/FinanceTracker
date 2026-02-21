using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Helpers;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Repositories
{
	public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
	{
		public RefreshTokenRepository(DataContext context) : base(context) { }

		public async Task<RefreshToken?> GetByTokenAsync(string token)
		{
			return await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token);
		}

		public async Task<IEnumerable<RefreshToken>> GetActiveByFamilyAsync(string family)
		{
			return await _dbSet
				.Where(rt => rt.Family == family && rt.RevokedAt == null && rt.ExpiresAt > PhilippineDateTime.Now)
				.ToListAsync();
		}

		public async Task RevokeAllByFamilyAsync(string family)
		{
			var tokens = await _dbSet
				.Where(rt => rt.Family == family && rt.RevokedAt == null)
				.ToListAsync();

			foreach (var token in tokens)
			{
				token.RevokedAt = PhilippineDateTime.Now;
			}
		}

		public async Task RevokeAllByUserIdAsync(Guid userId)
		{
			var tokens = await _dbSet
				.Where(rt => rt.UserId == userId && rt.RevokedAt == null)
				.ToListAsync();

			foreach (var token in tokens)
			{
				token.RevokedAt = PhilippineDateTime.Now;
			}
		}
	}
}
