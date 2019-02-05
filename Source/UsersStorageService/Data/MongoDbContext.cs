using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using UsersStorageService.Settings;

namespace UsersStorageService.Data
{
    public abstract class MongoDbContext : IMongoDbContext
    {
        private readonly ILogger _logger;

        public IMongoDatabase MongoDatabase { get; set; }
        public IGridFSBucket GridFsBucket { get; set; }

        protected MongoDbContext(
            IOptionsMonitor<AppSettings> options,
            ILogger<MongoDbContext> logger)
        {
            _logger = logger;

            var connectionString = options.CurrentValue.ConnectionStrings.DefaultConnection;
            var connection = new MongoUrlBuilder(url: connectionString);

            // Получаем клиента для взаимодействия с базой данных
            var client = new MongoClient(connectionString: connectionString);

            // Получаем доступ к самой базе данных
            MongoDatabase = client.GetDatabase(name: connection.DatabaseName);

            // Получаем доступ к файловому хранилищу
            GridFsBucket = new GridFSBucket(database: MongoDatabase);

            _logger.LogInformation("--- Create MongoDbContext");
        }

        public async void CreateCollAsync(string name)
        {
            var filter = new BsonDocument(name: "name", value: name);
            var options = new ListCollectionNamesOptions { Filter = filter };
            var col = await MongoDatabase.ListCollectionNamesAsync(options);
            var any = await col.AnyAsync();
            if (!any)
            {
                await MongoDatabase.CreateCollectionAsync(name: name);
                _logger.LogInformation($"--- Create collection with name \"{name}\"");
            }
        }


        private bool _disposed = false;
        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    MongoDatabase.Client.Cluster.Dispose();
                }
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
