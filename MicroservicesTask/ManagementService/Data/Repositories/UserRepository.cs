using ManagementService.Data.Repositories.Base;
using ManagementService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementService.Data.Repositories
{
    public class UserRepository : RepositoryAsync<User>, IUserRepository
    {
        public UserRepository(DbContext dbContext) 
            : base(dbContext)
        {
        }

        public ManagementDbContext ManagementDbContext
            => DbContext as ManagementDbContext;

        public Task<List<User>> GetAllWithIncludesAsync()
        {
            return ManagementDbContext
                .Users
                .Include(u => u.BindingEntities).ThenInclude(be => be.User)
                .ToListAsync();
        }
    }
}
