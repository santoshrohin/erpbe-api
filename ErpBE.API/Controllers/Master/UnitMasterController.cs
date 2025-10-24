using MediatR;
using Microsoft.AspNetCore.Mvc;
using ErpBE.API.Common;
using ErpBE.Application.DTOs;
using ErpBE.Application.Common.Models;
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
        public async Task<IActionResult> CreateUnitMaster([FromBody] CreateUnitMasterRequest request)
        {
            // Validation is handled by FluentValidation pipeline
            var command = new CreateUnitMasterCommand { Request = request };
            var unitId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetUnitMasterById), new { id = unitId }, unitId);
        }

        /// <summary>
        /// Updates an existing unit master.
        /// </summary>
        /// <param name="request">Unit master update details.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUnitMaster([FromBody] UpdateUnitMasterRequest request)
        {
            // Validation is handled by FluentValidation pipeline
            var command = new UpdateUnitMasterCommand { Request = request };
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Deletes a unit master by ID (soft delete).
        /// </summary>
        /// <param name="id">The ID of the unit master to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteUnitMaster(int id)
        {
            // Validation is handled by FluentValidation pipeline
            var command = new DeleteUnitMasterCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Gets a unit master by ID.
        /// </summary>
        /// <param name="id">The ID of the unit master.</param>
        /// <returns>The unit master details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UnitMasterDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetUnitMasterById(int id)
        {
            // Validation is handled by FluentValidation pipeline
            var query = new GetUnitMasterByIdQuery { Id = id };
            var unit = await _mediator.Send(query);
            return Ok(unit);
        }

        /// <summary>
        /// Gets a unit master by name.
        /// </summary>
        /// <param name="unitName">The name of the unit master.</param>
        /// <param name="companyId">The company ID.</param>
        /// <returns>The unit master details.</returns>
        [HttpGet("name/{unitName}")]
        [ProducesResponseType(typeof(UnitMasterDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUnitMasterByName(string unitName, [FromQuery] int companyId)
        {
            var query = new GetUnitMasterByNameQuery { UnitName = unitName, CompanyId = companyId };
            var unit = await _mediator.Send(query);
            return Ok(unit);
        }

        /// <summary>
        /// Checks if a unit name is unique within a company.
        /// </summary>
        /// <param name="unitName">The unit name to check.</param>
        /// <param name="companyId">The company ID.</param>
        /// <param name="excludeId">Optional ID to exclude from uniqueness check (for updates).</param>
        /// <returns>Indication of whether the name is unique.</returns>
        [HttpGet("check-unique")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckUnitNameUnique([FromQuery] string unitName, [FromQuery] int companyId, [FromQuery] int? excludeId = null)
        {
            var query = new CheckUnitNameUniqueQuery 
            { 
                UnitName = unitName, 
                CompanyId = companyId, 
                ExcludeId = excludeId 
            };
            var isUnique = await _mediator.Send(query);
            return Ok(new { isUnique });
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SetUnitMasterActiveStatus(int id, [FromQuery] bool isActive)
        {
            // Validation is handled by FluentValidation pipeline
            var unit = await _mediator.Send(new GetUnitMasterByIdQuery { Id = id });

            var updateRequest = new UpdateUnitMasterRequest
            {
                Id = id,
                UnitName = unit!.UnitName,
                UnitDescription = unit.UnitDescription,
                IsActive = isActive
            };

            var command = new UpdateUnitMasterCommand { Request = updateRequest };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
