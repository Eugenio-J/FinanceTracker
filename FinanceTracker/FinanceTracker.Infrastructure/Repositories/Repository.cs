using FinanceTracker.Domain.Interfaces;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Infrastructure.Repositories
{
	public class Repository<T> : IRepository<T> where T : class
	{
		protected readonly DataContext _dataContext;
		protected readonly DbSet<T> _dbSet;

		public Repository(DataContext context)
		{
			_dataContext = context;
			_dbSet = context.Set<T>();
		}

		public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

		public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

		public async Task<T> AddAsync(T entity)
		{
			await _dbSet.AddAsync(entity);
			return entity;
		}

		public void Update(T entity) => _dbSet.Update(entity);

		public void Delete(T entity) => _dbSet.Remove(entity);
	}
}
