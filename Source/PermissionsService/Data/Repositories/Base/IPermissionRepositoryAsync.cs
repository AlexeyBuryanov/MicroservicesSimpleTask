using PermissionsService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PermissionsService.Data.Repositories.Base
{
    public interface IPermissionRepositoryAsync
    {
        Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId);

        Task<bool> AssignPermissionAsync(string permissionId, string userId);

        Task<bool> UnassignPermissionAsync(string permissionId, string userId);

        Task<bool> DeleteUserRemainingIndices(string userId);
    }
}
