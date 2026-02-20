using FinanceTracker.Application.DTOs.SalaryCycle;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.Interfaces
{
	public interface ISalaryCycleService
	{
		Task<IEnumerable<SalaryCycleDto>> GetRecentCyclesAsync(Guid userId, int count = 6);
		Task<SalaryCycleDto> CreateSalaryCycleAsync(Guid userId, CreateSalaryCycleDto dto);
		Task<SalaryCycleDto> ExecuteDistributionsAsync(Guid userId, Guid cycleId);
		Task<DateTime?> GetNextPayDateAsync(Guid userId);
	}
}
