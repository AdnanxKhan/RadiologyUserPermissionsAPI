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
    }
}