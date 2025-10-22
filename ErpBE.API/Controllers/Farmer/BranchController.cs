using ErpBE.Application.BranchMasters.Commands;
using ErpBE.Application.BranchMasters.Queries;
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
    public class BranchController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BranchController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AuthorizeReadOnly] // Admin, ReadOnlyManager can read
        public async Task<IActionResult> GetAll([FromQuery] BranchQueryParameters? parameters = null)
        {
            if (parameters == null)
            {
                // Return simple list for backward compatibility
                var result = await _mediator.Send(new GetAllBranchesQuery());
                return Ok(result);
            }
            else
            {
                // Return paged and filtered results
                var result = await _mediator.Send(new GetBranchesWithFiltersQuery(parameters));
                return Ok(result);
            }
        }

        [HttpPost]
        [AuthorizeAdmin] // Only Admin can create branches
        [Audit("Branch", EntityIdProperty = "Id", Description = "Create new branch")]
        public async Task<IActionResult> Create([FromBody] CreateBranchCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAll), new { id }, id);
        }
    }
}
