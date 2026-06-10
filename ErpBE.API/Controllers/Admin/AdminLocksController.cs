using ErpBE.API.Common;
using ErpBE.Application.Admin.Commands;
using ErpBE.Application.Admin.Queries;
using ErpBE.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Admin;

[ApiController]
[Route("api/admin/locks")]
[Authorize]
public class AdminLocksController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminLocksController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Returns all records currently held under a lock across every module.
    /// Requires Admin > View permission.
    /// </summary>
    [HttpGet]
    [RequirePermission(ModuleCodes.Admin, PermissionBit.View)]
    public async Task<IActionResult> GetActiveLocks()
    {
        var locks = await _mediator.Send(new GetActiveLocksQuery());
        return Ok(locks);
    }

    /// <summary>
    /// Force-unlocks a specific record. Use when a user's session died and left the record locked.
    /// Requires Admin > Edit permission.
    /// </summary>
    [HttpDelete("{module}/{id:int}")]
    [RequirePermission(ModuleCodes.Admin, PermissionBit.Edit)]
    public async Task<IActionResult> ForceUnlock(string module, int id)
    {
        var userCode = int.TryParse(User.FindFirst("user_code")?.Value, out var uc) ? uc : 0;
        var unlocked = await _mediator.Send(new ForceUnlockCommand(module, id, userCode));
        if (!unlocked)
            return NotFound(new { message = $"No active lock found for {module}/{id}." });

        return NoContent();
    }
}
