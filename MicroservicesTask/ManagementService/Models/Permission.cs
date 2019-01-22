using System.Collections.Generic;

namespace ManagementService.Models
{
    public class Permission
    {
        public string PermissionId { get; set; }

        public string Name { get; set; }

        public ICollection<BindingEntity> BindingEntities { get; set; }
    }
}
