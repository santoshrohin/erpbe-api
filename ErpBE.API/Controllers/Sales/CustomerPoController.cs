using ErpBE.Application.CustomerPo.Commands;
using ErpBE.Application.CustomerPo.Queries;
using ErpBE.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Sales;

/// <summary>
/// Customer PO (Purchase Order) management endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerPoController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CustomerPoController> _logger;

    public CustomerPoController(IMediator mediator, ILogger<CustomerPoController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all Customer POs with pagination, filtering, and sorting
    /// </summary>
    /// <param name="parameters">Query parameters</param>
    /// <returns>Paged list of Customer POs</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllCustomerPos([FromQuery] CustomerPoQueryParameters parameters)
    {
        _logger.LogInformation("Fetching Customer POs - CompanyId: {CompanyId}, Page: {PageNumber}, PageSize: {PageSize}",
            parameters.CompanyId, parameters.PageNumber, parameters.PageSize);

        var query = new GetAllCustomerPosQuery { Parameters = parameters };
        var result = await _mediator.Send(query);

        return Ok(new
        {
            data = result.Data,
            totalCount = result.TotalCount,
            pageNumber = parameters.PageNumber,
            pageSize = parameters.PageSize
        });
    }

    /// <summary>
    /// Get a Customer PO by ID with all details
    /// </summary>
    /// <param name="id">PO Code</param>
    /// <param name="companyId">Company ID</param>
    /// <returns>Customer PO with details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerPoById(int id, [FromQuery] int companyId)
    {
        _logger.LogInformation("Fetching Customer PO by ID: {PoCode}, CompanyId: {CompanyId}", id, companyId);

        var query = new GetCustomerPoByIdQuery { PoCode = id, CompanyId = companyId };
        var result = await _mediator.Send(query);

        if (result == null)
        {
            _logger.LogWarning("Customer PO not found - PoCode: {PoCode}, CompanyId: {CompanyId}", id, companyId);
            return NotFound(new { message = $"Customer PO with ID '{id}' not found." });
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new Customer PO with details
    /// </summary>
    /// <param name="command">Create command</param>
    /// <returns>Created Customer PO</returns>
    [HttpPost]
    public async Task<IActionResult> CreateCustomerPo([FromBody] CreateCustomerPoCommand command)
    {
        _logger.LogInformation("Creating new Customer PO - PoNumber: {PoNumber}, CompanyId: {CompanyId}",
            command.PoNumber, command.CompanyId);

        var result = await _mediator.Send(command);

        _logger.LogInformation("Customer PO created successfully - PoCode: {PoCode}", result.PoCode);

        return CreatedAtAction(
            nameof(GetCustomerPoById),
            new { id = result.PoCode, companyId = result.CompanyId },
            result);
    }

    /// <summary>
    /// Update an existing Customer PO
    /// </summary>
    /// <param name="id">PO Code</param>
    /// <param name="command">Update command</param>
    /// <returns>Updated Customer PO</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomerPo(int id, [FromBody] UpdateCustomerPoCommand command)
    {
        if (id != command.PoCode)
        {
            _logger.LogWarning("PO Code mismatch - URL: {UrlId}, Body: {BodyId}", id, command.PoCode);
            return BadRequest(new { message = "PO Code in URL does not match the body." });
        }

        _logger.LogInformation("Updating Customer PO - PoCode: {PoCode}, CompanyId: {CompanyId}",
            command.PoCode, command.CompanyId);

        var result = await _mediator.Send(command);

        _logger.LogInformation("Customer PO updated successfully - PoCode: {PoCode}", result.PoCode);

        return Ok(result);
    }

    /// <summary>
    /// Delete (soft delete) a Customer PO
    /// </summary>
    /// <param name="id">PO Code</param>
    /// <param name="companyId">Company ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomerPo(int id, [FromQuery] int companyId)
    {
        // Validate parameters directly (not through command validation)
        if (id == 0)
            return BadRequest(new { message = "PO Code is required." });
        
        if (companyId <= 0)
            return BadRequest(new { message = "Company ID must be greater than 0." });

        _logger.LogInformation("Deleting Customer PO - PoCode: {PoCode}, CompanyId: {CompanyId}", id, companyId);

        var command = new DeleteCustomerPoCommand { PoCode = id, CompanyId = companyId };
        var result = await _mediator.Send(command);

        if (!result)
        {
            _logger.LogWarning("Customer PO not found for deletion - PoCode: {PoCode}, CompanyId: {CompanyId}",
                id, companyId);
            return NotFound(new { message = $"Customer PO with ID '{id}' not found." });
        }

        _logger.LogInformation("Customer PO deleted successfully - PoCode: {PoCode}", id);

        return Ok(new { message = "Customer PO deleted successfully.", poCode = id });
    }
}

