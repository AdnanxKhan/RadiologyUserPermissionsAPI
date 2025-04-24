namespace UserPermissionsApi.Models
{
    public class UserPermissionMapping
    {
        public int UserId { get; set; }
        public int UserCategoryId { get; set; }
        public int UserPermissionId { get; set; }
        public bool IsAllowed { get; set; }
    }
}
