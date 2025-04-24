using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using UserPermissionsApi.DTOs;

namespace UserPermissionsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand("SELECT Id, Username, UserCategoryId FROM Users WHERE Username = @Username AND Password = @Password", connection);
                command.Parameters.AddWithValue("@Username", dto.Username);
                command.Parameters.AddWithValue("@Password", dto.Password);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        // ✅ Store data before closing reader
                        int userId = reader.GetInt32(0);
                        string username = reader.GetString(1);
                        int userCategoryId = reader.GetInt32(2);

                        reader.Close(); // ✅ Now safe to close

                        // Insert into UserSessions
                        var sessionId = Guid.NewGuid().ToString();
                        var loginTime = DateTime.Now;

                        var insertCmd = new SqlCommand(@"INSERT INTO UserSessions 
                    (UserId, LoginTime, IsActive, SessionId) 
                    VALUES (@UserId, @LoginTime, @IsActive, @SessionId)", connection);

                        insertCmd.Parameters.AddWithValue("@UserId", userId);
                        insertCmd.Parameters.AddWithValue("@LoginTime", loginTime);
                        insertCmd.Parameters.AddWithValue("@IsActive", true);
                        insertCmd.Parameters.AddWithValue("@SessionId", sessionId);

                        await insertCmd.ExecuteNonQueryAsync();

                        return Ok(new
                        {
                            Id = userId,
                            Username = username,
                            UserCategoryId = userCategoryId,
                            SessionId = sessionId
                        });
                    }
                }
            }

            return Unauthorized("Invalid username or password.");
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(int sessionId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var command = new SqlCommand(@"
                    UPDATE UserSessions
                    SET IsActive = 0,
                    LogoutTime = @LogoutTime
                    WHERE Id = @SessionId AND IsActive = 1", connection);

                command.Parameters.AddWithValue("@SessionId", Convert.ToInt32(sessionId));
                command.Parameters.AddWithValue("@LogoutTime", DateTime.Now);

                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return Ok("User logged out successfully.");
                }
                else
                {
                    return NotFound("Session not found or already inactive.");
                }
            }
        }
    }
}