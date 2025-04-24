using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using UserPermissionsApi.DTOs;

[ApiController]
[Route("api/[controller]")]
public class UserPermissionsController : ControllerBase
{
    private readonly string _connectionString;

    public UserPermissionsController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    [HttpGet("by-category/{categoryId}")]
    public async Task<IActionResult> GetPermissionsByCategoryId(int categoryId)
    {
        var result = new List<UserPermissionByCategoryDTO>();

        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (var command = new SqlCommand("GetPermissionsByCategory", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserCategoryId", categoryId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new UserPermissionByCategoryDTO
                        {
                            UserPermissionId = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            IsActive = reader.GetBoolean(2)
                        });
                    }
                }
            }
        }

        return Ok(result);
    }
}