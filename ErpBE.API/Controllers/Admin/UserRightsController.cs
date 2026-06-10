using ErpBE.API.Common;
using ErpBE.Application.UserRights.Commands;
using ErpBE.Application.UserRights.Queries;
using ErpBE.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Admin;

[ApiController]
[Route("api/userrights")]
[RequirePermission(ModuleCodes.Admin, PermissionBit.View)]
public class UserRightsController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserRightsController(IMediator mediator) => _mediator = mediator;

    // GET api/userrights/screens
    [HttpGet("screens")]
    public async Task<IActionResult> GetScreens(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetScreenMastersQuery(), ct);
        return Ok(result);
    }

    // GET api/userrights/{userCode}
    [HttpGet("{userCode:int}")]
    public async Task<IActionResult> GetUserRights(int userCode, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserRightsQuery { UserCode = userCode }, ct);
        return Ok(result);
    }

    // PUT api/userrights/{userCode}
    [HttpPut("{userCode:int}")]
    [RequirePermission(ModuleCodes.Admin, PermissionBit.Edit)]
    public async Task<IActionResult> SaveUserRights(int userCode, [FromBody] List<ErpBE.Application.DTOs.UserRightRequest> rights, CancellationToken ct)
    {
        var result = await _mediator.Send(new SaveUserRightsCommand { UserCode = userCode, Rights = rights }, ct);
        return result ? Ok() : BadRequest();
    }

    // POST api/userrights/{userCode}/copy-from/{fromUserCode}
    [HttpPost("{userCode:int}/copy-from/{fromUserCode:int}")]
    [RequirePermission(ModuleCodes.Admin, PermissionBit.Edit)]
    public async Task<IActionResult> CopyUserRights(int userCode, int fromUserCode, CancellationToken ct)
    {
        var result = await _mediator.Send(new CopyUserRightsCommand { FromUserCode = fromUserCode, ToUserCode = userCode }, ct);
        return result ? Ok() : BadRequest();
    }

    // DELETE api/userrights/{userCode}
    [HttpDelete("{userCode:int}")]
    [RequirePermission(ModuleCodes.Admin, PermissionBit.Delete)]
    public async Task<IActionResult> DeleteUserRights(int userCode, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteUserRightsCommand { UserCode = userCode }, ct);
        return result ? Ok() : NotFound();
    }
}
