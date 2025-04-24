using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using UserPermissionsApi.DTOs;
using UserPermissionsApi.Models;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System.ComponentModel.DataAnnotations;

namespace UserPermissionsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly string _connectionString;

        public UsersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        private bool CanCreateUser(int creatorCategoryId, int targetCategoryId)
        {
            if (creatorCategoryId == 1) return true; // Super Admin
            if (creatorCategoryId == 2 && targetCategoryId != 1) return true; // Admin
            if (creatorCategoryId == 3 && targetCategoryId >= 4 && targetCategoryId <= 7) return true; // Group Admin
            return false;
        }

        [HttpPost("CreateNewUser")]
        public async Task<IActionResult> CreateUser(CreateUserDTO dto)
        {
            int creatorCategoryId;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand("SELECT UserCategoryId FROM Users WHERE Id = @UserId", connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", dto.CreatedByUserId);

                    var result = await cmd.ExecuteScalarAsync();
                    if (result == null)
                        return NotFound("Creator user not found.");

                    creatorCategoryId = Convert.ToInt32(result);
                }

                // Check permission
                if (!CanCreateUser(creatorCategoryId, dto.UserCategoryIdToCreate))
                    return Forbid("You are not allowed to create this type of user.");

                // Call stored proc to create user
                int newUserId;
                using (var cmd = new SqlCommand("CreateUserWithPermissions", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", dto.Username);
                    cmd.Parameters.AddWithValue("@UserCategoryId", dto.UserCategoryIdToCreate);
                    cmd.Parameters.AddWithValue("@CreatedByUserId", dto.CreatedByUserId);
                    cmd.Parameters.AddWithValue("@Password", dto.Password);

                    object result = await cmd.ExecuteScalarAsync();
                    newUserId = Convert.ToInt32(result);
                }

                // Get default permissions for category
                List<PermissionDTO> categoryPermissions = new List<PermissionDTO>();

                using (var cmd = new SqlCommand("SELECT UserPermissionId FROM UserCategoryPermission WHERE UserCategoryId = @CategoryId", connection))
                {
                    cmd.Parameters.AddWithValue("@CategoryId", dto.UserCategoryIdToCreate);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categoryPermissions.Add(new PermissionDTO
                            {
                                UserPermissionId = reader.GetInt32(0),
                                IsAllowed = true
                            });
                        }
                    }
                }

                //Merge custom permissions from DTO
                foreach (var defaultPermission in categoryPermissions)
                {
                    var custom = dto.Permissions.FirstOrDefault(p => p.UserPermissionId == defaultPermission.UserPermissionId);
                    defaultPermission.IsAllowed = custom?.IsAllowed ?? true;

                    using (var cmd = new SqlCommand(@"
                        INSERT INTO UserPermissionMapping (UserId, UserCategoryId, UserPermissionId, IsAllowed)
                        VALUES (@UserId, @CategoryId, @PermissionId, @IsAllowed)", connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", newUserId);
                        cmd.Parameters.AddWithValue("@CategoryId", dto.UserCategoryIdToCreate);
                        cmd.Parameters.AddWithValue("@PermissionId", defaultPermission.UserPermissionId);
                        cmd.Parameters.AddWithValue("@IsAllowed", defaultPermission.IsAllowed);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return CreatedAtAction(nameof(CreateUser), new { id = newUserId }, newUserId);
            }
        }
        [HttpGet("GetUserPermissions/{userId}")]
        public async Task<IActionResult> GetUserPermissions(int userId)
        {
            var permissions = new List<UserPermissionsDTO>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                SELECT up.Name, upm.IsAllowed
                FROM UserPermissionMapping upm
                LEFT JOIN UserPermission up ON up.Id = upm.UserPermissionId
                WHERE upm.UserId = @UserId", connection);

            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                permissions.Add(new UserPermissionsDTO
                {
                    Name = reader["Name"].ToString(),
                    IsAllowed = Convert.ToBoolean(reader["IsAllowed"])
                });
            }

            return Ok(permissions);
        }
        [HttpPost("UpdateUserPermission")]
        public async Task<IActionResult> UpdatePermission([FromBody] PermissionUpdateDTO dto)
        {
            const int MANAGE_PERMISSION_ID = 30;

            if (!await HasPermission(dto.UserId, MANAGE_PERMISSION_ID))
                return Forbid("User does not have permission to manage permissions.");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                UPDATE UserPermissionMapping
                SET IsAllowed = @IsAllowed, UpdatedAt = GETDATE()
                WHERE UserId = @AssignedToUserId AND UserPermissionId = @PermissionId", connection);

            command.Parameters.AddWithValue("@IsAllowed", dto.IsAllowed ? 1 : 0);
            command.Parameters.AddWithValue("@AssignedToUserId", dto.AssignedToUserId);
            command.Parameters.AddWithValue("@PermissionId", dto.PermissionId);

            int rowsAffected = await command.ExecuteNonQueryAsync();
            if (rowsAffected == 0)
                return NotFound("Mapping not found.");

            return Ok("Permission updated successfully.");
        }
        [HttpPost("AddPermission")]
        public async Task<IActionResult> AddPermission([FromBody] PermissionUpdateDTO dto)
        {
            const int MANAGE_PERMISSION_ID = 30;

            if (!await HasPermission(dto.UserId, MANAGE_PERMISSION_ID))
                return Forbid("User does not have permission to manage permissions.");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var getUserCategoryCmd = new SqlCommand("SELECT UserCategoryId FROM Users WHERE Id = @AssignedToUserId", connection);
            getUserCategoryCmd.Parameters.AddWithValue("@AssignedToUserId", dto.AssignedToUserId);

            var userCategoryId = (int?)await getUserCategoryCmd.ExecuteScalarAsync();
            if (userCategoryId == null)
                return NotFound("User not found.");

            var insertCmd = new SqlCommand(@"
                INSERT INTO UserPermissionMapping (UserId, UserCategoryId, UserPermissionId, IsAllowed, CreatedAt, UpdatedAt)
                VALUES (@AssignedToUserId, @UserCategoryId, @PermissionId, @IsAllowed, GETDATE(), GETDATE())", connection);

            insertCmd.Parameters.AddWithValue("@AssignedToUserId", dto.AssignedToUserId);
            insertCmd.Parameters.AddWithValue("@UserCategoryId", userCategoryId);
            insertCmd.Parameters.AddWithValue("@PermissionId", dto.PermissionId);
            insertCmd.Parameters.AddWithValue("@IsAllowed", dto.IsAllowed ? 1 : 0);

            await insertCmd.ExecuteNonQueryAsync();

            return Ok("Permission added successfully.");
        }
        private async Task<bool> HasPermission(int userId, int permissionId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM UserPermissionMapping 
                WHERE UserId = @UserId AND UserPermissionId = @PermissionId AND IsAllowed = 1", connection);

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@PermissionId", permissionId);

            return (int)await cmd.ExecuteScalarAsync() > 0;
        }

        [HttpPost("AssignPatientToUser")]
        public async Task<IActionResult> AssignPatient(int userId, int patientId, int assignTo)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            //int permissionId = 4;
            const int READ_PERMISSION_ID = 4;

            if (!await HasPermission(userId, READ_PERMISSION_ID))
                return Forbid("User does not have permission to assign patients to users.");

            using var command = new SqlCommand(@"
                IF EXISTS (SELECT 1 FROM UserPatientAssignmentMapping WHERE PatientId = @PatientId AND IsActive=1)
                THROW 50000, 'Patient already assigned to another user.', 1;

                INSERT INTO UserPatientAssignmentMapping (UserId, PatientId, AssignedBy, IsActive)
                VALUES (@UserId, @PatientId, @AssignedBy, @IsActive)", connection);

            command.Parameters.AddWithValue("@UserId", assignTo);
            command.Parameters.AddWithValue("@PatientId", patientId);
            command.Parameters.AddWithValue("@AssignedBy", userId);
            command.Parameters.AddWithValue("@IsActive", true);

            await command.ExecuteNonQueryAsync();
            return Ok("Patient assigned successfully.");
        }

        [HttpPatch("UnassignPatient")]
        public async Task<IActionResult> UnassignPatient(int userId, int patientId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            const int READ_PERMISSION_ID = 5;

            if (!await HasPermission(userId, READ_PERMISSION_ID))
                return Forbid("User does not have permission to read patient history.");

            using var command = new SqlCommand("UPDATE UserPatientAssignmentMapping SET IsActive=0 WHERE PatientId = @PatientId", connection);
            command.Parameters.AddWithValue("@PatientId", patientId);

            int affected = await command.ExecuteNonQueryAsync();
            if (affected == 0)
                return NotFound("Patient was not assigned.");

            return Ok("Patient unassigned successfully.");
        }
        [HttpGet("GetPatientHistory")]
        public async Task<IActionResult> GetHistory(int patientId, [FromQuery] int userId)
        {
            const int READ_PERMISSION_ID = 6;

            if (!await HasPermission(userId, READ_PERMISSION_ID))
                return Forbid("User does not have permission to read patient history.");

            var historyList = new List<PatientHistoryDTO>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand("SELECT PatientId, UserId, History FROM PatientsHistory WHERE PatientId = @PatientId", connection);
            cmd.Parameters.AddWithValue("@PatientId", patientId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                historyList.Add(new PatientHistoryDTO
                {
                    PatientId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    History = reader.GetString(2)
                });
            }

            return Ok(historyList);
        }

        [HttpPost("WritePatientHistory")]
        public async Task<IActionResult> WriteHistory(PatientHistoryDTO dto)
        {
            const int WRITE_PERMISSION_ID = 7;

            if (!await HasPermission(dto.UserId, WRITE_PERMISSION_ID))
                return Forbid("User does not have permission to write patient history.");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var cmd = new SqlCommand(@"
                INSERT INTO PatientsHistory (PatientId, UserId, History)
                VALUES (@PatientId, @UserId, @History)", connection);

            cmd.Parameters.AddWithValue("@PatientId", dto.PatientId);
            cmd.Parameters.AddWithValue("@UserId", dto.UserId);
            cmd.Parameters.AddWithValue("@History", dto.History);

            await cmd.ExecuteNonQueryAsync();

            return Ok("Patient history added successfully.");
        }
        [HttpGet("GetPatientAttachments")]
        public async Task<IActionResult> GetAttachments(int userId, int patientId)
        {
            const int READ_ATTACHMENT_PERMISSION_ID = 8;

            if (!await HasPermission(userId, READ_ATTACHMENT_PERMISSION_ID))
                return Forbid("User does not have permission to view attachments.");

            var attachments = new List<object>();
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("SELECT Id, FileName, UploadDate FROM PatientAttachments WHERE PatientId = @PatientId", connection);
            command.Parameters.AddWithValue("@PatientId", patientId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                attachments.Add(new
                {
                    Id = reader.GetInt32(0),
                    FileName = reader.GetString(1),
                    UploadDate = reader.GetDateTime(2)
                });
            }

            return Ok(attachments);
        }
        [HttpPost("UploadPatientAttachments")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAttachment([FromForm] UploadAttachmentDTO dto)
        {
            const int UPLOAD_ATTACHMENT_PERMISSION_ID = 9;

            if (!await HasPermission(dto.UserId, UPLOAD_ATTACHMENT_PERMISSION_ID))
                return Forbid("User does not have permission to upload attachments.");

            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("File is required.");

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "PatientAttachments");

            // Create the directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique file name
            string uniqueFileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file to disk
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(fileStream);
            }

            // Optionally, read file bytes to save in DB
            //byte[] fileBytes = await System.IO.File.ReadAllBytes(filePath);
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                INSERT INTO PatientAttachments (PatientId, FileName, FileData, UploadedBy, UploadDate)
                VALUES (@PatientId, @FileName, @FileData, @UploadedBy, @UploadDate)", connection);

            command.Parameters.AddWithValue("@PatientId", dto.PatientId);
            command.Parameters.AddWithValue("@FileName", uniqueFileName);
            command.Parameters.AddWithValue("@FileData", fileBytes);
            command.Parameters.AddWithValue("@UploadedBy", dto.UserId);
            command.Parameters.AddWithValue("@UploadDate", DateTime.UtcNow);

            await command.ExecuteNonQueryAsync();

            return Ok("Attachment uploaded and saved successfully.");
        }
        [HttpPost("SaveReport")]
        public async Task<IActionResult> SaveReport([FromBody] ReportDTO report)
        {
            const int WRITE_PERMISSION_ID = 8;
            if (!await HasPermission(report.RadiologistId, WRITE_PERMISSION_ID))
                return Forbid("Permission denied.");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            string query = report.Id == 0
                ? @"INSERT INTO Reports (PatientId, RadiologistId, ReportText, CreatedAt, UpdatedAt, Reviewed)
            VALUES (@PatientId, @RadiologistId, @ReportText, GETUTCDATE(), GETUTCDATE(), 0)"
                : @"UPDATE Reports SET ReportText = @ReportText, UpdatedAt = GETUTCDATE() WHERE Id = @Id";

            using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@PatientId", report.PatientId);
            cmd.Parameters.AddWithValue("@RadiologistId", report.RadiologistId);
            cmd.Parameters.AddWithValue("@ReportText", report.ReportText);
            if (report.Id != 0)
                cmd.Parameters.AddWithValue("@Id", report.Id);

            await cmd.ExecuteNonQueryAsync();
            return Ok("Report saved successfully.");
        }
        [HttpGet("GetReport")]
        public async Task<IActionResult> GetReport(int patientId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT TOP 1 * FROM Reports WHERE PatientId = @PatientId ORDER BY UpdatedAt DESC", connection);
            command.Parameters.AddWithValue("@PatientId", patientId);

            using var reader = await command.ExecuteReaderAsync();
            if (!reader.HasRows) return NotFound("No report found.");

            await reader.ReadAsync();

            var report = new
            {
                Id = reader["Id"],
                PatientId = reader["PatientId"],
                RadiologistId = reader["RadiologistId"],
                ReportText = reader["ReportText"],
                Reviewed = reader["Reviewed"]
            };

            return Ok(report);
        }
        [HttpGet("GetReportTemplates")]
        public async Task<IActionResult> GetReportTemplates()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT Id, Title, TemplateText FROM ReportTemplates", connection);
            var reader = await command.ExecuteReaderAsync();

            var templates = new List<object>();
            while (await reader.ReadAsync())
            {
                templates.Add(new
                {
                    Id = reader["Id"],
                    Title = reader["Title"],
                    TemplateText = reader["TemplateText"]
                });
            }

            return Ok(templates);
        }
        [HttpDelete("DeleteReport")]
        public async Task<IActionResult> DeleteReport(int reportId, int userId)
        {
            const int DELETE_PERMISSION_ID = 9;
            if (!await HasPermission(userId, DELETE_PERMISSION_ID))
                return Forbid("Permission denied.");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("DELETE FROM Reports WHERE Id = @Id", connection);
            command.Parameters.AddWithValue("@Id", reportId);

            int affected = await command.ExecuteNonQueryAsync();
            return affected > 0 ? Ok("Report deleted.") : NotFound("Report not found.");
        }
        [HttpPost("ReviewReport")]
        public async Task<IActionResult> ReviewReport(int reportId, int reviewerId)
        {
            const int REVIEW_PERMISSION_ID = 10;
            if (!await HasPermission(reviewerId, REVIEW_PERMISSION_ID))
                return Forbid("Permission denied.");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(
                @"UPDATE Reports SET Reviewed = 1, ReviewedBy = @ReviewerId, ReviewedAt = GETUTCDATE()
                WHERE Id = @ReportId", connection);

            command.Parameters.AddWithValue("@ReportId", reportId);
            command.Parameters.AddWithValue("@ReviewerId", reviewerId);

            int affected = await command.ExecuteNonQueryAsync();
            return affected > 0 ? Ok("Report reviewed.") : NotFound("Report not found."); 
        }
        [HttpGet("ExportPatientsToExcel")]
        public async Task<IActionResult> ExportToExcel()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT * FROM Patient", connection);
            var reader = await command.ExecuteReaderAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Patients");

            var dataTable = new DataTable();
            dataTable.Load(reader);
            worksheet.Cell(1, 1).InsertTable(dataTable);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Patients.xlsx");
        }
        [HttpPost("SetEmergencyPriority")]
        public async Task<IActionResult> SetEmergency(int patientId, string priority)
        {
            var validPriorities = new[] { "Low", "Medium", "High" };
            if (!validPriorities.Contains(priority))
                return BadRequest("Invalid priority.");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("UPDATE Patient SET Priority = @Priority WHERE Id = @PatientId", connection);
            command.Parameters.AddWithValue("@Priority", priority);
            command.Parameters.AddWithValue("@PatientId", patientId);

            await command.ExecuteNonQueryAsync();
            return Ok("Priority set.");
        }
        [HttpPost("RemoveEmergency")]
        public async Task<IActionResult> RemoveEmergency(int patientId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("UPDATE Patient SET Priority = NULL WHERE Id = @PatientId", connection);
            command.Parameters.AddWithValue("@PatientId", patientId);

            await command.ExecuteNonQueryAsync();
            return Ok("Priority removed.");
        }
        [HttpPost("EditPatient")]
        public async Task<IActionResult> EditPatient([FromBody] EditPatientDTO dto)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"UPDATE Patient 
        SET PatientName = @Name, Age = @Age, StudyDescription = @Study 
        WHERE Id = @Id", connection);

            command.Parameters.AddWithValue("@Name", dto.Name);
            command.Parameters.AddWithValue("@Age", dto.Age);
            command.Parameters.AddWithValue("@Study", dto.StudyDescription);
            command.Parameters.AddWithValue("@Id", dto.Id);

            await command.ExecuteNonQueryAsync();
            return Ok("Patient updated.");
        }
        [HttpPost("UploadSignature")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadSignature([FromForm] SignatureUploadDTO dto)
        {
            const int SIGNATURE_PERMISSION_ID = 11;

            if (!await HasPermission(dto.UserId, SIGNATURE_PERMISSION_ID))
                return Forbid("No permission to upload signature.");

            string folder = Path.Combine(Directory.GetCurrentDirectory(), "Signatures");
            Directory.CreateDirectory(folder);

            string filePath = Path.Combine(folder, $"{dto.UserId}_signature.png");

            using var stream = new FileStream(filePath, FileMode.Create);
            await dto.Signature.CopyToAsync(stream);

            return Ok("Signature uploaded.");
        }
        [HttpPost("SoftDeletePatient")]
        public async Task<IActionResult> SoftDeletePatient(int patientId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("UPDATE Patient SET IsDeleted = 1 WHERE Id = @PatientId", connection);
            command.Parameters.AddWithValue("@PatientId", patientId);

            await command.ExecuteNonQueryAsync();
            return Ok("Patient soft-deleted.");
        }
        [HttpDelete("HardDeletePatient")]
        public async Task<IActionResult> HardDeletePatient(int patientId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("DELETE FROM Patient WHERE Id = @PatientId", connection);
            command.Parameters.AddWithValue("@PatientId", patientId);

            await command.ExecuteNonQueryAsync();
            return Ok("Patient permanently deleted.");
        }
        [HttpPost("LockPatient")]
        public async Task<IActionResult> LockPatient(int userId, int patientId)
        {
            const int LOCK_PERMISSION_ID = 10;

            if (!await HasPermission(userId, LOCK_PERMISSION_ID))
                return Forbid("User does not have permission to lock patients.");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
        UPDATE Patient
        SET IsLocked = 1, LockedBy = @UserId
        WHERE Id = @PatientId AND (IsLocked = 0 OR LockedBy = @UserId)", connection);

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@PatientId", patientId);

            int affected = await command.ExecuteNonQueryAsync();
            if (affected == 0)
                return Conflict("Patient is already locked by another user.");

            return Ok("Patient locked successfully.");
        }
        [HttpPost("UnlockPatient")]
        public async Task<IActionResult> UnlockPatient(int userId, int patientId)
        {
            const int UNLOCK_PERMISSION_ID = 10;

            if (!await HasPermission(userId, UNLOCK_PERMISSION_ID))
                return Forbid("User does not have permission to unlock patients.");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
                UPDATE Patient
                SET IsLocked = 0, LockedBy = NULL
                WHERE Id = @PatientId AND (LockedBy = @UserId OR @IsAdmin = 1)", connection);

            command.Parameters.AddWithValue("@PatientId", patientId);
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@IsAdmin", await IsAdmin(userId) ? 1 : 0);

            int affected = await command.ExecuteNonQueryAsync();
            if (affected == 0)
                return Conflict("Patient is locked by another user. Admin required to unlock.");

            return Ok("Patient unlocked successfully.");
        }
        private async Task<bool> IsAdmin(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand(@"
            SELECT COUNT(*) FROM Users
            WHERE Id = @UserId AND UserCategoryId IN (1, 2) -- 1: SuperAdmin, 2: Admin", connection);
            command.Parameters.AddWithValue("@UserId", userId);

            int count = (int)await command.ExecuteScalarAsync();
            return count > 0;
        }
        [HttpGet("getAllActiveUsers")]
        public async Task<IActionResult> GetActiveUsers(int userId)
        {
            const int VIEW_USERS_PERMISSION_ID = 11;

            if (!await HasPermission(userId, VIEW_USERS_PERMISSION_ID))
                return Forbid("User does not have permission to view active sessions.");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("SELECT UserId, LoginTime, SessionId FROM UserSessions WHERE IsActive = 1", connection);
            using var reader = await command.ExecuteReaderAsync();

            var users = new List<object>();
            while (await reader.ReadAsync())
            {
                users.Add(new
                {
                    UserId = reader["UserId"],
                    LoginTime = reader["LoginTime"],
                    SessionId = reader["SessionId"]
                });
            }

            return Ok(users);
        }
        [HttpPost("KillLoginSession")]
        public async Task<IActionResult> KillUserSession(int userId, Guid sessionId)
        {
            const int KILL_SESSION_PERMISSION_ID = 12;

            if (!await HasPermission(userId, KILL_SESSION_PERMISSION_ID))
                return Forbid("User does not have permission to kill sessions.");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new SqlCommand("UPDATE UserSessions SET IsActive = 0, SessionKilled=1, SessionKilledBy=@UserId WHERE SessionId = @SessionId", connection);
            command.Parameters.AddWithValue("@SessionId", sessionId);
            command.Parameters.AddWithValue("@UserId", userId);

            int affected = await command.ExecuteNonQueryAsync();
            if (affected == 0)
                return NotFound("Session not found.");

            return Ok("Session killed successfully.");
        }
    }
}