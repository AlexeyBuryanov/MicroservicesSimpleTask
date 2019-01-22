using System.Collections.Generic;
using System.Threading.Tasks;

namespace UsersStorageService.Data.Repositories.Base
{
    public interface IRepositoryAsync<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();

        Task<T> AddOrReplaceAsync(string id, T item);

        Task DeleteAsync(string id);

        Task<bool> AnyAsync();
    }
}
