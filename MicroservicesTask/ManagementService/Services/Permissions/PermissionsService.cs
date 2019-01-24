using ManagementService.Extensions;
using ManagementService.Models;
using ManagementService.Services.Request;
using ManagementService.Settings;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;

        public PermissionsService(
            IOptionsMonitor<AppSettings> options,
            IRequestService requestService,
            ILogger<PermissionsService> logger)
        {
            _apiEndPoints = options.CurrentValue.ApiEndPoints;
            _requestService = requestService;
            _logger = logger;
        }

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            _logger.LogInformation("--- GetPermissionsAsync()");

            var builder = new UriBuilder(_apiEndPoints.PermissionsService);
            builder.AppendToPath("Permissions");
            builder.AppendToPath("list");

            var uri = builder.ToString();

            return _requestService.GetAsync<IEnumerable<Permission>>(uri);
        }

        public Task<IEnumerable<Permission>> GetPermissionsForUserAsync(string userId)
        {
            _logger.LogInformation($"--- GetPermissionsAsync({userId})");

            var builder = new UriBuilder(_apiEndPoints.PermissionsService);
            builder.AppendToPath("Permissions");
            builder.AppendToPath("list");
            builder.AppendToPath(userId);

            var uri = builder.ToString();

            return _requestService.GetAsync<IEnumerable<Permission>>(uri);
        }

        public Task<Permission> AddOrReplacePermissionAsync(Permission permission)
        {
            _logger.LogInformation($"--- AddOrReplacePermissionAsync(Permission ID: {permission.PermissionId})");

            var builder = new UriBuilder(_apiEndPoints.PermissionsService);
            builder.AppendToPath("Permissions");
            builder.AppendToPath("update");
            builder.AppendToPath(permission.PermissionId);

            var uri = builder.ToString();

            return _requestService.PostAsync(uri, permission);
        }

        public async Task<bool> AssignPermissionAsync(string permissionId, string userId)
        {
            _logger.LogInformation($"--- AssignPermissionAsync({permissionId}, {userId})");

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
            _logger.LogInformation($"--- UnassignPermissionAsync({permissionId}, {userId})");

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
            _logger.LogInformation($"--- DeletePermissionAsync(Permission ID: {permission.PermissionId})");

            var builder = new UriBuilder(_apiEndPoints.PermissionsService);
            builder.AppendToPath("Permissions");
            builder.AppendToPath("delete");
            builder.AppendToPath(permission.PermissionId);

            var uri = builder.ToString();

            await _requestService.DeleteAsync(uri);
        }

        public async Task DeleteRemainingIndicesForUserAsync(User objForDelete)
        {
            _logger.LogInformation($"--- DeleteRemainingIndicesForUserAsync(User ID: {objForDelete.UserId})");

            var builder = new UriBuilder(_apiEndPoints.PermissionsService);
            builder.AppendToPath("Permissions");
            builder.AppendToPath("deleteIndicesForUser");
            builder.AppendToPath(objForDelete.UserId);

            var uri = builder.ToString();

            await _requestService.DeleteAsync(uri);
        }
    }
}
