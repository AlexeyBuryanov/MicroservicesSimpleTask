using ManagementService.Extensions;
using ManagementService.Models;
using ManagementService.Services.Request;
using ManagementService.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementService.Services.Permissions
{
    public class PermissionsService : IPermissionsService
    {
        private readonly ApiEndPoints _apiEndPoints;
        private readonly IRequestService _requestService;

        public PermissionsService(
            IOptionsMonitor<AppSettings> options,
            IRequestService requestService)
        {
            _apiEndPoints = options.CurrentValue.ApiEndPoints;
            _requestService = requestService;
        }

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            var builder = new UriBuilder(_apiEndPoints.PermissionsService);
            builder.AppendToPath("Permissions");
            builder.AppendToPath("list");

            var uri = builder.ToString();

            return _requestService.GetAsync<IEnumerable<Permission>>(uri);
        }

        public Task<IEnumerable<Permission>> GetPermissionsForUserAsync(string userId)
        {
            var builder = new UriBuilder(_apiEndPoints.PermissionsService);
            builder.AppendToPath("Permissions");
            builder.AppendToPath("list");
            builder.AppendToPath(userId);

            var uri = builder.ToString();

            return _requestService.GetAsync<IEnumerable<Permission>>(uri);
        }

        public Task<Permission> AddOrReplacePermissionAsync(Permission permission)
        {
            var builder = new UriBuilder(_apiEndPoints.PermissionsService);
            builder.AppendToPath("Permissions");
            builder.AppendToPath("update");
            builder.AppendToPath(permission.PermissionId);

            var uri = builder.ToString();

            return _requestService.PostAsync(uri, permission);
        }

        public async Task<bool> AssignPermissionAsync(string permissionId, string userId)
        {
            var builder = new UriBuilder(_apiEndPoints.PermissionsService);
            builder.AppendToPath("Permissions");
            builder.AppendToPath("assign");
            builder.AppendToPath(permissionId);
            builder.AppendToPath(userId);

            var uri = builder.ToString();

            return await _requestService.PostAsync<bool>(uri);
        }

        public async Task<bool> UnassignPermissionAsync(string permissionId, string userId)
        {
            var builder = new UriBuilder(_apiEndPoints.PermissionsService);
            builder.AppendToPath("Permissions");
            builder.AppendToPath("unassign");
            builder.AppendToPath(permissionId);
            builder.AppendToPath(userId);

            var uri = builder.ToString();

            return await _requestService.PostAsync<bool>(uri);
        }

        public async Task DeletePermissionAsync(Permission permission)
        {
            var builder = new UriBuilder(_apiEndPoints.PermissionsService);
            builder.AppendToPath("Permissions");
            builder.AppendToPath("delete");
            builder.AppendToPath(permission.PermissionId);

            var uri = builder.ToString();

            await _requestService.DeleteAsync(uri);
        }
    }
}
