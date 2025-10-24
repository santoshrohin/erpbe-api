using ErpBE.Application.Common.Models;
using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.CustomerMaster.Queries;
using ErpBE.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Sales
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerMasterController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerMasterController> _logger;

        public CustomerMasterController(IMediator mediator, ILogger<CustomerMasterController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all customer masters with filtering, searching, and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<CustomerMasterDto>>> GetAll([FromQuery] CustomerMasterQueryParameters parameters)
        {
            _logger.LogInformation("Getting customer masters with parameters: {@Parameters}", parameters);
            
            var query = new GetCustomerMastersQuery { Parameters = parameters };
            var result = await _mediator.Send(query);
            
            return Ok(result);
        }

        /// <summary>
        /// Get customer master by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerMasterDto>> GetById(int id, [FromQuery] int companyId)
        {
            _logger.LogInformation("Getting customer master with ID: {Id}, CompanyId: {CompanyId}", id, companyId);
            
            var query = new GetCustomerMasterByIdQuery { Id = id, CompanyId = companyId };
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                _logger.LogWarning("Customer master not found. ID: {Id}, CompanyId: {CompanyId}", id, companyId);
                return NotFound(new { message = $"Customer master with ID '{id}' not found." });
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Create a new customer master
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CustomerMasterDto>> Create([FromBody] CreateCustomerMasterRequest request)
        {
            _logger.LogInformation("Creating new customer master: {@Request}", request);
            
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = request.CompanyId,
                PartyName = request.PartyName,
                ContactPerson = request.ContactPerson,
                Abbreviation = request.Abbreviation,
                VendorCode = request.VendorCode,
                Address = request.Address,
                Phone = request.Phone,
                Mobile = request.Mobile,
                Email = request.Email,
                FaxNo = request.FaxNo,
                PinCode = request.PinCode,
                AreaCode = request.AreaCode,
                CustomerType = request.CustomerType,
                CountryCode = request.CountryCode,
                StateCode = request.StateCode,
                CityCode = request.CityCode,
                CategoryCode = request.CategoryCode,
                EmployeeCode = request.EmployeeCode,
                PanNo = request.PanNo,
                CstNo = request.CstNo,
                VatNo = request.VatNo,
                ServiceTaxNo = request.ServiceTaxNo,
                EccNo = request.EccNo,
                LbtNo = request.LbtNo,
                ExciseRange = request.ExciseRange,
                ExciseDivision = request.ExciseDivision,
                ExciseCollectorate = request.ExciseCollectorate,
                TallyName = request.TallyName,
                CreditDays = request.CreditDays,
                TdsPercentage = request.TdsPercentage,
                IsActive = request.IsActive,
                IsLbtApplicable = request.IsLbtApplicable
            };
            
            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Customer master created successfully with ID: {Id}", result.Id);
            return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = result.CompanyId }, result);
        }

        /// <summary>
        /// Update an existing customer master
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerMasterRequest request)
        {
            if (id != request.Id)
            {
                _logger.LogWarning("ID mismatch. Route ID: {RouteId}, Request ID: {RequestId}", id, request.Id);
                return BadRequest(new { message = "ID mismatch" });
            }
            
            _logger.LogInformation("Updating customer master with ID: {Id}", id);
            
            var command = new UpdateCustomerMasterCommand
            {
                Id = request.Id,
                CompanyId = request.CompanyId,
                PartyCode = request.PartyCode,
                PartyName = request.PartyName,
                ContactPerson = request.ContactPerson,
                Abbreviation = request.Abbreviation,
                VendorCode = request.VendorCode,
                Address = request.Address,
                Phone = request.Phone,
                Mobile = request.Mobile,
                Email = request.Email,
                FaxNo = request.FaxNo,
                PinCode = request.PinCode,
                AreaCode = request.AreaCode,
                CustomerType = request.CustomerType,
                CountryCode = request.CountryCode,
                StateCode = request.StateCode,
                CityCode = request.CityCode,
                CategoryCode = request.CategoryCode,
                EmployeeCode = request.EmployeeCode,
                PanNo = request.PanNo,
                CstNo = request.CstNo,
                VatNo = request.VatNo,
                ServiceTaxNo = request.ServiceTaxNo,
                EccNo = request.EccNo,
                LbtNo = request.LbtNo,
                ExciseRange = request.ExciseRange,
                ExciseDivision = request.ExciseDivision,
                ExciseCollectorate = request.ExciseCollectorate,
                TallyName = request.TallyName,
                CreditDays = request.CreditDays,
                TdsPercentage = request.TdsPercentage,
                IsActive = request.IsActive,
                IsLbtApplicable = request.IsLbtApplicable
            };
            
            await _mediator.Send(command);
            
            _logger.LogInformation("Customer master updated successfully. ID: {Id}", id);
            return NoContent();
        }

        /// <summary>
        /// Delete a customer master
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] int companyId)
        {
            _logger.LogInformation("Deleting customer master with ID: {Id}, CompanyId: {CompanyId}", id, companyId);
            
            var command = new DeleteCustomerMasterCommand { Id = id, CompanyId = companyId };
            await _mediator.Send(command);
            
            _logger.LogInformation("Customer master deleted successfully. ID: {Id}", id);
            return NoContent();
        }

        /// <summary>
        /// Check if party name is unique
        /// </summary>
        [HttpGet("check-partyname")]
        public async Task<ActionResult<bool>> CheckPartyNameUnique([FromQuery] string partyName, [FromQuery] int? id, [FromQuery] int companyId)
        {
            var query = new CheckPartyNameUniqueQuery 
            { 
                PartyName = partyName, 
                Id = id, 
                CompanyId = companyId 
            };
            var isUnique = await _mediator.Send(query);
            return Ok(new { isUnique });
        }

        /// <summary>
        /// Check if abbreviation is unique
        /// </summary>
        [HttpGet("check-abbreviation")]
        public async Task<ActionResult<bool>> CheckAbbreviationUnique([FromQuery] string abbreviation, [FromQuery] int? id, [FromQuery] int companyId)
        {
            var query = new CheckAbbreviationUniqueQuery 
            { 
                Abbreviation = abbreviation, 
                Id = id, 
                CompanyId = companyId 
            };
            var isUnique = await _mediator.Send(query);
            return Ok(new { isUnique });
        }
    }
}

