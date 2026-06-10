using ErpBE.API.Common;
using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using ErpBE.Application.SoTypeMaster.Commands;
using ErpBE.Application.SoTypeMaster.Queries;
using ErpBE.Application.SoTypeMaster.Validators;
using ErpBE.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Master
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SoTypeMasterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SoTypeMasterController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get all SO Type Masters with pagination, filtering, and sorting
        /// </summary>
        [HttpGet]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.View)]
        public async Task<IActionResult> GetSoTypeMasters([FromQuery] SoTypeMasterQueryParameters queryParameters)
        {
            var validator = new SoTypeMasterQueryParametersValidator();
            var validationResult = await validator.ValidateAsync(queryParameters);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = validationResult.Errors.GroupBy(x => x.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray())
                });
            }

            var query = new GetSoTypeMastersQuery { QueryParameters = queryParameters };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get SO Type Master by ID
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.View)]
        public async Task<IActionResult> GetSoTypeMasterById(int id)
        {
            var query = new GetSoTypeMasterByIdQuery { Id = id };
            var soType = await _mediator.Send(query);
            return Ok(soType);
        }

        /// <summary>
        /// Get SO Type Master by Short Name
        /// </summary>
        [HttpGet("by-name/{shortName}")]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.View)]
        public async Task<IActionResult> GetSoTypeMasterByShortName(string shortName, [FromQuery] int companyId)
        {
            var query = new GetSoTypeMasterByShortNameQuery
            {
                ShortName = shortName,
                CompanyId = companyId
            };
            var soType = await _mediator.Send(query);
            return Ok(soType);
        }

        /// <summary>
        /// Check if Short Name is unique
        /// </summary>
        [HttpGet("check-unique")]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.View)]
        public async Task<IActionResult> CheckShortNameUnique([FromQuery] string shortName, [FromQuery] int companyId, [FromQuery] int? excludeId = null)
        {
            var query = new CheckSoTypeShortNameUniqueQuery
            {
                ShortName = shortName,
                CompanyId = companyId,
                ExcludeId = excludeId
            };
            var isUnique = await _mediator.Send(query);
            return Ok(new { isUnique });
        }

        /// <summary>
        /// Create a new SO Type Master
        /// </summary>
        [HttpPost]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.Add)]
        public async Task<IActionResult> CreateSoTypeMaster([FromBody] CreateSoTypeMasterRequest request)
        {
            var validator = new CreateSoTypeMasterRequestValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = validationResult.Errors.GroupBy(x => x.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray())
                });
            }

            var command = new CreateSoTypeMasterCommand { Request = request };
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetSoTypeMasterById), new { id }, new { id });
        }

        /// <summary>
        /// Update an existing SO Type Master
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.Edit)]
        public async Task<IActionResult> UpdateSoTypeMaster(int id, [FromBody] UpdateSoTypeMasterRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest(new { message = "ID in URL does not match ID in request body." });
            }

            var validator = new UpdateSoTypeMasterRequestValidator();
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = validationResult.Errors.GroupBy(x => x.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray())
                });
            }

            var command = new UpdateSoTypeMasterCommand { Request = request };
            await _mediator.Send(command);
            return NoContent();
        }

        /// <summary>
        /// Delete an SO Type Master (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.Delete)]
        public async Task<IActionResult> DeleteSoTypeMaster(int id)
        {
            var command = new DeleteSoTypeMasterCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}

