using System.ComponentModel.DataAnnotations;

namespace ManagementService.ViewModels
{
    public class PermissionViewModel
    {
        [Editable(false)]
        public string PermissionId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long and no more {1}.", MinimumLength = 5)]
        public string Name { get; set; }
    }
}
