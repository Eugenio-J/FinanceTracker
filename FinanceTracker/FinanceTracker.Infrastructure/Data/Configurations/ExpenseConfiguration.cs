using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Data.Configurations
{
	public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
	{
		public void Configure(EntityTypeBuilder<Expense> builder)
		{
			builder.HasKey(e => e.Id);

			builder.Property(e => e.Amount)
				.HasPrecision(18, 2);

			builder.Property(e => e.Category)
				.HasConversion<string>()
				.HasMaxLength(50);

			builder.Property(e => e.Description)
				.HasMaxLength(500);

			builder.HasOne(e => e.User)
				.WithMany(u => u.Expenses)
				.HasForeignKey(e => e.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(e => e.Account)
				.WithMany(a => a.Expenses)
				.HasForeignKey(e => e.AccountId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasIndex(e => e.UserId);
			builder.HasIndex(e => e.Date);
		}
	}
}
