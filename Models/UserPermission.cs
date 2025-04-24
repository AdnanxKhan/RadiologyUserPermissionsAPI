namespace UserPermissionsApi.Models
{
    public class UserPermission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int UserCategoryId { get; set; }
        public UserCategory UserCategory { get; set; }
    }
}