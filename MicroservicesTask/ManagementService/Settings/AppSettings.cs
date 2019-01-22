namespace ManagementService.Settings
{
    public class AppSettings
    {
        public ApiEndPoints ApiEndPoints { get; set; }
    }

    public class ApiEndPoints
    {
        public string UsersStorageService { get; set; }
        public string PermissionsService { get; set; }
    }
}
