using ManagementService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementService.Services.UsersStorage
{
    public interface IUsersStorageService
    {
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User> AddOrReplaceUserAsync(User user);
        Task DeleteUserAsync(User user);
    }
}
