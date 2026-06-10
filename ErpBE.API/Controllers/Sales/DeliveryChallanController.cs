using ErpBE.API.Common;
using ErpBE.Application.DeliveryChallan.Commands;
using ErpBE.Application.DeliveryChallan.Queries;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Sales;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeliveryChallanController : ControllerBase
{
    private readonly IMediator                         _mediator;
    private readonly ILogger<DeliveryChallanController> _logger;
    private readonly IDeliveryChallanPdfService         _pdfService;

    public DeliveryChallanController(
        IMediator mediator,
        ILogger<DeliveryChallanController> logger,
        IDeliveryChallanPdfService pdfService)
    {
        _mediator   = mediator;
        _logger     = logger;
        _pdfService = pdfService;
    }

    [HttpGet]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
    public async Task<IActionResult> GetAll([FromQuery] DeliveryChallanQueryParameters parameters)
    {
        var (data, totalCount) = await _mediator.Send(new GetAllDeliveryChallansQuery { Parameters = parameters });
        return Ok(new { data, totalCount, pageNumber = parameters.PageNumber, pageSize = parameters.PageSize });
    }

    [HttpGet("{id}")]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
    public async Task<IActionResult> GetById(int id, [FromQuery] int companyCode)
    {
        var result = await _mediator.Send(new GetDeliveryChallanByIdQuery { ChallanCode = id, CompanyCode = companyCode });
        if (result == null)
            return NotFound(new { message = $"Delivery Challan with code '{id}' not found." });

        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.Add)]
    public async Task<IActionResult> Create([FromBody] CreateDeliveryChallanCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.ChallanCode, companyCode = result.CompanyCode }, result);
    }

    [HttpPut("{id}")]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDeliveryChallanCommand command)
    {
        if (id != command.ChallanCode)
            return BadRequest(new { message = "Challan code in URL does not match the body." });

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.Delete)]
    public async Task<IActionResult> Delete(int id, [FromQuery] int companyCode)
    {
        var deleted = await _mediator.Send(new DeleteDeliveryChallanCommand { ChallanCode = id, CompanyCode = companyCode });
        if (!deleted)
            return NotFound(new { message = $"Delivery Challan with code '{id}' not found." });

        return NoContent();
    }

    [HttpPost("{id}/lock")]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.Edit)]
    public async Task<IActionResult> Lock(int id, [FromQuery] int companyCode)
    {
        var userCode = int.TryParse(User.FindFirst("user_code")?.Value, out var uc) ? uc : 0;
        var result = await _mediator.Send(new LockDeliveryChallanCommand { ChallanCode = id, CompanyCode = companyCode, LockedByUserId = userCode });
        if (!result)
            return Conflict(new { message = $"Delivery Challan {id} is already locked by another user." });

        return Ok(new { message = "Delivery Challan locked successfully." });
    }

    [HttpPost("{id}/unlock")]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.Edit)]
    public async Task<IActionResult> Unlock(int id, [FromQuery] int companyCode)
    {
        var result = await _mediator.Send(new UnlockDeliveryChallanCommand { ChallanCode = id, CompanyCode = companyCode });
        if (!result)
            return NotFound(new { message = $"Delivery Challan with code '{id}' not found or already unlocked." });

        return Ok(new { message = "Delivery Challan unlocked successfully." });
    }

    [HttpGet("{id}/print")]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.Print)]
    public async Task<IActionResult> Print(int id, [FromQuery] int companyCode)
    {
        var printData = await _mediator.Send(new GetDeliveryChallanPrintDataQuery { ChallanCode = id, CompanyCode = companyCode });

        if (printData == null)
            return NotFound(new { message = $"Delivery Challan with code '{id}' not found." });

        var pdfBytes = _pdfService.GenerateDeliveryChallanPdf(printData);
        return File(pdfBytes, "application/pdf", $"DeliveryChallan_{printData.ChallanNumber}.pdf");
    }
}
