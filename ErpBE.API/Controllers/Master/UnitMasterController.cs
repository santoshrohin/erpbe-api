using MediatR;
using Microsoft.AspNetCore.Mvc;
using ErpBE.API.Common;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.CommonDto;
using ErpBE.Application.UnitMaster.Commands;
using ErpBE.Application.UnitMaster.Queries;

namespace ErpBE.API.Controllers.Master
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthorizeAdmin] // Only Admin can manage unit masters
    public class UnitMasterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UnitMasterController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new unit master.
        /// </summary>
        /// <param name="request">Unit master creation details.</param>
        /// <returns>The ID of the newly created unit master.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateUnitMaster([FromBody] CreateUnitMasterRequest request)
        {
            try
            {
                var command = new CreateUnitMasterCommand { Request = request };
                var unitId = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetUnitMasterById), new { id = unitId }, unitId);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating unit master", detail = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing unit master.
        /// </summary>
        /// <param name="request">Unit master update details.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateUnitMaster([FromBody] UpdateUnitMasterRequest request)
        {
            try
            {
                var command = new UpdateUnitMasterCommand { Request = request };
                var updated = await _mediator.Send(command);
                if (!updated)
                {
                    return NotFound(new { message = $"Unit master with ID '{request.Id}' not found." });
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating unit master", detail = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a unit master by ID (soft delete).
        /// </summary>
        /// <param name="id">The ID of the unit master to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUnitMaster(int id)
        {
            try
            {
                var command = new DeleteUnitMasterCommand { Id = id };
                var deleted = await _mediator.Send(command);
                if (!deleted)
                {
                    return NotFound(new { message = $"Unit master with ID '{id}' not found." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting unit master", detail = ex.Message });
            }
        }

        /// <summary>
        /// Gets a unit master by ID.
        /// </summary>
        /// <param name="id">The ID of the unit master.</param>
        /// <returns>The unit master details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UnitMasterDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUnitMasterById(int id)
        {
            var query = new GetUnitMasterByIdQuery { Id = id };
            var unit = await _mediator.Send(query);
            if (unit == null)
            {
                return NotFound(new { message = $"Unit master with ID '{id}' not found." });
            }
            return Ok(unit);
        }

        /// <summary>
        /// Gets a paginated list of unit masters with server-side filtering, searching, and sorting.
        /// </summary>
        /// <param name="queryParameters">Query parameters for pagination and filtering.</param>
        /// <returns>A paginated list of unit masters.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<UnitMasterDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnitMasters([FromQuery] UnitMasterQueryParameters queryParameters)
        {
            var query = new GetUnitMastersQuery { QueryParameters = queryParameters };
            var units = await _mediator.Send(query);
            return Ok(units);
        }

        /// <summary>
        /// Sets a unit master's active status.
        /// </summary>
        /// <param name="id">The ID of the unit master.</param>
        /// <param name="isActive">The new active status.</param>
        /// <returns>No content if successful.</returns>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetUnitMasterActiveStatus(int id, [FromQuery] bool isActive)
        {
            try
            {
                // This would need a separate command/handler for setting active status
                // For now, we'll use the update endpoint
                var unit = await _mediator.Send(new GetUnitMasterByIdQuery { Id = id });
                if (unit == null)
                {
                    return NotFound(new { message = $"Unit master with ID '{id}' not found." });
                }

                var updateRequest = new UpdateUnitMasterRequest
                {
                    Id = id,
                    UnitName = unit.UnitName,
                    UnitDescription = unit.UnitDescription,
                    IsActive = isActive
                };

                var command = new UpdateUnitMasterCommand { Request = updateRequest };
                var updated = await _mediator.Send(command);
                if (!updated)
                {
                    return StatusCode(500, new { message = "Failed to update unit master status." });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error setting unit master active status", detail = ex.Message });
            }
        }

        /// <summary>
        /// Debug endpoint to test stored procedure directly.
        /// </summary>
        [HttpGet("debug")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DebugUnitMasters()
        {
            try
            {
                var queryParameters = new UnitMasterQueryParameters
                {
                    PageNumber = 1,
                    PageSize = 10,
                    CompanyId = 1,
                    IsActive = true
                };

                var query = new GetUnitMastersQuery { QueryParameters = queryParameters };
                var units = await _mediator.Send(query);
                
                return Ok(new { 
                    message = "Debug successful", 
                    totalCount = units.TotalCount,
                    dataCount = units.Data.Count,
                    data = units.Data,
                    queryParameters = queryParameters
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Debug failed", 
                    error = ex.Message, 
                    stackTrace = ex.StackTrace 
                });
            }
        }
    }
}
