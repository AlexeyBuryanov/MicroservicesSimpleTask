using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using UsersStorageService.Data.Repositories.Base;
using UsersStorageService.Models;
using UsersStorageService.Utils;

namespace UsersStorageService.Extensions
{
    public static class IWebHostExtensions
    {
        public static async Task<IWebHost> SeedDbAsync(this IWebHost webHost)
        {
            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var usersRepo = services.GetService<IRepositoryAsync<User>>();
                var logger = services.GetService<ILogger<IWebHost>>();

                var any = await usersRepo.AnyAsync();

                if (!any)
                {
                    const string id1 = "e8a76441-56ce-483c-99f7-2dcbfb39ec21";
                    const string id2 = "2229587e-276d-42d0-93c4-fd0e9bd003c7";
                    var password = ShaHelper.GetSHA256String("12345");

                    await Task.WhenAll(
                        usersRepo.AddOrReplaceAsync(id: id1, item: new User
                        {
                            Email = "vasya.pupkin@mail.com",
                            FirstName = "Vasya",
                            LastName = "Pupkin",
                            Password = password,
                            UserId = id1
                        }),
                        usersRepo.AddOrReplaceAsync(id: id2, item: new User
                        {
                            Email = "sasha.ronin@mail.com",
                            FirstName = "Sasha",
                            LastName = "Ronin",
                            Password = password,
                            UserId = id2
                        })
                    );

                    logger.LogInformation("--- Seeded the database");
                }
            } // using

            return webHost;
        } // SeedDb
    }
}