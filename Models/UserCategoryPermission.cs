namespace UserPermissionsApi.Models
{
    public class UserCategoryPermission
    {
        public int UserCategoryId { get; set; }
        public int UserPermissionId { get; set; }
        public bool IsAllowed { get; set; }

        public UserCategory UserCategory { get; set; }
        public UserPermission UserPermission { get; set; }
    }
}
