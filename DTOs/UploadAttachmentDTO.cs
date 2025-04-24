namespace UserPermissionsApi.DTOs
{
    public class UploadAttachmentDTO
    {
        public int UserId { get; set; }
        public int PatientId { get; set; }
        public IFormFile File { get; set; }
    }
}
