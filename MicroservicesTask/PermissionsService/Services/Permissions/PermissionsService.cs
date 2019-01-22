using MongoDB.Bson;
using MongoDB.Driver;
using PermissionsService.Data;
using PermissionsService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PermissionsService.Services.Permissions
{
    public class PermissionsService : IPermissionsService
    {
        private readonly PermissionsContext _permissionsContext;

        public PermissionsService(IMongoDbContext permissionsContext)
        {
            _permissionsContext = permissionsContext as PermissionsContext;
        }


        /* Delete
         ----------------------------------------------------------------------
         */
        public async Task DeletePermissionUsersDocAsync(string permissionId)
        {
            await _permissionsContext
                .PermissionUsersCollection
                .AsQueryable()
                .ToAsyncEnumerable()
                .ForEachAsync(async pu =>
                {
                    if (pu.PermissionId == permissionId)
                    {
                        await _permissionsContext
                            .PermissionUsersCollection
                            .DeleteOneAsync(pu.ToBsonDocument());
                    } // if
                });
        }

        public async Task UpdateUserPermissionsDocAsync(string permissionId)
        {
            await _permissionsContext
                .UserPermissionsCollection
                .AsQueryable()
                .ToAsyncEnumerable()
                .ForEachAsync(up =>
                {
                    up.PermissionIds
                        .ForEach(async id =>
                        {
                            if (id == permissionId)
                            {
                                var upfOld = _permissionsContext
                                    .UserPermissionsCollection
                                    .AsQueryable()
                                    .ToList()
                                    .First(u => u.PermissionIds.Find(s => s == permissionId) == permissionId)
                                    .ToBsonDocument();
                                var upfNew = _permissionsContext
                                    .UserPermissionsCollection
                                    .AsQueryable()
                                    .ToList()
                                    .First(u => u.PermissionIds.Find(s => s == permissionId) == permissionId);
                                if (upfNew.PermissionIds.Exists(i => i == permissionId))
                                {
                                    // Обновляем список, если он не пустой
                                    upfNew.PermissionIds.Remove(permissionId);
                                    if (upfNew.PermissionIds.Count != 0)
                                    {
                                        await _permissionsContext
                                            .UserPermissionsCollection
                                            .ReplaceOneAsync(upfOld, upfNew);
                                    }
                                    // Иначе удаляем запись
                                    else
                                    {
                                        await _permissionsContext
                                            .UserPermissionsCollection
                                            .DeleteOneAsync(upfOld);
                                    }
                                }
                            }
                        });
                });
        }


        /* Assign
         ----------------------------------------------------------------------
         */
        public async Task<bool> AssignUserPermissionsAsync(string permissionId, string userId)
        {
            try
            {
                var userPermissionsList = await _permissionsContext
                    .UserPermissionsCollection
                    .AsQueryable()
                    .ToListAsync();

                // Insert ---------------------------------------------------------
                // Если для такого юзера не было пермишинсов создаём новый список
                if (!userPermissionsList.Exists(up => up.UserId == userId))
                {
                    var cursor = await _permissionsContext
                        .UserPermissionsCollection.FindAsync(u => u.UserId == userId);
                    var any = await cursor.AnyAsync();
                    if (!any)
                    {
                        var up = new UserPermissions
                        {
                            UserId = userId,
                            PermissionIds = new List<string> { permissionId }
                        };
                        await _permissionsContext
                            .UserPermissionsCollection
                            .InsertOneAsync(up);

                        return true;
                    }
                }
                // Update ---------------------------------------------------------
                // Иначе добавляем новые данные к старым и обновляем их в базе
                else
                {
                    var upfOld = userPermissionsList.First(up => up.UserId == userId).ToBsonDocument();
                    var upfNew = userPermissionsList.First(up => up.UserId == userId);
                    if (!upfNew.PermissionIds.Exists(id => id == permissionId))
                    {
                        upfNew.PermissionIds.Add(permissionId);
                        await _permissionsContext
                            .UserPermissionsCollection
                            .ReplaceOneAsync(upfOld, upfNew);

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка! Класс - PermissionRepository, метод - AssignUserPermissionsAsync.\n\n" +
                                      $"Причина:\n {e.Message}");
                return false;
            }

            return false;
        }

        public async Task<bool> AssignPermissionUsersAsync(string permissionId, string userId)
        {
            try
            {
                var permissionsUsersList = await _permissionsContext
                    .PermissionUsersCollection
                    .AsQueryable()
                    .ToListAsync();

                // Insert ---------------------------------------------------------
                // Если для такого пермишина не было юзеров создаём новый список
                if (!permissionsUsersList.Exists(up => up.PermissionId == permissionId))
                {
                    var cursor = await _permissionsContext
                        .PermissionUsersCollection.FindAsync(u => u.PermissionId == permissionId);
                    var any = await cursor.AnyAsync();
                    if (!any)
                    {
                        var up = new PermissionUsers
                        {
                            PermissionId = permissionId,
                            UserIds = new List<string> { userId }
                        };
                        await _permissionsContext
                            .PermissionUsersCollection
                            .InsertOneAsync(up);

                        return true;
                    }
                }
                // Update ---------------------------------------------------------
                // Иначе добавляем новые данные к старым и обновляем их в базе
                else
                {
                    var pufOld = permissionsUsersList.First(up => up.PermissionId == permissionId).ToBsonDocument();
                    var pufNew = permissionsUsersList.First(up => up.PermissionId == permissionId);
                    if (!pufNew.UserIds.Exists(id => id == userId))
                    {
                        pufNew.UserIds.Add(userId);
                        await _permissionsContext
                            .PermissionUsersCollection
                            .ReplaceOneAsync(pufOld, pufNew);

                        return true;
                    }
                } // if
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка! Класс - PermissionRepository, метод - AssignPermissionUsersAsync.\n\n" +
                                      $"Причина:\n {e.Message}");
                return false;
            }

            return false;
        }


        /* Unassign
         ----------------------------------------------------------------------
         */
        public async Task<bool> UnassignUserPermissionsAsync(string permissionId, string userId)
        {
            try
            {
                var userPermissionsList = await _permissionsContext
                    .UserPermissionsCollection
                    .AsQueryable()
                    .ToListAsync();

                // Если для такого юзера не было пермишинсов, то и снимать нечего - уходим
                if (!userPermissionsList.Exists(up => up.UserId == userId))
                {
                    return false;
                }

                // Иначе отнимаем разрешение у пользователя, если оно есть
                var upfOld = userPermissionsList.First(up => up.UserId == userId).ToBsonDocument();
                var upfNew = userPermissionsList.First(up => up.UserId == userId);
                if (upfNew.PermissionIds.Exists(id => id == permissionId))
                {
                    upfNew.PermissionIds.Remove(permissionId);
                    await _permissionsContext
                        .UserPermissionsCollection
                        .ReplaceOneAsync(upfOld, upfNew);

                    return true;
                } // if
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка! Класс - PermissionRepository, метод - UnassignUserPermissionsAsync.\n\n" +
                                      $"Причина:\n {e.Message}");
                return false;
            }

            return false;
        }

        public async Task<bool> UnassignPermissionUsersAsync(string permissionId, string userId)
        {
            try
            {
                var permissionsUsersList = await _permissionsContext
                    .PermissionUsersCollection
                    .AsQueryable()
                    .ToListAsync();

                // Если для такого пермишина не было юзеров, то и разрешений для снятия нет
                if (!permissionsUsersList.Exists(up => up.PermissionId == permissionId))
                {
                    return false;
                }

                // Иначе отнимаем у пользователя разрешение, если оно есть
                var pufOld = permissionsUsersList.First(up => up.PermissionId == permissionId).ToBsonDocument();
                var pufNew = permissionsUsersList.First(up => up.PermissionId == permissionId);
                if (pufNew.UserIds.Exists(id => id == userId))
                {
                    pufNew.UserIds.Remove(userId);
                    await _permissionsContext
                        .PermissionUsersCollection
                        .ReplaceOneAsync(pufOld, pufNew);

                    return true;
                } // if
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка! Класс - PermissionRepository, метод - UnassignPermissionUsersAsync.\n\n" +
                                      $"Причина:\n {e.Message}");
                return false;
            }

            return false;
        } // UnassignPermissionUsersAsync
    }
}
