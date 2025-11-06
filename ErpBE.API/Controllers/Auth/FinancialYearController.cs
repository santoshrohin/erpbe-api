using ErpBE.Application.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialYearController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FinancialYearController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get financial years for a specific company
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>List of financial years with formatted display text</returns>
        [HttpGet("{companyId}")]
        public async Task<IActionResult> GetFinancialYears(int companyId)
        {
            try
            {
                var query = new GetFinancialYearsQuery { CompanyId = companyId };
                var financialYears = await _mediator.Send(query);
                return Ok(financialYears);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }
    }
}

