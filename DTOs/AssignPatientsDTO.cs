namespace UserPermissionsApi.DTOs
{
    public class AssignPatientsDTO
    {
        public int UserId { get; set; }
        public List<int> PatientIds { get; set; }
    }
}
