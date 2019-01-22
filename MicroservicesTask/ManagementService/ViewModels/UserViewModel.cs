using System.ComponentModel.DataAnnotations;

namespace ManagementService.ViewModels
{
    public class UserViewModel
    {
        [Editable(false)]
        public string UserId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long and no more {1}.", MinimumLength = 5)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long and no more {1}.", MinimumLength = 5)]
        public string LastName { get; set; }

        [StringLength(250, ErrorMessage = "The {0} must be at least {2} characters long and no more {1}.", MinimumLength = 5)]
        public string FullNameUser { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} characters long and no more {1}.", MinimumLength = 5)]
        public string Email { get; set; }

        [Required]
        [StringLength(250, ErrorMessage = "The {0} must be at least {2} characters long and no more {1}.", MinimumLength = 5)]
        public string Password { get; set; }

        [Editable(false)]
        public string Permissions { get; set; }
    }
}
