using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ManagementService.Data;
using ManagementService.Models;
using ManagementService.Services.IndexPage;
using ManagementService.Services.Permissions;
using ManagementService.Services.UsersStorage;
using ManagementService.Utils;
using ManagementService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManagementService.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IIndexPageService _indexPageService;
        private readonly IUsersStorageService _usersStorageService;
        private readonly IPermissionsService _permissionsService;
        private readonly IUnitOfWork _uow;
        private readonly ILogger _logger;

        private static IList<UserViewModel> _users;
        private static IList<PermissionViewModel> _permissions;
        private static IList<ManagementViewModel> _management;

        public IndexModel(
            IIndexPageService indexPageService,
            IUsersStorageService usersStorageService,
            IPermissionsService permissionsService,
            IUnitOfWork unitOfWork,
            ILogger<IndexModel> logger)
        {
            _indexPageService = indexPageService;
            _usersStorageService = usersStorageService;
            _permissionsService = permissionsService;
            _uow = unitOfWork;
            _logger = logger;
        }

        public async Task OnGet()
        {
            await _indexPageService.CheckForUpdatesAsync();

            _users = await _indexPageService.GetUsersViewModelsAsync();
            _permissions = await _indexPageService.GetPermissionsViewModelsAsync();
            _management = await _indexPageService.GetManagementViewModelsAsync();

            await PopulateDropDownListsAsync();
        }

        private async Task PopulateDropDownListsAsync()
        {
            var users = new List<UserViewModel>();
            var permissions = new List<PermissionViewModel>();

            await _users.ToAsyncEnumerable().ForEachAsync(vm =>
            {
                var viewModel = new UserViewModel
                {
                    UserId = vm.UserId,
                    Email = vm.Email,
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    FullNameUser = $"{vm.FirstName} {vm.LastName}",
                    Password = vm.Password
                };
                if (!users.Contains(viewModel))
                {
                    users.Add(viewModel);
                }
            });

            ViewData["usersDropDownList"] = users;
            ViewData["defaultUserDropDownList"] = users.First();

            await _permissions.ToAsyncEnumerable().ForEachAsync(m =>
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

        #region User OnPost

        public async Task<JsonResult> OnPostReadUser([DataSourceRequest] DataSourceRequest request)
        {
            return await Task.FromResult(new JsonResult(_users.ToDataSourceResult(request)));
        }

        public async Task<JsonResult> OnPostCreateUser([DataSourceRequest] DataSourceRequest request, User user)
        {
            try
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

                //await PopulateDropDownListsAsync();
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- OnPostCreateUser something wrong.\n\n Reason: {e.Message}");
                _logger.LogDebug(3000, e, "------------------------------------------------------");
            }

            return new JsonResult(new[] { user }.ToDataSourceResult(request, ModelState));
        }

        public async Task<JsonResult> OnPostUpdateUser([DataSourceRequest] DataSourceRequest request, User newUser)
        {
            try
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

                //await PopulateDropDownListsAsync();
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- OnPostUpdateUser something wrong.\n\n Reason: {e.Message}");
                _logger.LogDebug(3000, e, "------------------------------------------------------");
            }

            return new JsonResult(new[] { newUser }.ToDataSourceResult(request, ModelState));
        }

        public async Task<JsonResult> OnPostDestroyUser([DataSourceRequest] DataSourceRequest request, User user)
        {
            try
            {
                var userForDelete = _users.First(x => x.UserId == user.UserId);
                _users.Remove(userForDelete);

                var objForDelete = await _uow.Users.GetAsync(user.UserId);

                // Удаляем из кэша
                await _uow.Users.DeleteAsync(objForDelete);
                await _uow.SaveChangesAsync();

                // Производим удаление из mongodb
                await _usersStorageService.DeleteUserAsync(objForDelete);

                // Удаляем индексы из таблиц UserPermissions и PermissionUsers
                await _permissionsService.DeleteRemainingIndicesForUserAsync(objForDelete);

                //await PopulateDropDownListsAsync();
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- OnPostDestroyUser something wrong.\n\n Reason: {e.Message}");
                _logger.LogDebug(3000, e, "------------------------------------------------------");
            }

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
            try
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

                //await PopulateDropDownListsAsync();
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- OnPostCreatePermission something wrong.\n\n Reason: {e.Message}");
                _logger.LogDebug(3000, e, "------------------------------------------------------");
            }

            return new JsonResult(new[] { permission }.ToDataSourceResult(request, ModelState));
        }

        public async Task<JsonResult> OnPostUpdatePermission([DataSourceRequest] DataSourceRequest request, Permission newPermission)
        {
            try
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

                // TODO: обновление без перезагрузки страницы
                //await PopulateDropDownListsAsync();
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- OnPostUpdatePermission something wrong.\n\n Reason: {e.Message}");
                _logger.LogDebug(3000, e, "------------------------------------------------------");
            }

            return new JsonResult(new[] { newPermission }.ToDataSourceResult(request, ModelState));
        }

        public async Task<JsonResult> OnPostDestroyPermission([DataSourceRequest] DataSourceRequest request, Permission permission)
        {
            try
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

                //await PopulateDropDownListsAsync();
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- OnPostDestroyPermission something wrong.\n\n Reason: {e.Message}");
                _logger.LogDebug(3000, e, "------------------------------------------------------");
            }

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
            try
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
                    if (permsForUser == null || !permsForUser.Any(p => p.PermissionId == permission.PermissionId))
                    {
                        // Даём разрешение в mongodb
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
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- OnPostCreateManagement something wrong.\n\n Reason: {e.Message}");
                _logger.LogDebug(3000, e, "------------------------------------------------------");
            }

            return new JsonResult(new[] { managementVm }.ToDataSourceResult(request, ModelState));
        }

        public async Task<JsonResult> OnPostDestroyManagement([DataSourceRequest] DataSourceRequest request, ManagementViewModel managementVm)
        {
            try
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
            }
            catch (Exception e)
            {
                _logger.LogWarning($"--- OnPostDestroyManagement something wrong.\n\n Reason: {e.Message}");
                _logger.LogDebug(3000, e, "------------------------------------------------------");
            }

            return new JsonResult(new[] { managementVm }.ToDataSourceResult(request, ModelState));
        }

        #endregion
    }
}
