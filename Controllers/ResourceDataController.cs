using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using UserPermissionsApi.DTOs;

namespace UserPermissionsApi.Controllers
{
    public class ResourceDataController : Controller
    {
        private readonly string _connectionString;

        public ResourceDataController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        [HttpPost("AddInstitution")]
        public async Task<IActionResult> AddInstitution([FromBody] ResourceDataInputDTO input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
                return BadRequest("Institution name is required.");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Check if Institution exists
                var checkQuery = "SELECT COUNT(*) FROM Institution WHERE LOWER(Name) = LOWER(@Name)";
                using (var checkCmd = new SqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Name", input.Name);
                    var exists = (int)await checkCmd.ExecuteScalarAsync() > 0;

                    if (exists)
                        return Conflict("Institution with this name already exists.");
                }

                // Insert new Institution
                var insertQuery = "INSERT INTO Institution (Name) VALUES (@Name)";
                using (var insertCmd = new SqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@Name", input.Name);
                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Institution added successfully.");
        }
        [HttpPost("AddModality")]
        public async Task<IActionResult> AddModality([FromBody] ResourceDataInputDTO input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
                return BadRequest("Modality name is required.");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var checkQuery = "SELECT COUNT(*) FROM Modality WHERE LOWER(Name) = LOWER(@Name)";
                using (var checkCmd = new SqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Name", input.Name);
                    var exists = (int)await checkCmd.ExecuteScalarAsync() > 0;

                    if (exists)
                        return Conflict("Modality with this name already exists.");
                }

                var insertQuery = "INSERT INTO Modality (Name) VALUES (@Name)";
                using (var insertCmd = new SqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@Name", input.Name);
                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Modality added successfully.");
        }
        [HttpPost("AddAETitle")]
        public async Task<IActionResult> AddAETitle([FromBody] ResourceDataInputDTO input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
                return BadRequest("AE Title name is required.");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var checkQuery = "SELECT COUNT(*) FROM AETitle WHERE LOWER(Name) = LOWER(@Name)";
                using (var checkCmd = new SqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Name", input.Name);
                    var exists = (int)await checkCmd.ExecuteScalarAsync() > 0;

                    if (exists)
                        return Conflict("AE Title with this name already exists.");
                }

                var insertQuery = "INSERT INTO AETitle (Name) VALUES (@Name)";
                using (var insertCmd = new SqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@Name", input.Name);
                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("AE Title added successfully.");
        }
    }
}
