using ErpBE.API.Common;
using ErpBE.Application.DTOs;
using ErpBE.Application.ProductionToStore.Commands;
using ErpBE.Application.ProductionToStore.Queries;
using ErpBE.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Sales;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductionToStoreController : ControllerBase
{
    private readonly IMediator                          _mediator;
    private readonly ILogger<ProductionToStoreController> _logger;

    public ProductionToStoreController(IMediator mediator, ILogger<ProductionToStoreController> logger)
    {
        _mediator = mediator;
        _logger   = logger;
    }

    [HttpGet]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
    public async Task<IActionResult> GetAll([FromQuery] ProductionToStoreQueryParameters parameters)
    {
        var (data, totalCount) = await _mediator.Send(new GetAllProductionToStoreQuery { Parameters = parameters });
        return Ok(new { data, totalCount, pageNumber = parameters.PageNumber, pageSize = parameters.PageSize });
    }

    [HttpGet("{id}")]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
    public async Task<IActionResult> GetById(int id, [FromQuery] int companyCode)
    {
        var result = await _mediator.Send(new GetProductionToStoreByIdQuery { ProductionCode = id, CompanyCode = companyCode });
        if (result == null)
            return NotFound(new { message = $"Production To Store with code '{id}' not found." });

        return Ok(result);
    }

    [HttpPost]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.Add)]
    public async Task<IActionResult> Create([FromBody] CreateProductionToStoreCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.ProductionCode, companyCode = result.CompanyCode }, result);
    }

    [HttpPut("{id}")]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.Edit)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductionToStoreCommand command)
    {
        if (id != command.ProductionCode)
            return BadRequest(new { message = "Production code in URL does not match the body." });

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [RequirePermission(ModuleCodes.Sales, PermissionBit.Delete)]
    public async Task<IActionResult> Delete(int id, [FromQuery] int companyCode)
    {
        var deleted = await _mediator.Send(new DeleteProductionToStoreCommand { ProductionCode = id, CompanyCode = companyCode });
        if (!deleted)
            return NotFound(new { message = $"Production To Store with code '{id}' not found." });

        return NoContent();
    }
}
