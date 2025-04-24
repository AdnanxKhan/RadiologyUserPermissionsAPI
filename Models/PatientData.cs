namespace UserPermissionsApi.Models
{
    public class PatientData
    {
        public string StudyInstanceUID { get; set; }
        public string PatientName { get; set; }
        public string PatientID { get; set; }
        public string Age { get; set; }
        public string Sex { get; set; }
        public string StudyDescription { get; set; }
        public string Modality { get; set; }
        public string AETitle { get; set; }
        public string InstitutionName { get; set; }
        public int ImageCount { get; set; }
    }
}
