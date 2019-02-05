using System.Collections.Generic;

namespace ManagementService.Models
{
    public class User
    {
        public string UserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public ICollection<BindingEntity> BindingEntities { get; set; }
    }
}
