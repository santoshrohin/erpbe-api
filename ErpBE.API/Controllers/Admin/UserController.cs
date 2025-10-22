using Microsoft.AspNetCore.Mvc;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using ErpBE.Domain.CommonDto;
using ErpBE.API.Common;

namespace ErpBE.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeAdmin] // Only Admin can manage users
    public class UserController : ControllerBase
    {
        private readonly IUserManagementService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserManagementService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="request">User creation details</param>
        /// <returns>Created user information</returns>
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = await _userService.CreateUserAsync(request);
                return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, user);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("already exists"))
                {
                    return Conflict(new { message = ex.Message });
                }
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="request">User update details</param>
        /// <returns>Updated user information</returns>
        [HttpPut]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(request);
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var success = await _userService.DeleteUserAsync(id);
                if (success)
                {
                    return Ok(new { message = "User deleted successfully" });
                }
                return NotFound(new { message = "User not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User information</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User information</returns>
        [HttpGet("username/{username}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                var user = await _userService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by username: {Username}", username);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all users with pagination and filtering
        /// </summary>
        /// <param name="parameters">Query parameters</param>
        /// <returns>Paginated list of users</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsers([FromQuery] QueryParameters parameters)
        {
            try
            {
                var users = await _userService.GetUsersAsync(parameters);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get users by company
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>List of users</returns>
        [HttpGet("company/{companyId}")]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersByCompany(int companyId)
        {
            try
            {
                var users = await _userService.GetUsersByCompanyAsync(companyId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by company: {CompanyId}", companyId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="request">Password change details</param>
        /// <returns>Success status</returns>
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var success = await _userService.ChangePasswordAsync(request);
                if (success)
                {
                    return Ok(new { message = "Password changed successfully" });
                }
                return BadRequest(new { message = "Failed to change password" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", request.UserId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Activate or deactivate user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="isActive">Activation status</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ActivateUser(int id, [FromBody] bool isActive)
        {
            try
            {
                var success = await _userService.ActivateUserAsync(id, isActive);
                if (success)
                {
                    return Ok(new { message = $"User {(isActive ? "activated" : "deactivated")} successfully" });
                }
                return BadRequest(new { message = "Failed to change user status" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing activation status for user: {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Assign roles to user
        /// </summary>
        /// <param name="request">Role assignment details</param>
        /// <returns>Success status</returns>
        [HttpPost("assign-roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesRequest request)
        {
            try
            {
                var success = await _userService.AssignRolesToUserAsync(request);
                if (success)
                {
                    return Ok(new { message = "Roles assigned successfully" });
                }
                return BadRequest(new { message = "Failed to assign roles" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning roles to user: {UserId}", request.UserId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Remove roles from user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="roles">Roles to remove</param>
        /// <returns>Success status</returns>
        [HttpPost("{id}/remove-roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveRoles(int id, [FromBody] List<string> roles)
        {
            try
            {
                var success = await _userService.RemoveRolesFromUserAsync(id, roles);
                if (success)
                {
                    return Ok(new { message = "Roles removed successfully" });
                }
                return BadRequest(new { message = "Failed to remove roles" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing roles from user: {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user roles
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>List of user roles</returns>
        [HttpGet("{id}/roles")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserRoles(int id)
        {
            try
            {
                var roles = await _userService.GetUserRolesAsync(id);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles for user: {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        /// <param name="roleName">Role name</param>
        /// <returns>List of users with the role</returns>
        [HttpGet("by-role/{roleName}")]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUsersByRole(string roleName)
        {
            try
            {
                var users = await _userService.GetUsersByRoleAsync(roleName);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role: {RoleName}", roleName);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>Existence status</returns>
        [HttpGet("check-username/{username}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckUsernameExists(string username)
        {
            try
            {
                var exists = await _userService.UsernameExistsAsync(username);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username existence: {Username}", username);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>Existence status</returns>
        [HttpGet("check-email/{email}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            try
            {
                var exists = await _userService.EmailExistsAsync(email);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence: {Email}", email);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
