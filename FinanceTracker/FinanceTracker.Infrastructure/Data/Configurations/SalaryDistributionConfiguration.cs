using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Data.Configurations
{
	public class SalaryDistributionConfiguration : IEntityTypeConfiguration<SalaryDistribution>
	{
		public void Configure(EntityTypeBuilder<SalaryDistribution> builder)
		{
			builder.HasKey(sd => sd.Id);

			builder.Property(sd => sd.Amount)
				.HasPrecision(18, 2);

			builder.Property(sd => sd.DistributionType)
				.HasConversion<string>()
				.HasMaxLength(50);

			builder.HasOne(sd => sd.SalaryCycle)
				.WithMany(sc => sc.Distributions)
				.HasForeignKey(sd => sd.SalaryCycleId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(sd => sd.TargetAccount)
				.WithMany(a => a.SalaryDistributions)
				.HasForeignKey(sd => sd.TargetAccountId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasIndex(sd => sd.SalaryCycleId);
		}
	}
}
