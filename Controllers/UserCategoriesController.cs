using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserPermissionsApi.DTOs;

namespace UserPermissionsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserCategoriesController : ControllerBase
    {
        private readonly string _connectionString;

        public UserCategoriesController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = new List<UserCategoryDTO>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT Id, Name FROM UserCategory", connection);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(new UserCategoryDTO
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }

            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserCategoryDTO categoryDto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("INSERT INTO UserCategory (Name) OUTPUT INSERTED.Id VALUES (@Name)", connection);
                command.Parameters.AddWithValue("@Name", categoryDto.Name);

                var newCategoryId = (int)await command.ExecuteScalarAsync();

                categoryDto.Id = newCategoryId;
            }

            return CreatedAtAction(nameof(GetAll), new { id = categoryDto.Id }, categoryDto);
        }
    }
}