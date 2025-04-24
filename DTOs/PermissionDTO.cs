namespace UserPermissionsApi.DTOs
{
    public class PermissionDTO
    {
        public int UserPermissionId { get; set; }
        public bool IsAllowed { get; set; }  // TRUE for allowed, FALSE for disallowed
    }
}
