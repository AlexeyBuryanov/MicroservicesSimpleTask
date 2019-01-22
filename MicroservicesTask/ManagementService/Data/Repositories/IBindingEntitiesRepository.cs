using ManagementService.Data.Repositories.Base;
using ManagementService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementService.Data.Repositories
{
    public interface IBindingEntitiesRepository : IRepositoryAsync<BindingEntity>
    {
        Task<List<BindingEntity>> GetAllWithIncludesAsync();
    }
}