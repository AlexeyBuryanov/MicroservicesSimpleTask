using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UsersStorageService.Data.Repositories.Base;
using UsersStorageService.Models;

namespace UsersStorageService.Data.Repositories
{
    public class UserRepository : IRepositoryAsync<User>
    {
        private readonly UsersContext _usersContext;

        public UserRepository(IMongoDbContext usersContext)
        {
            _usersContext = usersContext as UsersContext;
        }

        public Task<IEnumerable<User>> GetAllAsync()
        {
            return Task.FromResult(
                _usersContext
                    .UsersCollection
                    .AsQueryable()
                    .ToEnumerable());
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
                Debug.WriteLine("Ошибка! Класс - UserRepository, метод - AddOrReplaceAsync.\n\n" +
                                      $"Причина:\n {e.Message}");
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
                Debug.WriteLine("Ошибка! Класс - UserRepository, метод - DeleteAsync.\n\n" +
                                      $"Причина:\n {e.Message}");
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
