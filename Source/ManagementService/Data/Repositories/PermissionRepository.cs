using ManagementService.Data.Repositories.Base;
using ManagementService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementService.Data.Repositories
{
    public class PermissionRepository : RepositoryAsync<Permission>, IPermissionRepository
    {
        public PermissionRepository(DbContext dbContext) 
            : base(dbContext)
        {
        }

        public ManagementDbContext ManagementDbContext
            => DbContext as ManagementDbContext;

        public Task<List<Permission>> GetAllWithIncludesAsync()
        {
            return ManagementDbContext
                .Permissions
                .Include(u => u.BindingEntities).ThenInclude(be => be.Permission)
                .ToListAsync();
        }
    }
}
