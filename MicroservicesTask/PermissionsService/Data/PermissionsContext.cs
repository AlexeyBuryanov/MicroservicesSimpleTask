using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PermissionsService.Models;
using PermissionsService.Settings;

namespace PermissionsService.Data
{
    public class PermissionsContext : MongoDbContext
    {
        public PermissionsContext(
            IOptionsMonitor<AppSettings> options) 
            : base(options)
        {
            CreateCollAsync("Permissions");
            CreateCollAsync("PermissionUsers");
            CreateCollAsync("UserPermissions");

            PermissionsCollection = MongoDatabase.GetCollection<Permission>(name: "Permissions");
            PermissionUsersCollection = MongoDatabase.GetCollection<PermissionUsers>(name: "PermissionUsers");
            UserPermissionsCollection = MongoDatabase.GetCollection<UserPermissions>(name: "UserPermissions");
        }

        public IMongoCollection<Permission> PermissionsCollection { get; }
        public IMongoCollection<PermissionUsers> PermissionUsersCollection { get; }
        public IMongoCollection<UserPermissions> UserPermissionsCollection { get; }
    }
}
