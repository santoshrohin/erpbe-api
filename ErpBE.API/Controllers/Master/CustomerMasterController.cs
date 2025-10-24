using ErpBE.Application.Common.Models;
using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.CustomerMaster.Queries;
using ErpBE.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Master
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerMasterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerMasterController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Create a new Customer Master record
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CustomerMasterDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerMasterDto>> CreateCustomerMaster(
            [FromBody] CreateCustomerMasterRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateCustomerMasterCommand
            {
                CompanyId = request.CompanyId,
                PartyName = request.PartyName,
                ContactPerson = request.ContactPerson,
                Abbreviation = request.Abbreviation,
                Address = request.Address,
                Phone = request.Phone,
                Mobile = request.Mobile,
                Email = request.Email,
                Website = request.Website,
                FaxNo = request.FaxNo,
                AreaCode = request.AreaCode,
                CustomerType = request.CustomerType,
                CountryCode = request.CountryCode,
                StateCode = request.StateCode,
                CityCode = request.CityCode,
                PinCode = request.PinCode,
                VatTinNo = request.VatTinNo,
                CstNo = request.CstNo,
                GstNo = request.GstNo,
                PanNo = request.PanNo,
                ServiceTaxNo = request.ServiceTaxNo,
                TallyName = request.TallyName,
                OpeningBalance = request.OpeningBalance,
                OpeningBalanceType = request.OpeningBalanceType,
                CreditLimit = request.CreditLimit,
                CreditDays = request.CreditDays,
                BankName = request.BankName,
                BankAccountNo = request.BankAccountNo,
                BankBranchName = request.BankBranchName,
                BankIfscCode = request.BankIfscCode,
                IsLbtApplicable = request.IsLbtApplicable,
                IsSezCustomer = request.IsSezCustomer,
                IsCompositeDealer = request.IsCompositeDealer,
                Remark = request.Remark
            };

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(
                nameof(GetCustomerMasterById),
                new { id = result.Id, companyId = result.CompanyId },
                result);
        }

        /// <summary>
        /// Update an existing Customer Master record
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCustomerMaster(
            int id,
            [FromBody] UpdateCustomerMasterRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateCustomerMasterCommand
            {
                Id = id,
                CompanyId = request.CompanyId,
                PartyCode = request.PartyCode,
                PartyName = request.PartyName,
                ContactPerson = request.ContactPerson,
                Abbreviation = request.Abbreviation,
                Address = request.Address,
                Phone = request.Phone,
                Mobile = request.Mobile,
                Email = request.Email,
                Website = request.Website,
                FaxNo = request.FaxNo,
                AreaCode = request.AreaCode,
                CustomerType = request.CustomerType,
                CountryCode = request.CountryCode,
                StateCode = request.StateCode,
                CityCode = request.CityCode,
                PinCode = request.PinCode,
                VatTinNo = request.VatTinNo,
                CstNo = request.CstNo,
                GstNo = request.GstNo,
                PanNo = request.PanNo,
                ServiceTaxNo = request.ServiceTaxNo,
                TallyName = request.TallyName,
                OpeningBalance = request.OpeningBalance,
                OpeningBalanceType = request.OpeningBalanceType,
                CreditLimit = request.CreditLimit,
                CreditDays = request.CreditDays,
                BankName = request.BankName,
                BankAccountNo = request.BankAccountNo,
                BankBranchName = request.BankBranchName,
                BankIfscCode = request.BankIfscCode,
                IsLbtApplicable = request.IsLbtApplicable,
                IsSezCustomer = request.IsSezCustomer,
                IsCompositeDealer = request.IsCompositeDealer,
                Remark = request.Remark
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Delete a Customer Master record (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCustomerMaster(
            int id,
            [FromQuery] int companyId,
            CancellationToken cancellationToken)
        {
            var command = new DeleteCustomerMasterCommand
            {
                Id = id,
                CompanyId = companyId
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Get a single Customer Master record by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CustomerMasterDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerMasterDto>> GetCustomerMasterById(
            int id,
            [FromQuery] int companyId,
            CancellationToken cancellationToken)
        {
            var query = new GetCustomerMasterByIdQuery
            {
                Id = id,
                CompanyId = companyId
            };

            var result = await _mediator.Send(query, cancellationToken);

            if (result == null)
            {
                return NotFound(new { message = $"Customer Master with ID '{id}' not found." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Get a paginated list of Customer Master records with filtering, searching, and sorting
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<CustomerMasterDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResponse<CustomerMasterDto>>> GetCustomerMasters(
            [FromQuery] CustomerMasterQueryParameters parameters,
            CancellationToken cancellationToken)
        {
            var query = new GetCustomerMastersQuery
            {
                Parameters = parameters
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Check if a Party Name is unique
        /// </summary>
        [HttpGet("check-party-name-unique")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> CheckPartyNameUnique(
            [FromQuery] string partyName,
            [FromQuery] int? id,
            [FromQuery] int companyId,
            CancellationToken cancellationToken)
        {
            var query = new CheckPartyNameUniqueQuery
            {
                PartyName = partyName,
                Id = id,
                CompanyId = companyId
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Check if an Abbreviation is unique
        /// </summary>
        [HttpGet("check-abbreviation-unique")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> CheckAbbreviationUnique(
            [FromQuery] string abbreviation,
            [FromQuery] int? id,
            [FromQuery] int companyId,
            CancellationToken cancellationToken)
        {
            var query = new CheckAbbreviationUniqueQuery
            {
                Abbreviation = abbreviation,
                Id = id,
                CompanyId = companyId
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}

