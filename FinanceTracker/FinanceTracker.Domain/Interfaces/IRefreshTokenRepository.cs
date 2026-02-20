using FinanceTracker.Domain.Entities;

namespace FinanceTracker.Domain.Interfaces
{
	public interface IRefreshTokenRepository : IRepository<RefreshToken>
	{
		Task<RefreshToken?> GetByTokenAsync(string token);
		Task<IEnumerable<RefreshToken>> GetActiveByFamilyAsync(string family);
		Task RevokeAllByFamilyAsync(string family);
		Task RevokeAllByUserIdAsync(Guid userId);
	}
}
