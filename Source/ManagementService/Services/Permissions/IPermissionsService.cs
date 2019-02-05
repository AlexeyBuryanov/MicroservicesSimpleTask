using ManagementService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementService.Services.Permissions
{
    public interface IPermissionsService
    {
        Task<IEnumerable<Permission>> GetPermissionsAsync();
        Task<IEnumerable<Permission>> GetPermissionsForUserAsync(string userId);

        Task<Permission> AddOrReplacePermissionAsync(Permission permission);
        Task<bool> AssignPermissionAsync(string permissionId, string userId);
        Task<bool> UnassignPermissionAsync(string permissionId, string userId);

        Task DeletePermissionAsync(Permission permission);
        Task DeleteRemainingIndicesForUserAsync(User objForDelete);
    }
}
