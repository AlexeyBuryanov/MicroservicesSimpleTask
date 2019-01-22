using ManagementService.Data.Repositories.Base;
using ManagementService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementService.Data.Repositories
{
    public interface IPermissionRepository : IRepositoryAsync<Permission>
    {
        Task<List<Permission>> GetAllWithIncludesAsync();
    }
}