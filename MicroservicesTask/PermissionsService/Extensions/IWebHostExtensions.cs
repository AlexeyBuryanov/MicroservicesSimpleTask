using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PermissionsService.Data;
using PermissionsService.Data.Repositories;
using PermissionsService.Models;
using System.Threading.Tasks;
using PermissionsService.Data.Repositories.Base;

namespace PermissionsService.Extensions
{
    public static class IWebHostExtensions
    {
        public static async Task<IWebHost> SeedDbAsync(this IWebHost webHost)
        {
            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var repositoryService = services.GetService<IRepositoryAsync<Permission>>();

                if (repositoryService is PermissionRepository permissionsRepo)
                {
                    var any = await permissionsRepo.AnyAsync();

                    if (!any)
                    {
                        // Permissions Ids
                        const string addUsersPermId     = "f4264577-7705-4d4f-835c-4fd0e9d924c0";
                        const string readNewsPermId     = "4a97b484-eefe-43ea-8377-2a3595ef49cd";
                        const string createGroupsPermId = "6db07cbb-f8e6-41f1-af9b-ed10a78e740f";
                        const string publishNewsPermId  = "e1dfec14-80f2-4f32-bce7-dab3a39fe978";

                        // Permissions seed
                        await Task.WhenAll(
                            permissionsRepo.AddOrReplaceAsync(addUsersPermId, new Permission
                            {
                                PermissionId = addUsersPermId,
                                Name = "AddUsers"
                            }),
                            permissionsRepo.AddOrReplaceAsync(readNewsPermId, new Permission
                            {
                                PermissionId = readNewsPermId,
                                Name = "ReadNews"
                            }),
                            permissionsRepo.AddOrReplaceAsync(createGroupsPermId, new Permission
                            {
                                PermissionId = createGroupsPermId,
                                Name = "CreateGroups"
                            }),
                            permissionsRepo.AddOrReplaceAsync(publishNewsPermId, new Permission
                            {
                                PermissionId = publishNewsPermId,
                                Name = "PublishNews"
                            })
                        );

                        // Vasya Pupkin
                        await permissionsRepo.AssignPermissionAsync(permissionId: addUsersPermId, userId: "e8a76441-56ce-483c-99f7-2dcbfb39ec21");
                        await permissionsRepo.AssignPermissionAsync(permissionId: readNewsPermId, userId: "e8a76441-56ce-483c-99f7-2dcbfb39ec21");
                        await permissionsRepo.AssignPermissionAsync(permissionId: createGroupsPermId, userId: "e8a76441-56ce-483c-99f7-2dcbfb39ec21");
                        await permissionsRepo.AssignPermissionAsync(permissionId: publishNewsPermId, userId: "e8a76441-56ce-483c-99f7-2dcbfb39ec21");

                        // Test unassign
                        //await permissionsRepo.UnassignPermissionAsync(permissionId: createGroupsPermId, userId: "e8a76441-56ce-483c-99f7-2dcbfb39ec21");
                        //await permissionsRepo.UnassignPermissionAsync(permissionId: publishNewsPermId, userId: "e8a76441-56ce-483c-99f7-2dcbfb39ec21");

                        // Sasha Ronin
                        await permissionsRepo.AssignPermissionAsync(permissionId: readNewsPermId, userId: "2229587e-276d-42d0-93c4-fd0e9bd003c7");
                        await permissionsRepo.AssignPermissionAsync(permissionId: publishNewsPermId, userId: "2229587e-276d-42d0-93c4-fd0e9bd003c7");
                    }
                }
            } // using

            return webHost;
        } // SeedDb
    }
}
