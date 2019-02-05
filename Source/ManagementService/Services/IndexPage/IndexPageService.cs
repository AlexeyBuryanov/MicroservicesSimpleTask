using ManagementService.Data;
using ManagementService.Models;
using ManagementService.Pages;
using ManagementService.Services.Permissions;
using ManagementService.Services.UsersStorage;
using ManagementService.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementService.Services.IndexPage
{
    public class IndexPageService : IIndexPageService
    {
        private readonly IUsersStorageService _usersStorageService;
        private readonly IPermissionsService _permissionsService;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _logger;

        public IndexPageService(
            IUsersStorageService usersStorageService,
            IPermissionsService permissionsService,
            IUnitOfWork unitOfWork,
            ILogger<IndexModel> logger)
        {
            _usersStorageService = usersStorageService;
            _permissionsService = permissionsService;
            _uow = unitOfWork;
            _logger = logger;
        }

        public async Task CheckForUpdatesAsync()
        {
            try
            {
                await _uow.EnsureCreatedAsync();

                var permissionsFromService = (await _permissionsService.GetPermissionsAsync()).ToList();
                var permissionsFromCache = (await _uow.Permissions.GetAllAsync()).ToList();

                if (permissionsFromService.Count != permissionsFromCache.Count)
                {
                    // Добавляем разрешения во внутренюю базу
                    await _uow.Permissions.AddRangeAsync(permissionsFromService);
                    await _uow.SaveChangesAsync();
                } // if

                var usersFromService = (await _usersStorageService.GetUsersAsync()).ToList();
                var usersFromCache = (await _uow.Users.GetAllWithIncludesAsync()).ToList();

                // Обновляем информацию в кэше при необходимости
                if (usersFromService.Count != usersFromCache.Count)
                {
                    // Проходим по пользователям
                    await usersFromService.ToAsyncEnumerable().ForEachAsync(async user =>
                    {
                        // Добавляем пользователя
                        await _uow.Users.AddAsync(user);

                        // Вытягиваем для каждого юзера разрешения из сервиса (должно выполняться синхронно)
                        // TODO: плохое решение с точки зрения оптимизации
                        if (_permissionsService.GetPermissionsForUserAsync(user.UserId)
                            .GetAwaiter().GetResult() is List<Permission> permissionsForUser)
                            // Проходим по разрешениям попутно формируя данные для добавления
                            await permissionsForUser.ToAsyncEnumerable().ForEachAsync(async permission =>
                            {
                                var entityToAdd = new BindingEntity
                                {
                                    UserId = user.UserId,
                                    PermissionId = permission.PermissionId
                                };

                                // Добавляем только тогда, если такой записи не содержится
                                if (!(await _uow.BindingEntities
                                        .FindAsync(e =>
                                            e.UserId != user.UserId && e.PermissionId != permission.PermissionId))
                                    .Contains(entityToAdd))
                                {
                                    await _uow.BindingEntities.AddAsync(entityToAdd);
                                } // if
                            });
                    });
                    await _uow.SaveChangesAsync();
                } // if
            }
            catch (Exception e)
            {
                // TODO: создать специальный класс для ивентов
                _logger.LogWarning($"--- CheckForUpdatesAsync() \n\n Reason:\n {e.Message}");
                _logger.LogDebug(2000, e, "------------------------------------------------------");
            }
        }

        public async Task<List<UserViewModel>> GetUsersViewModelsAsync()
        {
            var users = new List<UserViewModel>();

            try
            {
                await (await _uow.Users.GetAllWithIncludesAsync())
                    .AsParallel()
                    .WithDegreeOfParallelism(Environment.ProcessorCount)
                    .OrderBy(u => u.UserId)
                    .ToAsyncEnumerable()
                    .ForEachAsync(user =>
                    {
                        var userToAdd = new UserViewModel
                        {
                            UserId = user.UserId,
                            Password = user.Password,
                            Email = user.Email,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Permissions = "No permissions"
                        };
                        var permissions = user.BindingEntities.Select(be => be.Permission).ToList();
                        var sb = new StringBuilder();
                        permissions.ForEach(p =>
                        {
                            sb.Append($"{p.Name}, ");
                        });
                        if (!string.IsNullOrWhiteSpace(sb.ToString()))
                        {
                            userToAdd.Permissions = sb.ToString().Substring(0, sb.ToString().Length - 2);
                        }

                        users.Add(userToAdd);
                    });

                return users;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- GetUsersViewModelsAsync() \n\n Reason:\n {e.Message}");
                _logger.LogDebug(2000, e, "------------------------------------------------------");
                return users;
            }
        }

        public async Task<List<PermissionViewModel>> GetPermissionsViewModelsAsync()
        {
            var permissions = new List<PermissionViewModel>();

            try
            {
                await (await _uow.Permissions.GetAllWithIncludesAsync())
                    .AsParallel()
                    .WithDegreeOfParallelism(Environment.ProcessorCount)
                    .OrderBy(u => u.PermissionId)
                    .ToAsyncEnumerable()
                    .ForEachAsync(permission =>
                    {
                        var permissionToAdd = new PermissionViewModel
                        {
                            PermissionId = permission.PermissionId,
                            Name = permission.Name
                        };
                        permissions.Add(permissionToAdd);
                    });

                return permissions;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- GetPermissionsViewModelsAsync() \n\n Reason:\n {e.Message}");
                _logger.LogDebug(2000, e, "------------------------------------------------------");
                return permissions;
            }
        }

        public async Task<List<ManagementViewModel>> GetManagementViewModelsAsync()
        {
            var management = new List<ManagementViewModel>();

            try
            {
                await (await _uow.Users.GetAllWithIncludesAsync())
                    .AsParallel()
                    .WithDegreeOfParallelism(Environment.ProcessorCount)
                    .OrderBy(u => u.UserId)
                    .ToAsyncEnumerable()
                    .ForEachAsync(user =>
                    {
                        var permissions = user.BindingEntities.Select(be => be.Permission).ToList();
                        permissions.ForEach(p =>
                        {
                            management.Add(new ManagementViewModel
                            {
                                Id = Guid.NewGuid().ToString(),
                                Guid = Guid.NewGuid().ToString(),
                                User = new UserViewModel
                                {
                                    FullNameUser = $"{user.FirstName} {user.LastName}",
                                    FirstName = user.FirstName,
                                    LastName = user.LastName,
                                    UserId = user.UserId,
                                    Email = user.Email,
                                    Password = user.Password
                                },
                                Permission = new PermissionViewModel
                                {
                                    PermissionId = p.PermissionId,
                                    Name = p.Name
                                }
                            });
                        });
                    });

                return management;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- GetManagementViewModelsAsync() \n\n Reason:\n {e.Message}");
                _logger.LogDebug(2000, e, "------------------------------------------------------");
                return management;
            }
        }
    }
}