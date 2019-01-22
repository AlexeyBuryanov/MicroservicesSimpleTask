using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ManagementService.Data.Repositories.Base
{
    public class RepositoryAsync<T> : IRepositoryAsync<T> where T : class
    {
        protected readonly DbContext DbContext;

        public RepositoryAsync(DbContext dbContext)
        {
            DbContext = dbContext;
        }

        public Task<T> GetAsync(string id)
        {
            return DbContext.Set<T>()
                .FindAsync(id);
        }

        public Task<List<T>> GetAllAsync()
        {
            return DbContext.Set<T>()
                .ToListAsync();
        }

        public Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return DbContext.Set<T>()
                .Where(predicate)
                .ToListAsync();
        }

        public Task AddAsync(T item)
        {
            return DbContext.Set<T>()
                .AddAsync(item);
        }

        public void Add(T item)
        {
            DbContext.Set<T>()
                .Add(item);
        }

        public Task AddRangeAsync(IEnumerable<T> items)
        {
            return DbContext.Set<T>()
                .AddRangeAsync(items);
        }

        public async Task UpdateAsync(T item)
        {
            DbContext.Entry(item)
                .State = EntityState.Modified;

            await Task.FromResult(true);
        }

        public async Task UpdateRangeAsync(IEnumerable<T> items)
        {
            DbContext.Set<T>()
                .UpdateRange(items);

            await Task.FromResult(true);
        }

        public async Task DeleteAsync(T item)
        {
            DbContext.Set<T>()
                .Remove(item);

            await Task.FromResult(true);
        }

        public async Task DeleteRangeAsync(IEnumerable<T> items)
        {
            DbContext.Set<T>()
                .RemoveRange(items);

            await Task.FromResult(true);
        }

        public Task<bool> AnyAsync()
        {
            return DbContext.Set<T>()
                .AnyAsync();
        }
    }
}
