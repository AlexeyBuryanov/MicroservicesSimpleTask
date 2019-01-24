using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UsersStorageService.Data.Repositories.Base;
using UsersStorageService.Models;

namespace UsersStorageService.Data.Repositories
{
    public class UserRepository : IRepositoryAsync<User>
    {
        private readonly UsersContext _usersContext;
        private readonly ILogger _logger;

        public UserRepository(
            IMongoDbContext usersContext,
            ILogger<UserRepository> logger)
        {
            _usersContext = usersContext as UsersContext;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            await Task.FromResult(true);
            return _usersContext
                .UsersCollection
                .AsQueryable()
                .ToList();
        }

        public async Task<User> AddOrReplaceAsync(string userId, User item)
        {
            if (string.IsNullOrWhiteSpace(userId) || item == null)
            {
                return null;
            }

            try
            {
                var cursor = await _usersContext
                    .UsersCollection
                    .FindAsync(filter: user => user.UserId == userId);
                var any = await cursor.AnyAsync();

                // Если такого юзера нет, то добавляем, иначе обновляем
                if (!any)
                {
                    // Учитываем указанный id. id в объекте item игнорируется, как должно было быть по заданию.
                    // Но я бы не передавал сюда userId т.к. объект item и так содержит в себе userId.
                    item.UserId = userId;
                    await _usersContext
                        .UsersCollection
                        .InsertOneAsync(document: item);
                }
                else
                {
                    var user = _usersContext
                        .UsersCollection
                        .AsQueryable()
                        .ToList()
                        .Find(u => u.UserId == userId);

                    item.UserId = userId;
                    await _usersContext
                        .UsersCollection
                        .ReplaceOneAsync(user.ToBsonDocument(), item);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- AddOrReplaceAsync() \n\n Reason:\n {e.Message}");
                _logger.LogDebug(1000, e, "------------------------------------------------------");
                return null;
            }

            return item;
        }

        public async Task DeleteAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            try
            {
                var user = _usersContext
                    .UsersCollection
                    .AsQueryable()
                    .ToList()
                    .Find(u => u.UserId == userId);

                await _usersContext
                    .UsersCollection
                    .DeleteOneAsync(user.ToBsonDocument());
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- DeleteAsync() \n\n Reason:\n {e.Message}");
                _logger.LogDebug(1000, e, "------------------------------------------------------");
            }
        }

        public async Task<bool> AnyAsync()
        {
            return await _usersContext
                .UsersCollection
                .AsQueryable()
                .AnyAsync();
        }

        public IMongoDbContext GetContextDb()
        {
            return _usersContext;
        }
    }
}
