using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PermissionsService.Data.Repositories.Base;
using PermissionsService.Models;
using PermissionsService.Services.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PermissionsService.Data.Repositories
{
    public class PermissionRepository 
        : IRepositoryAsync<Permission>, IPermissionRepositoryAsync
    {
        private readonly IPermissionsService _permissionsService;
        private readonly PermissionsContext _permissionsContext;
        private readonly ILogger _logger;

        public PermissionRepository(
            IPermissionsService permissionsService,
            IMongoDbContext permissionsContext,
            ILogger<PermissionRepository> logger)
        {
            _permissionsService = permissionsService;
            _permissionsContext = permissionsContext as PermissionsContext;
            _logger = logger;
        }


        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            await Task.FromResult(true);
            return _permissionsContext
                .PermissionsCollection
                .AsQueryable()
                .ToList();
        }


        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId)
        {
            // Результирующий список
            var result = new List<Permission>();

            if (string.IsNullOrWhiteSpace(userId))
            {
                return result;
            }

            try
            {
                // Список имеющихся разрешений
                var permissionsList = await _permissionsContext
                    .PermissionsCollection
                    .AsQueryable()
                    .ToListAsync();

                // Находим список разрешений по айди пользователя
                var cursor = await _permissionsContext
                    .UserPermissionsCollection
                    .FindAsync(p => p.UserId == userId);
                var userPermissions = await cursor.FirstAsync();

                // Если есть такой документ
                if (userPermissions != null)
                {
                    // Перебираем его список разрешений
                    await userPermissions.PermissionIds
                        .ToAsyncEnumerable()
                        .ForEachAsync(id =>
                        {
                            var first = permissionsList.First(permission => permission.PermissionId == id);
                            if (first != null)
                            {
                                // Каждое разрешение добавляем в результат
                                result.Add(first);
                            } // if
                        });

                    return result;
                } // if

                return result;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- GetUserPermissionsAsync() \n\n Reason:\n {e.Message}");
                _logger.LogDebug(1000, e, "------------------------------------------------------");
                return null;
            }
        }


        public async Task<Permission> AddOrReplaceAsync(string permissionId, Permission item)
        {
            if (string.IsNullOrWhiteSpace(permissionId) || item == null)
            {
                return null;
            }

            try
            {
                var cursor = await _permissionsContext
                    .PermissionsCollection
                    .FindAsync(filter: p => p.PermissionId == permissionId);
                var any = await cursor.AnyAsync();

                // Если такого пермишина нет, то добавляем, иначе обновляем
                if (!any)
                {
                    // Учитываем указанный id. id в объекте item игнорируется, как должно было быть по заданию.
                    // Но я бы не передавал сюда Id т.к. объект item и так содержит в себе Id.
                    item.PermissionId = permissionId;
                    await _permissionsContext
                        .PermissionsCollection
                        .InsertOneAsync(document: item);
                }
                else
                {
                    var permission = _permissionsContext
                        .PermissionsCollection
                        .AsQueryable()
                        .ToList()
                        .Find(p => p.PermissionId == permissionId);

                    item.PermissionId = permissionId;
                    await _permissionsContext
                        .PermissionsCollection
                        .ReplaceOneAsync(permission.ToBsonDocument(), item);
                } // if
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- AddOrReplaceAsync() \n\n Reason:\n {e.Message}");
                _logger.LogDebug(1000, e, "------------------------------------------------------");
                return null;
            }

            return item;
        }


        public async Task DeleteAsync(string permissionId)
        {
            if (string.IsNullOrWhiteSpace(permissionId))
            {
                return;
            }

            try
            {
                var permission = _permissionsContext
                    .PermissionsCollection
                    .AsQueryable()
                    .ToList()
                    .Find(p => p.PermissionId == permissionId);

                if (permission != null)
                {
                    // Удаляем сам пермишинс
                    await _permissionsContext
                        .PermissionsCollection
                        .DeleteOneAsync(permission.ToBsonDocument());

                    // Удаляем ссылающиеся на него индексы

                    // PermissionUsers
                    await _permissionsService.DeletePermissionUsersDocAsync(permissionId);
                    // UserPermissions
                    await _permissionsService.UpdateUserPermissionsDocAsync(permissionId);
                } // if
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- DeleteAsync() \n\n Reason:\n {e.Message}");
                _logger.LogDebug(1000, e, "------------------------------------------------------");
            }
        }


        public async Task<bool> AssignPermissionAsync(string permissionId, string userId)
        {
            if (string.IsNullOrWhiteSpace(permissionId) || string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            var operation1IsOk = await _permissionsService.AssignUserPermissionsAsync(permissionId, userId);
            var operation2IsOk = await _permissionsService.AssignPermissionUsersAsync(permissionId, userId);

            return operation1IsOk && operation2IsOk;
        }


        public async Task<bool> UnassignPermissionAsync(string permissionId, string userId)
        {
            if (string.IsNullOrWhiteSpace(permissionId) || string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            var operation1IsOk = await _permissionsService.UnassignUserPermissionsAsync(permissionId, userId);
            var operation2IsOk = await _permissionsService.UnassignPermissionUsersAsync(permissionId, userId);

            return operation1IsOk && operation2IsOk;
        }


        public async Task<bool> DeleteUserRemainingIndices(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            // UserPermissions
            await _permissionsService.DeleteUserPermissionsDocAsync(userId);
            // PermissionUsers
            await _permissionsService.UpdatePermissionUsersDocAsync(userId);

            return true;
        }


        public async Task<bool> AnyAsync()
        {
            try
            {
                var anys = await Task.WhenAll(
                    _permissionsContext
                        .PermissionsCollection
                        .AsQueryable()
                        .AnyAsync(),
                    _permissionsContext
                        .UserPermissionsCollection
                        .AsQueryable()
                        .AnyAsync(),
                    _permissionsContext
                        .PermissionUsersCollection
                        .AsQueryable()
                        .AnyAsync());

                return anys.ToList()
                    .TrueForAll(b => b.Equals(true));
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- AnyAsync() \n\n Reason:\n {e.Message}");
                _logger.LogDebug(1000, e, "------------------------------------------------------");
                return false;
            }
        } // AnyAsync
    }
}
