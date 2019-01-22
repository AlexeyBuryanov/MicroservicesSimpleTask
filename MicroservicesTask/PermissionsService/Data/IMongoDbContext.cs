using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;

namespace PermissionsService.Data
{
    public interface IMongoDbContext : IDisposable
    {
        /// <summary>
        /// База данных
        /// </summary>
        IMongoDatabase MongoDatabase { get; set; }

        /// <summary>
        /// Файловое хранилище
        /// </summary>
        IGridFSBucket GridFsBucket { get; set; }

        /// <summary>
        /// Проверяет наличие коллекции и создаёт её при отсутствии
        /// </summary>
        void CreateCollAsync(string name);
    }
}