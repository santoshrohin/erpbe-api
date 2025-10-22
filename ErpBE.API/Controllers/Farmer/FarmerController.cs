using ErpBE.Application.FarmerMaster;
using ErpBE.Application.FarmerMaster.Queries;
using ErpBE.API.Common;
using ErpBE.Domain.Common;
using ErpBE.Domain.CommonDto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Farmer
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication
    public class FarmerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FarmerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AuthorizeReadOnly] // Admin, ReadOnlyManager can read
        public async Task<IActionResult> GetAll([FromQuery] FarmerQueryParameters? parameters = null)
        {
            if (parameters == null)
            {
                // Return simple list for backward compatibility
                var result = await _mediator.Send(new GetAllFarmersQuery());
                return Ok(result);
            }
            else
            {
                // Return paged and filtered results
                var result = await _mediator.Send(new GetFarmersWithFiltersQuery(parameters));
                return Ok(result);
            }
        }

        [HttpPost]
        [AuthorizeManagement] // Admin, SalesManager, StoreManager, PurchaseManager can create
        [Audit("Farmer", EntityIdProperty = "Id", Description = "Create new farmer")]
        public async Task<IActionResult> Create([FromBody] CreateFarmerCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAll), new { id }, id);
        }

        [HttpGet("{id}")]
        [AuthorizeReadOnly] // Admin, ReadOnlyManager can read
        public async Task<IActionResult> GetById(int id)
        {
            var farmer = await _mediator.Send(new GetFarmerByIdQuery(id));
            if (farmer == null)
                return NotFound();

            return Ok(farmer);
        }
    }
}
