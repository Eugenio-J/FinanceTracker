using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Domain.Interfaces
{
	public interface IRepository<T> where T : class
	{
		Task<T?> GetByIdAsync(Guid id);
		Task<IEnumerable<T>> GetAllAsync();
		Task<T> AddAsync(T entity);
		void Update(T entity);
		void Delete(T entity);
	}
}
