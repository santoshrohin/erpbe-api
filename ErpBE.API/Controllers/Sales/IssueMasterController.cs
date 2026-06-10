using ErpBE.API.Common;
using ErpBE.Application.DTOs;
using ErpBE.Application.IssueMaster.Commands;
using ErpBE.Application.IssueMaster.Queries;
using ErpBE.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Sales;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IssueMasterController : ControllerBase
{
    private readonly IMediator                    _mediator;
    private readonly ILogger<IssueMasterController> _logger;

    public IssueMasterController(IMediator mediator, ILogger<IssueMasterController> logger)
    {
        _mediator = mediator;
        _logger   = logger;
    }

    [HttpGet]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
    public async Task<IActionResult> GetAll([FromQuery] IssueMasterQueryParameters parameters)
    {
        var (data, totalCount) = await _mediator.Send(new GetAllIssueMasterQuery { Parameters = parameters });
        return Ok(new { data, totalCount, pageNumber = parameters.PageNumber, pageSize = parameters.PageSize });
    }

    [HttpGet("{id}")]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
    public async Task<IActionResult> GetById(int id, [FromQuery] int companyCode)
    {
        var result = await _mediator.Send(new GetIssueMasterByIdQuery { IssueCode = id, CompanyCode = companyCode });
        if (result == null)
            return NotFound(new { message = $"Issue Master with code '{id}' not found." });

        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.Add)]
    public async Task<IActionResult> Create([FromBody] CreateIssueMasterCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.IssueCode, companyCode = result.CompanyCode }, result);
    }

    [HttpPut("{id}")]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateIssueMasterCommand command)
    {
        if (id != command.IssueCode)
            return BadRequest(new { message = "Issue code in URL does not match the body." });

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.Delete)]
    public async Task<IActionResult> Delete(int id, [FromQuery] int companyCode)
    {
        var deleted = await _mediator.Send(new DeleteIssueMasterCommand { IssueCode = id, CompanyCode = companyCode });
        if (!deleted)
            return NotFound(new { message = $"Issue Master with code '{id}' not found." });

        return NoContent();
    }
}
