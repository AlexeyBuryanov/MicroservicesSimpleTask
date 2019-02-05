using ManagementService.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ManagementService.Services.IndexPage
{
    public interface IIndexPageService
    {
        Task CheckForUpdatesAsync();
        Task<List<UserViewModel>> GetUsersViewModelsAsync();
        Task<List<PermissionViewModel>> GetPermissionsViewModelsAsync();
        Task<List<ManagementViewModel>> GetManagementViewModelsAsync();
    }
}