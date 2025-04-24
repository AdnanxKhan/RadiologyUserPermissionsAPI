namespace UserPermissionsApi.Models
{
    public class UserCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<UserPermission> Permissions { get; set; } = new List<UserPermission>();
    }
}