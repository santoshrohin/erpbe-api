using ErpBE.Application.Common;
using ErpBE.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Common
{
    [ApiController]
    [Route("api/[controller]")]
    public class DropdownController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DropdownController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Returns dropdown data based on provided configuration.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GetDropdown([FromBody] DropdownRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Table) ||
                string.IsNullOrWhiteSpace(request.IdColumn) ||
                string.IsNullOrWhiteSpace(request.DisplayColumn))
            {
                return BadRequest("Table, IdColumn, and DisplayColumn are required fields.");
            }

            try
            {
                var query = new GetDropdownQuery(request);
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        [HttpPost("batch")]
        public async Task<IActionResult> GetBatchDropdowns([FromBody] BatchDropdownRequest request)
        {
            // Validate all required properties for each request
            for (int i = 0; i < request.Requests.Count; i++)
            {
                var r = request.Requests[i];
                var req = r.Request;
                if (string.IsNullOrWhiteSpace(req.Table) ||
                    string.IsNullOrWhiteSpace(req.IdColumn) ||
                    string.IsNullOrWhiteSpace(req.DisplayColumn))
                {
                    return BadRequest($"Dropdown batch[{i}] (key: {r.Key}) is missing required fields: Table, IdColumn, DisplayColumn");
                }
            }
            try
            {
                var query = new GetBatchDropdownsQuery(request);
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", detail = ex.ToString() });
            }
        }
    }
}
