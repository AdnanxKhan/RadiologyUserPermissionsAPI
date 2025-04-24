namespace UserPermissionsApi.DTOs
{
    public class PermissionUpdateDTO
    {
        public int UserId { get; set; }
        public int AssignedToUserId { get; set; }
        public int PermissionId { get; set; }
        public bool IsAllowed { get; set; }
    }
}
