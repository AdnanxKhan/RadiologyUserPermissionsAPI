namespace UserPermissionsApi.DTOs
{
    public class CreateUserDTO
    {
        public string Username { get; set; } = string.Empty;
        public int UserCategoryIdToCreate { get; set; } // The type of user to create
        public int CreatedByUserId { get; set; }
        public List<PermissionDTO> Permissions { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}
