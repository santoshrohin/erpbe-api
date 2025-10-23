using Microsoft.AspNetCore.Mvc;
using MediatR;
using ErpBE.API.Common;
using ErpBE.Application.UserManagement.Commands;
using ErpBE.Application.UserManagement.Queries;
using ErpBE.Application.DTOs;
using ErpBE.Application.Common.Models;

namespace ErpBE.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeAdmin] // Only Admin can manage users
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserController> _logger;

        public UserController(IMediator mediator, ILogger<UserController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var command = new CreateUserCommand { Request = request };
                var user = await _mediator.Send(command);
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
        [HttpPut]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            try
            {
                var command = new UpdateUserCommand { Request = request };
                var user = await _mediator.Send(command);
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
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var command = new DeleteUserCommand { UserId = id };
                var success = await _mediator.Send(command);
                if (success)
                {
                    return Ok(new { message = "User deleted successfully" });
                }
                return NotFound(new { message = "User not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var query = new GetUserByIdQuery { UserId = id };
                var user = await _mediator.Send(query);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        [HttpGet("username/{username}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                var query = new GetUserByUsernameQuery { Username = username };
                var user = await _mediator.Send(query);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all users with pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers([FromQuery] QueryParameters parameters)
        {
            try
            {
                var query = new GetUsersQuery { QueryParameters = parameters };
                var users = await _mediator.Send(query);
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
        [HttpGet("company/{companyId}")]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsersByCompany(int companyId)
        {
            try
            {
                var query = new GetUsersByCompanyQuery { CompanyId = companyId };
                var users = await _mediator.Send(query);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var command = new ChangePasswordCommand { Request = request };
                var success = await _mediator.Send(command);
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
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Activate or deactivate a user
        /// </summary>
        [HttpPost("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ActivateUser(int id, [FromBody] bool isActive)
        {
            try
            {
                var command = new SetUserActiveStatusCommand { UserId = id, IsActive = isActive };
                var success = await _mediator.Send(command);
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
                _logger.LogError(ex, "Error changing user status");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Assign roles to a user
        /// </summary>
        [HttpPost("{id}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignRoles(int id, [FromBody] AssignRolesRequest request)
        {
            try
            {
                request.UserId = id;
                var command = new AssignRolesToUserCommand { Request = request };
                var success = await _mediator.Send(command);
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
                _logger.LogError(ex, "Error assigning roles");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Remove roles from a user
        /// </summary>
        [HttpDelete("{id}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveRoles(int id, [FromBody] List<string> roles)
        {
            try
            {
                var command = new RemoveRolesFromUserCommand { UserId = id, Roles = roles };
                var success = await _mediator.Send(command);
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
                _logger.LogError(ex, "Error removing roles");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get roles for a user
        /// </summary>
        [HttpGet("{id}/roles")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserRoles(int id)
        {
            try
            {
                var query = new GetUserRolesQuery { UserId = id };
                var roles = await _mediator.Send(query);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user roles");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get users by role
        /// </summary>
        [HttpGet("role/{roleName}")]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsersByRole(string roleName)
        {
            try
            {
                var query = new GetUsersByRoleQuery { RoleName = roleName };
                var users = await _mediator.Send(query);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        [HttpGet("check-username/{username}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckUsername(string username)
        {
            try
            {
                var query = new CheckUsernameExistsQuery { Username = username };
                var exists = await _mediator.Send(query);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        [HttpGet("check-email/{email}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckEmail(string email)
        {
            try
            {
                var query = new CheckEmailExistsQuery { Email = email };
                var exists = await _mediator.Send(query);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
