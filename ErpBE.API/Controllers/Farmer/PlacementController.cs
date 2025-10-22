using ErpBE.Application.Placement;
using ErpBE.Application.Placement.Queries;
using ErpBE.API.Common;
using ErpBE.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Farmer
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication
    public class PlacementController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlacementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [AuthorizeSales] // SalesManager and Admin can create placements
        public async Task<IActionResult> Create([FromBody] CreatePlacementCommand command)
        {
            var placementId = await _mediator.Send(command);
            return Ok(new { PlacementId = placementId });
        }

        [HttpGet("GetPlacementDetails")]
        [AuthorizeReadOnly] // Admin, ReadOnlyManager can view details
        public async Task<IActionResult> GetPlacementDetails(int? placementQty, DateTime? placementDate)
        {
            var query = new GetPlacementDetailsQuery(placementQty, placementDate);
            var placementDetails = await _mediator.Send(query);

            return Ok(placementDetails);
        }

        [HttpGet("GetAll")]
        [AuthorizeReadOnly] // Admin, ReadOnlyManager can view all
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllPlacementsQuery();
            var placements = await _mediator.Send(query);
            return Ok(placements);
        }

    }
}
