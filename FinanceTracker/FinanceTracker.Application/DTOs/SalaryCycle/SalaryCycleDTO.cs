using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.DTOs.SalaryCycle
{
	public record SalaryCycleDto(
	Guid Id,
	DateTime PayDate,
	decimal GrossSalary,
	decimal NetSalary,
	string Status,
	DateTime CreatedAt,
	DateTime? CompletedAt,
	List<SalaryDistributionDto> Distributions
);

	public record CreateSalaryCycleDto(
		DateTime PayDate,
		decimal GrossSalary,
		decimal NetSalary,
		List<CreateDistributionDto> Distributions
	);

	public record CreateDistributionDto(
		Guid TargetAccountId,
		decimal Amount,
		string DistributionType,
		int OrderIndex
	);

	public record SalaryDistributionDto(
		Guid Id,
		Guid TargetAccountId,
		string TargetAccountName,
		decimal Amount,
		string DistributionType,
		bool IsExecuted,
		DateTime? ExecutedAt
	);

}
