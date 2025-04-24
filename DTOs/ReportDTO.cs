namespace UserPermissionsApi.DTOs
{
    public class ReportDTO
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int RadiologistId { get; set; }
        public string ReportText { get; set; }
    }
}
