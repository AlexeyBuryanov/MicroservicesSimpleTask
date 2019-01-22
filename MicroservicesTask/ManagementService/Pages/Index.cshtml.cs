using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ManagementService.Data;
using ManagementService.Models;
using ManagementService.Services.Permissions;
using ManagementService.Services.UsersStorage;
using ManagementService.Utils;
using ManagementService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementService.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IUsersStorageService _usersStorageService;
        private readonly IPermissionsService _permissionsService;
        private readonly IUnitOfWork _uow;

        private static IList<UserViewModel> _users;
        private static IList<PermissionViewModel> _permissions;
        private static IList<ManagementViewModel> _management;

        public IndexModel(
            IUsersStorageService usersStorageService,
            IPermissionsService permissionsService,
            IUnitOfWork unitOfWork)
        {
            _usersStorageService = usersStorageService;
            _permissionsService = permissionsService;
            _uow = unitOfWork;
        }

        public async Task OnGet()
        {
            await _uow.EnsureCreatedAsync();
            await CheckForUpdatesAsync();
            await UpdateViewAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
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
                ModelState.AddModelError("CheckForNewUsersAsync", e.Message);
                Debug.WriteLine(e);
                throw;
            }
        }

        private async Task UpdateViewAsync()
        {
            await UpdateUsersViewAsync();
            await UpdatePermissionsViewAsync();
            await UpdateManagementViewAsync();
            await PopulateDropDownsListsAsync();
        }

        private async Task UpdateUsersViewAsync()
        {
            if (_users == null)
                _users = new List<UserViewModel>();
            _users.Clear();
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

                    _users.Add(userToAdd);
                });
        }

        private async Task UpdatePermissionsViewAsync()
        {
            if (_permissions == null)
                _permissions = new List<PermissionViewModel>();
            _permissions.Clear();
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
                    _permissions.Add(permissionToAdd);
                });
        }

        private async Task UpdateManagementViewAsync()
        {
            if (_management == null)
                _management = new List<ManagementViewModel>();
            _management.Clear();
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
                        _management.Add(new ManagementViewModel
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
        }

        private async Task PopulateDropDownsListsAsync()
        {
            var users = new List<UserViewModel>();
            var permissions = new List<PermissionViewModel>();

            await _management.ToAsyncEnumerable().ForEachAsync(vm =>
            {
                if (!users.Exists(m => m.UserId == vm.User.UserId))
                {
                    users.Add(vm.User);
                }
            });

            ViewData["usersDropDownList"] = users;
            ViewData["defaultUserDropDownList"] = users.First();

            _permissions.ToList().ForEach(m =>
            {
                var viewModel = new PermissionViewModel
                {
                    PermissionId = m.PermissionId,
                    Name = m.Name
                };
                if (!permissions.Contains(viewModel))
                {
                    permissions.Add(viewModel);
                }
            });

            ViewData["permissionsDropDownList"] = permissions;
            ViewData["defaultPermissionDropDownList"] = permissions.First();
        }

        // TODO: разделить код на вспомогательные сервисы

        #region User OnPost

        public async Task<JsonResult> OnPostReadUser([DataSourceRequest] DataSourceRequest request)
        {
            return await Task.FromResult(new JsonResult(_users.ToDataSourceResult(request)));
        }

        public async Task<JsonResult> OnPostCreateUser([DataSourceRequest] DataSourceRequest request, User user)
        {
            await Task.Run(async () =>
            {
                user.UserId = Guid.NewGuid().ToString();
                user.Password = ShaHelper.GetSHA256String(user.Password);
                _users.Add(new UserViewModel
                {
                    UserId = user.UserId,
                    Password = user.Password,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Permissions = "No permissions"
                });

                // Сохраняем в кэш (наша мини-базочка sqlite, которая хранится в памяти)
                await _uow.Users.AddAsync(user);
                await _uow.SaveChangesAsync();

                // Производим добавление непосредственно в базу на сервер
                await _usersStorageService.AddOrReplaceUserAsync(user);
            });

            return new JsonResult(new[] { user }.ToDataSourceResult(request, ModelState));
        }

        public async Task<JsonResult> OnPostUpdateUser([DataSourceRequest] DataSourceRequest request, User newUser)
        {
            await Task.Run(async () =>
            {
                var userFromList = _users.First(user => user.UserId == newUser.UserId);
                _users.Remove(userFromList);
                _users.Add(userFromList);

                // Обновляем в кэше
                var objForUpdate = await _uow.Users.GetAsync(newUser.UserId);
                objForUpdate.Email = newUser.Email;
                objForUpdate.Password = newUser.Password;
                objForUpdate.FirstName = newUser.FirstName;
                objForUpdate.LastName = newUser.LastName;
                await _uow.Users.UpdateAsync(objForUpdate);
                await _uow.SaveChangesAsync();

                // Производим обновление в mongodb
                await _usersStorageService.AddOrReplaceUserAsync(objForUpdate);
            });

            return new JsonResult(new[] { newUser }.ToDataSourceResult(request, ModelState));
        }

        public async Task<JsonResult> OnPostDestroyUser([DataSourceRequest] DataSourceRequest request, User user)
        {
            await Task.Run(async () =>
            {
                var userForDelete = _users.First(x => x.UserId == user.UserId);
                _users.Remove(userForDelete);

                // Удаляем из кэша
                var objForDelete = await _uow.Users.GetAsync(user.UserId);
                await _uow.Users.DeleteAsync(objForDelete);
                await _uow.SaveChangesAsync();

                // Производим удаление из mongodb
                await _usersStorageService.DeleteUserAsync(objForDelete);
            });

            return new JsonResult(new[] { user }.ToDataSourceResult(request, ModelState));
        }

        #endregion

        #region Permission OnPost

        public async Task<JsonResult> OnPostReadPermission([DataSourceRequest] DataSourceRequest request)
        {
            return await Task.FromResult(new JsonResult(_permissions.ToDataSourceResult(request)));
        }

        public async Task<JsonResult> OnPostCreatePermission([DataSourceRequest] DataSourceRequest request, Permission permission)
        {
            await Task.Run(async () =>
            {
                permission.PermissionId = Guid.NewGuid().ToString();
                _permissions.Add(new PermissionViewModel
                {
                    PermissionId = permission.PermissionId,
                    Name = permission.Name
                });

                // Сохраняем в кэш (наша мини-базочка sqlite, которая хранится в памяти)
                await _uow.Permissions.AddAsync(permission);
                await _uow.SaveChangesAsync();

                // Производим добавление непосредственно в базу на сервер
                await _permissionsService.AddOrReplacePermissionAsync(permission);
            });

            return new JsonResult(new[] { permission }.ToDataSourceResult(request, ModelState));
        }

        public async Task<JsonResult> OnPostUpdatePermission([DataSourceRequest] DataSourceRequest request, Permission newPermission)
        {
            await Task.Run(async () =>
            {
                var permissionFromList = _permissions.First(p => p.PermissionId == newPermission.PermissionId);
                _permissions.Remove(permissionFromList);
                _permissions.Add(permissionFromList);

                // Обновляем в кэше
                var objForUpdate = await _uow.Permissions.GetAsync(newPermission.PermissionId);
                objForUpdate.Name = newPermission.Name;
                await _uow.Permissions.UpdateAsync(objForUpdate);
                await _uow.SaveChangesAsync();

                // Производим обновление в mongodb
                await _permissionsService.AddOrReplacePermissionAsync(objForUpdate);
            });

            return new JsonResult(new[] { newPermission }.ToDataSourceResult(request, ModelState));
        }

        public async Task<JsonResult> OnPostDestroyPermission([DataSourceRequest] DataSourceRequest request, Permission permission)
        {
            await Task.Run(async () =>
            {
                var permissionForDelete = _permissions.First(x => x.PermissionId == permission.PermissionId);
                _permissions.Remove(permissionForDelete);

                // Удаляем из кэша
                var objForDelete = await _uow.Permissions.GetAsync(permission.PermissionId);
                await _uow.Permissions.DeleteAsync(objForDelete);
                await _uow.SaveChangesAsync();

                // Производим удаление из mongodb
                await _permissionsService.DeletePermissionAsync(objForDelete);
            });

            return new JsonResult(new[] { permission }.ToDataSourceResult(request, ModelState));
        }

        #endregion

        #region Management OnPost

        public async Task<JsonResult> OnPostReadManagement([DataSourceRequest] DataSourceRequest request)
        {
            return await Task.FromResult(new JsonResult(_management.ToDataSourceResult(request)));
        }

        public async Task<JsonResult> OnPostCreateManagement([DataSourceRequest] DataSourceRequest request, ManagementViewModel managementVm)
        {
            await Task.Run(async () =>
            {
                // Получаем юзера из списка
                var user = _users.First(m =>
                {
                    var fullName = $"{m.FirstName} {m.LastName}";
                    return fullName == managementVm.User.FullNameUser;
                });
                // Получаем пермишн из списка
                var permission = _permissions.First(m => m.Name == managementVm.Permission.Name);
                // Добавляем новый элемент в список для отображения
                _management.Add(new ManagementViewModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Guid = Guid.NewGuid().ToString(),
                    User = user,
                    Permission = permission
                });

                // Проверяем на наличие такого же заданного разрешения
                var permsForUser = await _permissionsService.GetPermissionsForUserAsync(user.UserId);
                // Если такого нет, даём разрешения
                if (!permsForUser.Any(p => p.PermissionId == permission.PermissionId))
                {
                    await _permissionsService.AssignPermissionAsync(permission.PermissionId, user.UserId);

                    // Фиксируем разрешение для юзера в локальной базе
                    await _uow.BindingEntities.AddAsync(new BindingEntity
                    {
                        UserId = user.UserId,
                        PermissionId = permission.PermissionId
                    });
                    await _uow.SaveChangesAsync();

                    // Обновляем в списке для отображения
                    _users.Remove(user);
                    user.Permissions = user.Permissions.TrimEnd() + $", {permission.Name}";
                    _users.Add(user);
                }
            });

            return new JsonResult(new[] { managementVm }.ToDataSourceResult(request, ModelState));
        }

        public async Task<JsonResult> OnPostDestroyManagement([DataSourceRequest] DataSourceRequest request, ManagementViewModel managementVm)
        {
            await Task.Run(async () =>
            {
                // Удаляем из списка для отображения
                var forDeleteVm = _management
                    .First(x => x.User.FullNameUser == managementVm.User.FullNameUser
                           && x.Permission.Name == managementVm.Permission.Name);
                _management.Remove(forDeleteVm);

                // Удаляем из базы
                var bindingEntities = await _uow.BindingEntities.GetAllWithIncludesAsync();
                var forDelete = bindingEntities
                    .First(be => 
                        be.PermissionId == forDeleteVm.Permission.PermissionId
                        && be.UserId == forDeleteVm.User.UserId);
                await _uow.BindingEntities.DeleteAsync(forDelete);
                await _uow.SaveChangesAsync();

                // Снимаем разрешение
                await _permissionsService.UnassignPermissionAsync(forDeleteVm.Permission.PermissionId, forDeleteVm.User.UserId);
            });

            return new JsonResult(new[] { managementVm }.ToDataSourceResult(request, ModelState));
        }

        #endregion
    }
}
