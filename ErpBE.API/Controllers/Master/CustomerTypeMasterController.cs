using ErpBE.API.Common;
using ErpBE.Application.Common.Models;
using ErpBE.Application.CustomerTypeMaster.Commands;
using ErpBE.Application.CustomerTypeMaster.Queries;
using ErpBE.Application.DTOs;
using ErpBE.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Master
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerTypeMasterController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerTypeMasterController> _logger;

        public CustomerTypeMasterController(
            IMediator mediator,
            ILogger<CustomerTypeMasterController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all customer type masters with pagination, filtering, and sorting
        /// </summary>
        [HttpGet]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.View)]
        [ProducesResponseType(typeof(PagedResponse<CustomerTypeMasterDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResponse<CustomerTypeMasterDto>>> GetCustomerTypeMasters(
            [FromQuery] CustomerTypeMasterQueryParameters parameters,
            CancellationToken cancellationToken)
        {
            var query = new GetCustomerTypeMastersQuery { Parameters = parameters };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get customer type master by ID
        /// </summary>
        [HttpGet("{id}")]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.View)]
        [ProducesResponseType(typeof(CustomerTypeMasterDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerTypeMasterDto>> GetCustomerTypeMasterById(
            int id,
            [FromQuery] int companyId,
            CancellationToken cancellationToken)
        {
            var query = new GetCustomerTypeMasterByIdQuery { Id = id, CompanyId = companyId };
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
            {
                return NotFound(new { message = $"Customer Type Master with ID '{id}' not found." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Get customer type master by type code
        /// </summary>
        [HttpGet("byTypeCode/{typeCode}")]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.View)]
        [ProducesResponseType(typeof(CustomerTypeMasterDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerTypeMasterDto>> GetCustomerTypeMasterByTypeCode(
            string typeCode,
            [FromQuery] int companyId,
            CancellationToken cancellationToken)
        {
            var query = new GetCustomerTypeMasterByTypeCodeQuery 
            { 
                TypeCode = typeCode, 
                CompanyId = companyId 
            };
            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
            {
                return NotFound(new { message = $"Customer Type Master with Type Code '{typeCode}' not found." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Check if type code is unique
        /// </summary>
        [HttpGet("checkUnique")]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.View)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> CheckTypeCodeUnique(
            [FromQuery] string typeCode,
            [FromQuery] int? excludeId,
            [FromQuery] int companyId,
            CancellationToken cancellationToken)
        {
            var query = new CheckTypeCodeUniqueQuery 
            { 
                TypeCode = typeCode, 
                ExcludeId = excludeId, 
                CompanyId = companyId 
            };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Create a new customer type master
        /// </summary>
        [HttpPost]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.Add)]
        [ProducesResponseType(typeof(CustomerTypeMasterDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerTypeMasterDto>> CreateCustomerTypeMaster(
            [FromBody] CreateCustomerTypeMasterRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateCustomerTypeMasterCommand
            {
                CompanyId = request.CompanyId,
                TypeCode = request.TypeCode,
                TypeDescription = request.TypeDescription,
                FirstLetter = request.FirstLetter
            };

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(
                nameof(GetCustomerTypeMasterById),
                new { id = result.Id, companyId = result.CompanyId },
                result);
        }

        /// <summary>
        /// Update an existing customer type master
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.Edit)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCustomerTypeMaster(
            int id,
            [FromBody] UpdateCustomerTypeMasterRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateCustomerTypeMasterCommand
            {
                Id = id,
                CompanyId = request.CompanyId,
                TypeCode = request.TypeCode,
                TypeDescription = request.TypeDescription,
                FirstLetter = request.FirstLetter
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Delete a customer type master (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission(ModuleCodes.Masters, PermissionBit.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCustomerTypeMaster(
            int id,
            [FromQuery] int companyId,
            CancellationToken cancellationToken)
        {
            var command = new DeleteCustomerTypeMasterCommand { Id = id, CompanyId = companyId };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
    }
}

