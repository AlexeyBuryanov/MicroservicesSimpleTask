using ManagementService.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ManagementService.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ManagementDbContext _managementDbContext;

        public UnitOfWork(ManagementDbContext managementDbContext)
        {
            _managementDbContext = managementDbContext;
            Users = new UserRepository(_managementDbContext);
            Permissions = new PermissionRepository(_managementDbContext);
            BindingEntities = new BindingEntitiesRepository(_managementDbContext);
        }

        private static readonly Lazy<UnitOfWork> Lazy =
            new Lazy<UnitOfWork>(() => new UnitOfWork(new ManagementDbContext()));
        public static UnitOfWork Instance => Lazy.Value;

        public IUserRepository Users { get; }
        public IPermissionRepository Permissions { get; }
        public IBindingEntitiesRepository BindingEntities { get; }

        public int SaveChanges() 
            => _managementDbContext.SaveChanges();

        public Task<int> SaveChangesAsync() 
            => _managementDbContext.SaveChangesAsync();

        public async Task EnsureCreatedAsync() 
            => await _managementDbContext.Database.EnsureCreatedAsync();

        public async Task EnsureDeletedAsync()
            => await _managementDbContext.Database.EnsureDeletedAsync();


        private bool _disposed = false;
        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _managementDbContext.Database.CloseConnection();
                    _managementDbContext.Dispose();
                }
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
