using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ManagementService.Data.Repositories.Base
{
    public interface IRepositoryAsync<T> where T : class
    {
        Task<T> GetAsync(string id);
        Task<List<T>> GetAllAsync();
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T item);
        void Add(T item);
        Task AddRangeAsync(IEnumerable<T> items);

        Task UpdateAsync(T item);
        Task UpdateRangeAsync(IEnumerable<T> items);

        Task DeleteAsync(T item);
        Task DeleteRangeAsync(IEnumerable<T> items);

        Task<bool> AnyAsync();
    }
}
