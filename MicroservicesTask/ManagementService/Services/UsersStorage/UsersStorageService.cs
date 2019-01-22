using ManagementService.Extensions;
using ManagementService.Models;
using ManagementService.Services.Request;
using ManagementService.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementService.Services.UsersStorage
{
    public class UsersStorageService : IUsersStorageService
    {
        private readonly ApiEndPoints _apiEndPoints;
        private readonly IRequestService _requestService;

        public UsersStorageService(
            IOptionsMonitor<AppSettings> options,
            IRequestService requestService)
        {
            _apiEndPoints = options.CurrentValue.ApiEndPoints;
            _requestService = requestService;
        }

        public Task<IEnumerable<User>> GetUsersAsync()
        {
            var builder = new UriBuilder(_apiEndPoints.UsersStorageService);
            builder.AppendToPath("Users");
            builder.AppendToPath("list");

            var uri = builder.ToString();

            return _requestService.GetAsync<IEnumerable<User>>(uri);
        }

        public Task<User> AddOrReplaceUserAsync(User user)
        {
            var builder = new UriBuilder(_apiEndPoints.UsersStorageService);
            builder.AppendToPath("Users");
            builder.AppendToPath("update");
            builder.AppendToPath(user.UserId);

            var uri = builder.ToString();

            return _requestService.PostAsync(uri, user);
        }

        public async Task DeleteUserAsync(User user)
        {
            var builder = new UriBuilder(_apiEndPoints.UsersStorageService);
            builder.AppendToPath("Users");
            builder.AppendToPath("delete");
            builder.AppendToPath(user.UserId);

            var uri = builder.ToString();

            await _requestService.DeleteAsync(uri);
        }
    }
}
