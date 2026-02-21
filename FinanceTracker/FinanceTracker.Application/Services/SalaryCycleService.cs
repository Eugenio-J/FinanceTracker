using AutoMapper;
using FinanceTracker.Application.DTOs.SalaryCycle;
using FinanceTracker.Application.Interfaces;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Helpers;
using FinanceTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Application.Services
{
	public class SalaryCycleService : ISalaryCycleService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly ILogger<SalaryCycleService> _logger;

		public SalaryCycleService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SalaryCycleService> logger)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_logger = logger;
		}

		public async Task<IEnumerable<SalaryCycleDto>> GetRecentCyclesAsync(Guid userId, int count = 6)
		{
			var cycles = await _unitOfWork.SalaryCycles.GetByUserIdAsync(userId, count);
			return _mapper.Map<IEnumerable<SalaryCycleDto>>(cycles);
		}

		public async Task<SalaryCycleDto> CreateSalaryCycleAsync(Guid userId, CreateSalaryCycleDto dto)
		{
			var cycle = new SalaryCycle
			{
				Id = Guid.NewGuid(),
				UserId = userId,
				PayDate = dto.PayDate,
				GrossSalary = dto.GrossSalary,
				NetSalary = dto.NetSalary,
				Status = SalaryCycleStatus.Pending,
				CreatedAt = PhilippineDateTime.Now
			};

			// Add distributions
			foreach (var distDto in dto.Distributions)
			{
				var distribution = new SalaryDistribution
				{
					Id = Guid.NewGuid(),
					SalaryCycleId = cycle.Id,
					TargetAccountId = distDto.TargetAccountId,
					Amount = distDto.Amount,
					DistributionType = Enum.Parse<DistributionType>(distDto.DistributionType, true),
					OrderIndex = distDto.OrderIndex,
					IsExecuted = false
				};
				cycle.Distributions.Add(distribution);
			}

			await _unitOfWork.SalaryCycles.AddAsync(cycle);
			await _unitOfWork.SaveChangesAsync();

			_logger.LogInformation("Created salary cycle {CycleId} for user {UserId}, pay date {PayDate}, net {NetSalary}, {DistributionCount} distributions",
				cycle.Id, userId, dto.PayDate, dto.NetSalary, dto.Distributions.Count);

			return _mapper.Map<SalaryCycleDto>(cycle);
		}

		public async Task<SalaryCycleDto> ExecuteDistributionsAsync(Guid userId, Guid cycleId)
		{
			_logger.LogInformation("Starting distribution execution for cycle {CycleId}, user {UserId}", cycleId, userId);
			await _unitOfWork.BeginTransactionAsync();

			try
			{
				var cycle = await _unitOfWork.SalaryCycles.GetByIdWithDistributionsAsync(cycleId);
				if (cycle == null || cycle.UserId != userId)
				{
					throw new ArgumentException("Salary cycle not found.");
				}

				if (cycle.Status == SalaryCycleStatus.Completed)
				{
					throw new InvalidOperationException("Salary cycle already completed.");
				}

				cycle.Status = SalaryCycleStatus.InProgress;
				var remainingSalary = cycle.NetSalary;

				// Process distributions in order
				var orderedDistributions = cycle.Distributions.OrderBy(d => d.OrderIndex).ToList();

				foreach (var distribution in orderedDistributions)
				{
					var account = await _unitOfWork.Accounts.GetByIdAsync(distribution.TargetAccountId);
					if (account == null) continue;

					decimal amountToTransfer;

					switch (distribution.DistributionType)
					{
						case DistributionType.Fixed:
							amountToTransfer = Math.Min(distribution.Amount, remainingSalary);
							break;
						case DistributionType.Percentage:
							amountToTransfer = Math.Min(cycle.NetSalary * (distribution.Amount / 100), remainingSalary);
							break;
						case DistributionType.Remainder:
							amountToTransfer = remainingSalary;
							break;
						default:
							amountToTransfer = 0;
							break;
					}

					if (amountToTransfer > 0)
					{
						// Update account balance
						account.CurrentBalance += amountToTransfer;
						account.UpdatedAt = PhilippineDateTime.Now;
						_unitOfWork.Accounts.Update(account);

						// Create transaction
						var transaction = new Transactions
						{
							Id = Guid.NewGuid(),
							AccountId = account.Id,
							Amount = amountToTransfer,
							TransactionType = TransactionType.Deposit,
							Category = TransactionCategory.Distribution,
							Description = $"Salary distribution - {cycle.PayDate:yyyy-MM-dd}",
							Date = PhilippineDateTime.Now,
							CreatedAt = PhilippineDateTime.Now
						};
						await _unitOfWork.Transactions.AddAsync(transaction);

						distribution.IsExecuted = true;
						distribution.ExecutedAt = PhilippineDateTime.Now;
						remainingSalary -= amountToTransfer;

						_logger.LogInformation("Distributed {Amount} to account {AccountId} ({DistributionType})",
							amountToTransfer, account.Id, distribution.DistributionType);
					}
				}

				cycle.Status = SalaryCycleStatus.Completed;
				cycle.CompletedAt = PhilippineDateTime.Now;

				await _unitOfWork.SaveChangesAsync();
				await _unitOfWork.CommitTransactionAsync();

				_logger.LogInformation("Completed distribution execution for cycle {CycleId}", cycleId);
				return _mapper.Map<SalaryCycleDto>(cycle);
			}
			catch
			{
				await _unitOfWork.RollbackTransactionAsync();
				_logger.LogWarning("Distribution execution failed for cycle {CycleId}, transaction rolled back", cycleId);
				throw;
			}
		}

		public async Task<DateTime?> GetNextPayDateAsync(Guid userId)
		{
			var latestCycle = await _unitOfWork.SalaryCycles.GetLatestByUserIdAsync(userId);
			if (latestCycle == null) return null;

			// Bi-weekly: add 14 days to last pay date
			return latestCycle.PayDate.AddDays(14);
		}
	}
}
