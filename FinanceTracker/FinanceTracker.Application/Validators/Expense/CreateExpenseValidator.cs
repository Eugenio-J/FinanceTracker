using FinanceTracker.Application.DTOs.Expense;
using FinanceTracker.Domain.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.Validators.Expense
{
	public class CreateExpenseValidator : AbstractValidator<CreateExpenseDto>
	{
		public CreateExpenseValidator()
		{
			RuleFor(x => x.AccountId)
				.NotEmpty()
				.WithMessage("Account is required.");

			RuleFor(x => x.Amount)
				.GreaterThan(0)
				.WithMessage("Amount must be greater than zero.");

			RuleFor(x => x.Category)
				.NotEmpty()
				.WithMessage("Category is required.")
				.Must(BeValidCategory)
				.WithMessage("Invalid expense category.");

			RuleFor(x => x.Date)
				.NotEmpty()
				.WithMessage("Date is required.")
				.LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
				.WithMessage("Date cannot be in the future.");

			RuleFor(x => x.Description)
				.MaximumLength(500)
				.WithMessage("Description cannot exceed 500 characters.");
		}

		private bool BeValidCategory(string category)
		{
			return Enum.TryParse<ExpenseCategory>(category, true, out _);
		}
	}
}
