using System.ComponentModel.DataAnnotations;

namespace UserPermissionsApi.DTOs
{
    public class UserPermissionDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int UserCategoryId { get; set; }
    }
}