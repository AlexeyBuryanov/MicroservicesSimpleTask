using ManagementService.Data.Repositories;
using System;
using System.Threading.Tasks;

namespace ManagementService.Data
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IPermissionRepository Permissions { get; }
        IBindingEntitiesRepository BindingEntities { get; }
        int SaveChanges();
        Task<int> SaveChangesAsync();
        Task EnsureCreatedAsync();
        Task EnsureDeletedAsync();
    }
}
