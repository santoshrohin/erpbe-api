using ErpBE.Application.FarmerItemMaster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Farmer
{
    [ApiController]
    [Route("api/[controller]")]
    public class FarmerItemController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FarmerItemController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _mediator.Send(new GetAllFarmerItemsQuery()));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFarmerItemCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAll), new { id }, id);
        }
    }

}
