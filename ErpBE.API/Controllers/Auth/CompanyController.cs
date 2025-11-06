using ErpBE.Application.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CompanyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get distinct active companies
        /// </summary>
        /// <returns>List of distinct companies</returns>
        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                var query = new GetCompaniesQuery();
                var companies = await _mediator.Send(query);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }
    }
}

