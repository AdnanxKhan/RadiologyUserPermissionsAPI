using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using UserPermissionsApi.Models;

namespace UserPermissionsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public PatientsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetPatients()
        {
            var patients = new List<PatientData>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("SELECT StudyInstanceUID, PatientName, PatientID, Age, Sex, StudyDescription, Modality, AETitle, InstitutionName, ImageCount FROM Patient", connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        patients.Add(new PatientData
                        {
                            StudyInstanceUID = reader.GetString(0),
                            PatientName = reader.GetString(1),
                            PatientID = reader.GetString(2),
                            Age = reader.GetString(3),
                            Sex = reader.GetString(4),
                            StudyDescription = reader.GetString(5),
                            Modality = reader.GetString(6),
                            AETitle = reader.GetString(7),
                            InstitutionName = reader.GetString(8),
                            ImageCount = reader.GetInt32(9)
                        });
                    }
                }
            }

            return Ok(patients);
        }
        [HttpGet("GetPatientsByUser")]
        public async Task<IActionResult> GetPatientsByUser(int userId)
        {
            var patients = new List<PatientData>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var mappedInstitutions = new List<int>();
                var mappedAETitles = new List<int>();
                var mappedModalities = new List<int>();

                async Task LoadIds(string table, string column, List<int> targetList)
                {
                    var cmd = new SqlCommand($"SELECT {column} FROM {table} WHERE UserId = @UserId AND IsActive = 1", connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                        targetList.Add(reader.GetInt32(0));
                    reader.Close();
                }

                await LoadIds("UserInstitutionMapping", "InstitutionId", mappedInstitutions);
                await LoadIds("UserAETitleMapping", "AETitleId", mappedAETitles);
                await LoadIds("UserModalityMapping", "ModalityId", mappedModalities);

                string instIn = mappedInstitutions.Count > 0 ? string.Join(",", mappedInstitutions) : "NULL";
                string aeIn = mappedAETitles.Count > 0 ? string.Join(",", mappedAETitles) : "NULL";
                string modIn = mappedModalities.Count > 0 ? string.Join(",", mappedModalities) : "NULL";

                // Main query: Patients by institution (and optionally by AE Title / Modality)
                var query = $@"
                    SELECT DISTINCT p.StudyInstanceUID, p.PatientName, p.PatientID, p.Age, p.Sex, p.StudyDescription,
                   i.Name AS InstitutionName, a.Name AS AETitle, m.Name AS Modality, p.ImageCount
                    FROM Patient p
                    INNER JOIN Institution i ON i.Id = p.InstitutionId
                    LEFT JOIN AETitle a ON a.Id = p.AETitleId
                    LEFT JOIN Modality m ON m.Id = p.ModalityId
                    WHERE p.InstitutionId IN ({instIn})";

                if (mappedAETitles.Count > 0)
                    query += $" AND (p.AETitleId IS NULL OR p.AETitleId IN ({aeIn}))";

                if (mappedModalities.Count > 0)
                    query += $" AND (p.ModalityId IS NULL OR p.ModalityId IN ({modIn}))";

                // Also include directly assigned patients
                query += @"
                    UNION
                    SELECT DISTINCT p.StudyInstanceUID, p.PatientName, p.PatientID, p.Age, p.Sex, p.StudyDescription,
                    i.Name AS InstitutionName, a.Name AS AETitle, m.Name AS Modality, p.ImageCount
                    FROM Patient p
                    INNER JOIN UserPatientAssignmentMapping upam ON upam.PatientId = p.Id AND upam.IsActive = 1
                    INNER JOIN Institution i ON i.Id = p.InstitutionId
                    LEFT JOIN AETitle a ON a.Id = p.AETitleId
                    LEFT JOIN Modality m ON m.Id = p.ModalityId
                    WHERE upam.UserId = @UserId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var seenUIDs = new HashSet<string>();

                        while (await reader.ReadAsync())
                        {
                            var uid = reader.GetString(0);
                            if (seenUIDs.Contains(uid)) continue;
                            seenUIDs.Add(uid);

                            patients.Add(new PatientData
                            {
                                StudyInstanceUID = uid,
                                PatientName = reader.GetString(1),
                                PatientID = reader.GetString(2),
                                Age = reader.GetString(3),
                                Sex = reader.GetString(4),
                                StudyDescription = reader.GetString(5),
                                InstitutionName = reader.GetString(6),
                                AETitle = reader.IsDBNull(7) ? null : reader.GetString(7),
                                Modality = reader.IsDBNull(8) ? null : reader.GetString(8),
                                ImageCount = reader.GetInt32(9)
                            });
                        }
                    }
                }
            }

            return Ok(patients);
        }
        [HttpGet("GetPatientsByInstitutionAssignedToUser")]
        public async Task<IActionResult> GetPatientsByInstitution(int userId)
        {
            var patients = new List<PatientData>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var institutionIds = new List<int>();
                var cmd = new SqlCommand("SELECT InstitutionId FROM UserInstitutionMapping WHERE UserId = @UserId AND IsActive = 1", connection);
                cmd.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        institutionIds.Add(reader.GetInt32(0));
                }

                if (institutionIds.Count == 0)
                    return Ok(patients);

                string instIn = string.Join(",", institutionIds);

                var query = $@"
                    SELECT p.StudyInstanceUID, p.PatientName, p.PatientID, p.Age, p.Sex, p.StudyDescription,
                    i.Name AS InstitutionName, a.Name AS AETitle, m.Name AS Modality, p.ImageCount
                    FROM Patient p
                    INNER JOIN Institution i ON i.Id = p.InstitutionId
                    LEFT JOIN AETitle a ON a.Id = p.AETitleId
                    LEFT JOIN Modality m ON m.Id = p.ModalityId
                    WHERE p.InstitutionId IN ({instIn})";

                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        patients.Add(new PatientData
                        {
                            StudyInstanceUID = reader.GetString(0),
                            PatientName = reader.GetString(1),
                            PatientID = reader.GetString(2),
                            Age = reader.GetString(3),
                            Sex = reader.GetString(4),
                            StudyDescription = reader.GetString(5),
                            InstitutionName = reader.GetString(6),
                            AETitle = reader.IsDBNull(7) ? null : reader.GetString(7),
                            Modality = reader.IsDBNull(8) ? null : reader.GetString(8),
                            ImageCount = reader.GetInt32(9)
                        });
                    }
                }
            }

            return Ok(patients);
        }
        [HttpGet("GetPatientsByAETitleAssignedToUser")]
        public async Task<IActionResult> GetPatientsByAETitle(int userId)
        {
            var patients = new List<PatientData>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var aeTitleIds = new List<int>();
                var cmd = new SqlCommand("SELECT AETitleId FROM UserAETitleMapping WHERE UserId = @UserId AND IsActive = 1", connection);
                cmd.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        aeTitleIds.Add(reader.GetInt32(0));
                }

                if (aeTitleIds.Count == 0)
                    return Ok(patients);

                string aeIn = string.Join(",", aeTitleIds);

                var query = $@"
                    SELECT p.StudyInstanceUID, p.PatientName, p.PatientID, p.Age, p.Sex, p.StudyDescription,
                    i.Name AS InstitutionName, a.Name AS AETitle, m.Name AS Modality, p.ImageCount
                    FROM Patient p
                    INNER JOIN Institution i ON i.Id = p.InstitutionId
                    LEFT JOIN AETitle a ON a.Id = p.AETitleId
                    LEFT JOIN Modality m ON m.Id = p.ModalityId
                    WHERE p.AETitleId IN ({aeIn})";

                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        patients.Add(new PatientData
                        {
                            StudyInstanceUID = reader.GetString(0),
                            PatientName = reader.GetString(1),
                            PatientID = reader.GetString(2),
                            Age = reader.GetString(3),
                            Sex = reader.GetString(4),
                            StudyDescription = reader.GetString(5),
                            InstitutionName = reader.GetString(6),
                            AETitle = reader.IsDBNull(7) ? null : reader.GetString(7),
                            Modality = reader.IsDBNull(8) ? null : reader.GetString(8),
                            ImageCount = reader.GetInt32(9)
                        });
                    }
                }
            }

            return Ok(patients);
        }
        [HttpGet("GetPatientsByModalityAssignedToUser")]
        public async Task<IActionResult> GetPatientsByModality(int userId)
        {
            var patients = new List<PatientData>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var modalityIds = new List<int>();
                var cmd = new SqlCommand("SELECT ModalityId FROM UserModalityMapping WHERE UserId = @UserId AND IsActive = 1", connection);
                cmd.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        modalityIds.Add(reader.GetInt32(0));
                }

                if (modalityIds.Count == 0)
                    return Ok(patients);

                string modIn = string.Join(",", modalityIds);

                var query = $@"
                    SELECT p.StudyInstanceUID, p.PatientName, p.PatientID, p.Age, p.Sex, p.StudyDescription,
                    i.Name AS InstitutionName, a.Name AS AETitle, m.Name AS Modality, p.ImageCount
                    FROM Patient p
                    INNER JOIN Institution i ON i.Id = p.InstitutionId
                    LEFT JOIN AETitle a ON a.Id = p.AETitleId
                    LEFT JOIN Modality m ON m.Id = p.ModalityId
                    WHERE p.ModalityId IN ({modIn})";

                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        patients.Add(new PatientData
                        {
                            StudyInstanceUID = reader.GetString(0),
                            PatientName = reader.GetString(1),
                            PatientID = reader.GetString(2),
                            Age = reader.GetString(3),
                            Sex = reader.GetString(4),
                            StudyDescription = reader.GetString(5),
                            InstitutionName = reader.GetString(6),
                            AETitle = reader.IsDBNull(7) ? null : reader.GetString(7),
                            Modality = reader.IsDBNull(8) ? null : reader.GetString(8),
                            ImageCount = reader.GetInt32(9)
                        });
                    }
                }
            }

            return Ok(patients);
        }
    }
}