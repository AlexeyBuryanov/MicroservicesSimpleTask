using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using UsersStorageService.Data;
using UsersStorageService.Data.Repositories;
using UsersStorageService.Data.Repositories.Base;
using UsersStorageService.Models;
using UsersStorageService.Settings;

namespace UsersStorageService
{
    public class Startup
    {
        private readonly ILogger _logger;

        public Startup(
            IConfiguration configuration, 
            ILogger<Startup> logger)
        {
            Configuration = configuration;
            _logger = logger;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<AppSettings>(Configuration);

            services.AddSingleton<IRepositoryAsync<User>, UserRepository>();
            services.AddSingleton<IMongoDbContext, UsersContext>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Users Storage API",
                    Version = "v1",
                    Description = "ASP.NET Core Web API for users storage",
                    TermsOfService = "None",
                    Contact = new Contact
                    {
                        Name = "Alexey Bur'yanov",
                        Email = @"alexeyburyanov@gmail.com",
                        Url = "https://github.com/AlexeyBuryanov"
                    },
                    License = new License
                    {
                        Name = "MIT License",
                        Url = "https://opensource.org/licenses/MIT"
                    }
                });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddLogging();
        }

        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            app.UseCors(builder => {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Users Storage API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseMvc();
        }
    }
}
