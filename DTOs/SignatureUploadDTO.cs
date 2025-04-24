using System.ComponentModel.DataAnnotations;

namespace UserPermissionsApi.DTOs
{
    public class SignatureUploadDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public IFormFile Signature { get; set; }
    }
}
