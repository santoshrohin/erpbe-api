using ErpBE.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialYearController : ControllerBase
    {
        private readonly IFinancialYearRepository _financialYearRepository;

        public FinancialYearController(IFinancialYearRepository financialYearRepository)
        {
            _financialYearRepository = financialYearRepository;
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
                var financialYears = await _financialYearRepository.GetFinancialYearsByCompanyIdAsync(companyId);
                return Ok(financialYears);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }
    }
}

