using ManagementService.Data.Repositories.Base;
using ManagementService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementService.Data.Repositories
{
    public class BindingEntitiesRepository : RepositoryAsync<BindingEntity>, IBindingEntitiesRepository
    {
        public BindingEntitiesRepository(DbContext dbContext) 
            : base(dbContext)
        {
        }

        public ManagementDbContext ManagementDbContext
            => DbContext as ManagementDbContext;

        public Task<List<BindingEntity>> GetAllWithIncludesAsync()
        {
            return ManagementDbContext
                .BindingEntities
                .Include(be => be.User)
                .Include(be => be.Permission)
                .ToListAsync();
        }
    }
}
