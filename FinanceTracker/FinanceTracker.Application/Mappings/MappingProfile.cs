using AutoMapper;
using FinanceTracker.Application.DTOs.Account;
using FinanceTracker.Application.DTOs.Dashboard;
using FinanceTracker.Application.DTOs.Expense;
using FinanceTracker.Application.DTOs.SalaryCycle;
using FinanceTracker.Application.DTOs.Transaction;
using FinanceTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.Mappings
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			// Account mappings
			CreateMap<Account, AccountDto>()
				.ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
				.ForCtorParam("Name", opt => opt.MapFrom(src => src.Name))
				.ForCtorParam("AccountType", opt => opt.MapFrom(src => src.AccountType.ToString()))
				.ForCtorParam("CurrentBalance", opt => opt.MapFrom(src => src.CurrentBalance))
				.ForCtorParam("CreatedAt", opt => opt.MapFrom(src => src.CreatedAt))
				.ForCtorParam("TransactionCount", opt => opt.MapFrom(src => src.Transactions.Count));

			// Expense mappings
			CreateMap<Expense, ExpenseDto>()
				.ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
				.ForCtorParam("AccountId", opt => opt.MapFrom(src => src.AccountId))
				.ForCtorParam("AccountName", opt => opt.MapFrom(src => src.Account.Name))
				.ForCtorParam("Amount", opt => opt.MapFrom(src => src.Amount))
				.ForCtorParam("Category", opt => opt.MapFrom(src => src.Category.ToString()))
				.ForCtorParam("Description", opt => opt.MapFrom(src => src.Description))
				.ForCtorParam("Date", opt => opt.MapFrom(src => src.Date))
				.ForCtorParam("CreatedAt", opt => opt.MapFrom(src => src.CreatedAt));

			// SalaryCycle mappings
			CreateMap<SalaryCycle, SalaryCycleDto>()
				.ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
				.ForCtorParam("PayDate", opt => opt.MapFrom(src => src.PayDate))
				.ForCtorParam("GrossSalary", opt => opt.MapFrom(src => src.GrossSalary))
				.ForCtorParam("NetSalary", opt => opt.MapFrom(src => src.NetSalary))
				.ForCtorParam("Status", opt => opt.MapFrom(src => src.Status.ToString()))
				.ForCtorParam("CreatedAt", opt => opt.MapFrom(src => src.CreatedAt))
				.ForCtorParam("CompletedAt", opt => opt.MapFrom(src => src.CompletedAt))
				.ForCtorParam("Distributions", opt => opt.MapFrom(src => src.Distributions));

			CreateMap<SalaryDistribution, SalaryDistributionDto>()
				.ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
				.ForCtorParam("TargetAccountId", opt => opt.MapFrom(src => src.TargetAccountId))
				.ForCtorParam("TargetAccountName", opt => opt.MapFrom(src => src.TargetAccount.Name))
				.ForCtorParam("Amount", opt => opt.MapFrom(src => src.Amount))
				.ForCtorParam("DistributionType", opt => opt.MapFrom(src => src.DistributionType.ToString()))
				.ForCtorParam("IsExecuted", opt => opt.MapFrom(src => src.IsExecuted))
				.ForCtorParam("ExecutedAt", opt => opt.MapFrom(src => src.ExecutedAt));

			// Transaction mappings
			CreateMap<Transactions, TransactionDto>()
				.ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
				.ForCtorParam("AccountId", opt => opt.MapFrom(src => src.AccountId))
				.ForCtorParam("AccountName", opt => opt.MapFrom(src => src.Account.Name))
				.ForCtorParam("Amount", opt => opt.MapFrom(src => src.Amount))
				.ForCtorParam("TransactionType", opt => opt.MapFrom(src => src.TransactionType.ToString()))
				.ForCtorParam("Category", opt => opt.MapFrom(src => src.Category.ToString()))
				.ForCtorParam("Description", opt => opt.MapFrom(src => src.Description))
				.ForCtorParam("Date", opt => opt.MapFrom(src => src.Date))
				.ForCtorParam("RelatedTransactionId", opt => opt.MapFrom(src => src.RelatedTransactionId))
				.ForCtorParam("CreatedAt", opt => opt.MapFrom(src => src.CreatedAt));
		}
	}
}
