using ManagementService.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ManagementService.Data
{
    public class ManagementDbContext : DbContext
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public DbSet<User> Users { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<BindingEntity> BindingEntities { get; set; }

        public ManagementDbContext(
            ILogger<ManagementDbContext> logger,
            ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        protected override async void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            _logger.LogInformation("--- Open connection with internal sqlitedb");
            optionsBuilder
                .UseSqlite(connection)
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .UseLoggerFactory(_loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Не знаю, как в EF Core 2.2, но в версии 2.0 нельзя было создавать соотношения многие
            // ко многим без связующей сущности
            modelBuilder.Entity<BindingEntity>()
                .HasKey(be => new { be.UserId, be.PermissionId });
            modelBuilder.Entity<BindingEntity>()
                .HasOne(be => be.User)
                .WithMany(u => u.BindingEntities)
                .HasForeignKey(be => be.UserId);
            modelBuilder.Entity<BindingEntity>()
                .HasOne(be => be.Permission)
                .WithMany(p => p.BindingEntities)
                .HasForeignKey(be => be.PermissionId);

            //modelBuilder.Entity<UserPermissions>()
            //    .HasOne(p => p.User)
            //    .WithMany(u => u.UserPermissions)
            //    .HasForeignKey(u => u.UserId);
            //modelBuilder.Entity<PermissionUsers>()
            //    .HasOne(p => p.Permission)
            //    .WithMany(u => u.PermissionUsers)
            //    .HasForeignKey(u => u.PermissionId);

            _logger.LogInformation("--- SQLite model created");
        }
    }
}
