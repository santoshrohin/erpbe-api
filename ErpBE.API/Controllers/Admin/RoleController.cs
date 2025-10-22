using Microsoft.AspNetCore.Mvc;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using ErpBE.API.Common;

namespace ErpBE.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeAdmin] // Only Admin can manage roles
    public class RoleController : ControllerBase
    {
        private readonly IUserManagementService _userService;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IUserManagementService userService, ILogger<RoleController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="request">Role creation details</param>
        /// <returns>Created role information</returns>
        [HttpPost]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            try
            {
                var role = await _userService.CreateRoleAsync(request);
                return CreatedAtAction(nameof(GetRoleById), new { id = role.RoleId }, role);
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
                _logger.LogError(ex, "Error creating role");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update an existing role
        /// </summary>
        /// <param name="request">Role update details</param>
        /// <returns>Updated role information</returns>
        [HttpPut]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest request)
        {
            try
            {
                var role = await _userService.UpdateRoleAsync(request);
                return Ok(role);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete a role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var success = await _userService.DeleteRoleAsync(id);
                if (success)
                {
                    return Ok(new { message = "Role deleted successfully" });
                }
                return NotFound(new { message = "Role not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role: {RoleId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns>Role information</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoleById(int id)
        {
            try
            {
                var role = await _userService.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }
                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by ID: {RoleId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get role by name
        /// </summary>
        /// <param name="name">Role name</param>
        /// <returns>Role information</returns>
        [HttpGet("name/{name}")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoleByName(string name)
        {
            try
            {
                var role = await _userService.GetRoleByNameAsync(name);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }
                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by name: {RoleName}", name);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        /// <returns>List of all roles</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var roles = await _userService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all roles");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get active roles only
        /// </summary>
        /// <returns>List of active roles</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetActiveRoles()
        {
            try
            {
                var roles = await _userService.GetActiveRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active roles");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if role exists
        /// </summary>
        /// <param name="name">Role name to check</param>
        /// <returns>Existence status</returns>
        [HttpGet("check-exists/{name}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckRoleExists(string name)
        {
            try
            {
                var exists = await _userService.RoleExistsAsync(name);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role existence: {RoleName}", name);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
