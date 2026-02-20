using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Infrastructure.Data;
using FinanceTracker.Infrastructure.Repositories;
using FinanceTracker.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<DataContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

			// Repositories
			services.AddScoped<IAccountRepository, AccountRepository>();
			services.AddScoped<IExpenseRepository, ExpenseRepository>();
			services.AddScoped<ISalaryCycleRepository, SalaryCycleRepository>();
			services.AddScoped<ITransactionRepository, TransactionRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

			// Unit of Work
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			// Services
			services.AddScoped<ICurrentUserService, CurrentUserService>();

			return services;
		}
	}
}
