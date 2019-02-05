using System.Threading.Tasks;

namespace PermissionsService.Services.Permissions
{
    public interface IPermissionsService
    {
        Task DeletePermissionUsersDocAsync(string permissionId);
        Task DeleteUserPermissionsDocAsync(string userId);
        Task UpdateUserPermissionsDocAsync(string permissionId);
        Task UpdatePermissionUsersDocAsync(string userId);
        Task<bool> AssignUserPermissionsAsync(string permissionId, string userId);
        Task<bool> AssignPermissionUsersAsync(string permissionId, string userId);
        Task<bool> UnassignUserPermissionsAsync(string permissionId, string userId);
        Task<bool> UnassignPermissionUsersAsync(string permissionId, string userId);
    }
}
