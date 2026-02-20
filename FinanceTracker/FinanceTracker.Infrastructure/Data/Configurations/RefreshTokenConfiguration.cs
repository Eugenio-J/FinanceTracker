using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Data.Configurations
{
	public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
	{
		public void Configure(EntityTypeBuilder<RefreshToken> builder)
		{
			builder.HasKey(rt => rt.Id);

			builder.Property(rt => rt.Token)
				.IsRequired()
				.HasMaxLength(256);

			builder.HasIndex(rt => rt.Token)
				.IsUnique();

			builder.Property(rt => rt.Family)
				.IsRequired()
				.HasMaxLength(64);

			builder.HasIndex(rt => rt.Family);

			builder.HasIndex(rt => rt.UserId);

			builder.HasOne(rt => rt.User)
				.WithMany(u => u.RefreshTokens)
				.HasForeignKey(rt => rt.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Ignore(rt => rt.IsRevoked);
			builder.Ignore(rt => rt.IsExpired);
			builder.Ignore(rt => rt.IsActive);
		}
	}
}
