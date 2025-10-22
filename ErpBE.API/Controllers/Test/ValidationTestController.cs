using Microsoft.AspNetCore.Mvc;
using ErpBE.Domain.DTOs;
using MediatR;
using ErpBE.Application.UnitMaster.Commands;
using ErpBE.Application.UnitMaster.Queries;

namespace ErpBE.API.Controllers.Test
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValidationTestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ValidationTestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Test Unit Master validation with invalid data
        /// </summary>
        [HttpPost("unit-master/invalid")]
        public async Task<IActionResult> TestInvalidUnitMaster()
        {
            var invalidRequest = new CreateUnitMasterRequest
            {
                UnitName = "", // Invalid: Empty name
                UnitDescription = new string('A', 101), // Invalid: Too long
                CompanyId = -1, // Invalid: Negative ID
                IsActive = true
            };

            try
            {
                var result = await _mediator.Send(new CreateUnitMasterCommand { Request = invalidRequest });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, type = ex.GetType().Name });
            }
        }

        /// <summary>
        /// Test Unit Master validation with valid data
        /// </summary>
        [HttpPost("unit-master/valid")]
        public async Task<IActionResult> TestValidUnitMaster()
        {
            var validRequest = new CreateUnitMasterRequest
            {
                UnitName = "KG",
                UnitDescription = "Kilogram",
                CompanyId = 1,
                IsActive = true
            };

            try
            {
                var result = await _mediator.Send(new CreateUnitMasterCommand { Request = validRequest });
                return Ok(new { message = "Validation passed", result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, type = ex.GetType().Name });
            }
        }

        /// <summary>
        /// Test query validation with invalid parameters
        /// </summary>
        [HttpGet("unit-master/query/invalid")]
        public async Task<IActionResult> TestInvalidQuery()
        {
            var invalidQuery = new UnitMasterQueryParameters
            {
                PageNumber = -1, // Invalid: Negative page
                PageSize = 1000, // Invalid: Too large
                CompanyId = -5, // Invalid: Negative ID
                UnitName = new string('A', 11), // Invalid: Too long
                SearchTerm = new string('B', 256), // Invalid: Too long
                SortBy = "InvalidField", // Invalid: Not in allowed list
                SortDirection = "INVALID" // Invalid: Not ASC or DESC
            };

            try
            {
                var result = await _mediator.Send(new GetUnitMastersQuery { QueryParameters = invalidQuery });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, type = ex.GetType().Name });
            }
        }

        /// <summary>
        /// Test query validation with valid parameters
        /// </summary>
        [HttpGet("unit-master/query/valid")]
        public async Task<IActionResult> TestValidQuery()
        {
            var validQuery = new UnitMasterQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                CompanyId = 1,
                UnitName = "KG",
                SearchTerm = "unit",
                SortBy = "UnitName",
                SortDirection = "ASC"
            };

            try
            {
                var result = await _mediator.Send(new GetUnitMastersQuery { QueryParameters = validQuery });
                return Ok(new { message = "Query validation passed", result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, type = ex.GetType().Name });
            }
        }
    }
}
