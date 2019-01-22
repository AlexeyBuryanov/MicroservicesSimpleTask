using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using PermissionsService.Extensions;
using System.Threading.Tasks;

namespace PermissionsService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();
            webHost = await webHost.SeedDbAsync();
            await webHost.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
