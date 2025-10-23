using Microsoft.AspNetCore.Mvc;
using MediatR;
using ErpBE.API.Common;
using ErpBE.Application.UserManagement.Commands;
using ErpBE.Application.UserManagement.Queries;
using ErpBE.Domain.DTOs;

namespace ErpBE.API.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeAdmin] // Only Admin can manage roles
    public class RoleController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RoleController> _logger;

        public RoleController(IMediator mediator, ILogger<RoleController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            try
            {
                var command = new CreateRoleCommand { Request = request };
                var role = await _mediator.Send(command);
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
        [HttpPut]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest request)
        {
            try
            {
                var command = new UpdateRoleCommand { Request = request };
                var role = await _mediator.Send(command);
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
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var command = new DeleteRoleCommand { RoleId = id };
                var success = await _mediator.Send(command);
                if (success)
                {
                    return Ok(new { message = "Role deleted successfully" });
                }
                return NotFound(new { message = "Role not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoleById(int id)
        {
            try
            {
                var query = new GetRoleByIdQuery { RoleId = id };
                var role = await _mediator.Send(query);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }
                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get role by name
        /// </summary>
        [HttpGet("name/{name}")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoleByName(string name)
        {
            try
            {
                var query = new GetRoleByNameQuery { RoleName = name };
                var role = await _mediator.Send(query);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }
                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRoles()
        {
            try
            {
                var query = new GetAllRolesQuery();
                var roles = await _mediator.Send(query);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get active roles only
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveRoles()
        {
            try
            {
                var query = new GetActiveRolesQuery();
                var roles = await _mediator.Send(query);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active roles");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if role exists by name
        /// </summary>
        [HttpGet("check-exists/{name}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckRoleExists(string name)
        {
            try
            {
                var query = new CheckRoleExistsQuery { RoleName = name };
                var exists = await _mediator.Send(query);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
