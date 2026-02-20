using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using System.Transactions;

namespace FinanceTracker.Infrastructure.Data
{
	public class DataContext : DbContext
	{
		public DataContext(DbContextOptions<DataContext> options)
		: base(options) { }

		public DbSet<User> Users => Set<User>();
		public DbSet<Account> Accounts => Set<Account>();
		public DbSet<Transactions> Transactions => Set<Transactions>();
		public DbSet<SalaryCycle> SalaryCycles => Set<SalaryCycle>();
		public DbSet<SalaryDistribution> SalaryDistributions => Set<SalaryDistribution>();
		public DbSet<Expense> Expenses => Set<Expense>();
		public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfigurationsFromAssembly(
				typeof(DataContext).Assembly);
			base.OnModelCreating(modelBuilder);
		}
	}
}
