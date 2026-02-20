using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Data.Configurations
{
	public class UserConfiguration : IEntityTypeConfiguration<User>
	{
		public void Configure(EntityTypeBuilder<User> builder)
		{
			builder.HasKey(u => u.Id);

			builder.Property(u => u.Email)
				.IsRequired()
				.HasMaxLength(256);

			builder.Property(u => u.PasswordHash)
				.IsRequired();

			builder.Property(u => u.FirstName)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(u => u.LastName)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(u => u.Role)
				.HasConversion<string>()
				.HasMaxLength(50);

			builder.HasIndex(u => u.Email)
				.IsUnique();
		}
	}
}
