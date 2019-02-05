using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ManagementService.ViewModels
{
    public class ManagementViewModel
    {
        [Editable(false)]
        public string Id { get; set; }

        [UIHint("FullNameUser")]
        [DisplayName("User")]
        public UserViewModel User { get; set; }

        [DisplayName("Permission")]
        [UIHint("PermissionName")]
        public PermissionViewModel Permission { get; set; }

        public string Guid { get; set; }
    }
}
