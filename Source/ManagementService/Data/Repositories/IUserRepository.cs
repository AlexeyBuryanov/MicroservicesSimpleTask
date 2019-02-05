using ManagementService.Data.Repositories.Base;
using ManagementService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementService.Data.Repositories
{
    public interface IUserRepository : IRepositoryAsync<User>
    {
        Task<List<User>> GetAllWithIncludesAsync();
    }
}