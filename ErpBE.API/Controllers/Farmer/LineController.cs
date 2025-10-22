using ErpBE.Application.LineMasters.Commands;
using ErpBE.Application.LineMasters.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Farmer
{
    [ApiController]
    [Route("api/[controller]")]
    public class LineController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LineController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllLinesQuery());
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLineCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAll), new { id }, id);
        }
    }
}
