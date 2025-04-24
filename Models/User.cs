namespace UserPermissionsApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public int UserCategoryId { get; set; }
        public UserCategory UserCategory { get; set; }
    }
}
