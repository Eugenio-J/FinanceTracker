using FinanceTracker.Application.Interfaces;
using FinanceTracker.Application.Mappings;
using FinanceTracker.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(this IServiceCollection services)
		{
			// AutoMapper
			services.AddAutoMapper(typeof(MappingProfile));

			// FluentValidation
			services.AddValidatorsFromAssemblyContaining<MappingProfile>();

			// Services
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IExpenseService, ExpenseService>();
			services.AddScoped<ISalaryCycleService, SalaryCycleService>();
			services.AddScoped<IAccountService, AccountService>();
			services.AddScoped<ITransactionService, TransactionService>();
			services.AddScoped<IDashboardService, DashboardService>();

			return services;
		}
	}
}
