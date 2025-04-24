namespace UserPermissionsApi.DTOs
{
    public class PatientHistoryDTO
    {
        public int PatientId { get; set; }
        public int UserId { get; set; }
        public string History { get; set; }
    }
}
