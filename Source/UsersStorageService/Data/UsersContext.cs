using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UsersStorageService.Models;
using UsersStorageService.Settings;

namespace UsersStorageService.Data
{
    public class UsersContext : MongoDbContext
    {
        public UsersContext(
            IOptionsMonitor<AppSettings> options,
            ILogger<UsersContext> logger) 
            : base(options, logger)
        {
            CreateCollAsync("Users");
            UsersCollection = MongoDatabase.GetCollection<User>(name: "Users");
        }

        public IMongoCollection<User> UsersCollection { get; }
    }
}
