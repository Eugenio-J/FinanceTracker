using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Infrastructure.Data.Configurations
{
	public class AccountConfiguration : IEntityTypeConfiguration<Account>
	{
		public void Configure(EntityTypeBuilder<Account> builder)
		{
			builder.HasKey(a => a.Id);

			builder.Property(a => a.Name)
				.IsRequired()
				.HasMaxLength(100);

			builder.Property(a => a.CurrentBalance)
				.HasPrecision(18, 2);

			builder.HasOne(a => a.User)
				.WithMany(u => u.Accounts)
				.HasForeignKey(a => a.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasIndex(a => a.UserId);
		}
	}
}
