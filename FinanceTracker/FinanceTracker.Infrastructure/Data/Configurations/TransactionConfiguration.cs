using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Data.Configurations
{
	public class TransactionConfiguration : IEntityTypeConfiguration<Transactions>
	{
		public void Configure(EntityTypeBuilder<Transactions> builder)
		{
			builder.HasKey(t => t.Id);

			builder.Property(t => t.Amount)
				.HasPrecision(18, 2);

			builder.Property(t => t.TransactionType)
				.HasConversion<string>()
				.HasMaxLength(50);

			builder.Property(t => t.Category)
				.HasConversion<string>()
				.HasMaxLength(50);

			builder.Property(t => t.Description)
				.HasMaxLength(500);

			builder.HasOne(t => t.Account)
				.WithMany(a => a.Transactions)
				.HasForeignKey(t => t.AccountId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(t => t.RelatedTransaction)
				.WithOne()
				.HasForeignKey<Transactions>(t => t.RelatedTransactionId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasIndex(t => t.AccountId);
			builder.HasIndex(t => t.Date);
		}
	}
}
