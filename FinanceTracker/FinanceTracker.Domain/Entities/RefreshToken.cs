using FinanceTracker.Domain.Helpers;

namespace FinanceTracker.Domain.Entities
{
	public class RefreshToken
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string Token { get; set; } = string.Empty;
		public string Family { get; set; } = string.Empty;
		public DateTime ExpiresAt { get; set; }
		public DateTime CreatedAt { get; set; } = PhilippineDateTime.Now;
		public DateTime? RevokedAt { get; set; }
		public string? ReplacedByToken { get; set; }

		public bool IsRevoked => RevokedAt != null;
		public bool IsExpired => PhilippineDateTime.Now >= ExpiresAt;
		public bool IsActive => !IsRevoked && !IsExpired;

		// Navigation properties
		public User User { get; set; } = null!;
	}
}
