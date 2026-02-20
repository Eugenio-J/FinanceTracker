using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Data.Configurations
{
	public class SalaryCycleConfiguration : IEntityTypeConfiguration<SalaryCycle>
	{
		public void Configure(EntityTypeBuilder<SalaryCycle> builder)
		{
			builder.HasKey(sc => sc.Id);

			builder.Property(sc => sc.GrossSalary)
				.HasPrecision(18, 2);

			builder.Property(sc => sc.NetSalary)
				.HasPrecision(18, 2);

			builder.Property(sc => sc.Status)
				.HasConversion<string>()
				.HasMaxLength(50);

			builder.HasOne(sc => sc.User)
				.WithMany(u => u.SalaryCycles)
				.HasForeignKey(sc => sc.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasIndex(sc => sc.UserId);
			builder.HasIndex(sc => sc.PayDate);
		}
	}
}
