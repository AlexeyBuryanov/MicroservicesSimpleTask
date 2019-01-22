namespace ManagementService.Models
{
    public class BindingEntity
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public string PermissionId { get; set; }
        public Permission Permission { get; set; }
    }
}
