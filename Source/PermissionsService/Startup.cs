﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PermissionsService.Data;
using PermissionsService.Data.Repositories;
using PermissionsService.Data.Repositories.Base;
using PermissionsService.Models;
using PermissionsService.Services.Permissions;
using PermissionsService.Settings;
using Swashbuckle.AspNetCore.Swagger;

namespace PermissionsService
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

            services.AddSingleton<IRepositoryAsync<Permission>, PermissionRepository>();
            services.AddSingleton<IMongoDbContext, PermissionsContext>();
            services.AddSingleton<IPermissionsService, Services.Permissions.PermissionsService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Permissions API",
                    Version = "v1",
                    Description = "ASP.NET Core Web API for permissions and relations between permissions and users",
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Permissions API V1");
                c.RoutePrefix = string.Empty;
            });

            app.UseMvc();
        }
    }
}
