using ErpBE.API.Common;
using ErpBE.Application.DTOs.LabourChargeInvoice;
using ErpBE.Application.DTOs.TaxInvoice;
using ErpBE.Application.Interfaces;
using ErpBE.Application.LabourChargeInvoice.Commands;
using ErpBE.Application.LabourChargeInvoice.Queries;
using ErpBE.Application.TaxInvoice.Queries;
using ErpBE.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpBE.API.Controllers.Sales
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LabourChargeInvoiceController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LabourChargeInvoiceController> _logger;
        private readonly ILabourChargeInvoicePdfService _pdfService;

        public LabourChargeInvoiceController(
            IMediator mediator,
            ILogger<LabourChargeInvoiceController> logger,
            ILabourChargeInvoicePdfService pdfService)
        {
            _mediator = mediator;
            _logger = logger;
            _pdfService = pdfService;
        }

        /// <summary>
        /// Get all Labour Charge Invoices with filtering and pagination
        /// </summary>
        [HttpGet]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
        public async Task<ActionResult<LabourChargeInvoicePagedResponse>> GetAll(
            [FromQuery] LabourChargeInvoiceQueryParameters parameters)
        {
            _logger.LogInformation(
                "GET /api/LabourChargeInvoice - Company: {CompanyCode}, Page: {Page}",
                parameters.CompanyCode, parameters.PageNumber);

            var result = await _mediator.Send(new GetAllLabourChargeInvoicesQuery { Parameters = parameters });
            return Ok(result);
        }

        /// <summary>
        /// Get Labour Charge Invoice by ID
        /// </summary>
        [HttpGet("{id:int}")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
        public async Task<IActionResult> GetById(int id, [FromQuery] int companyCode)
        {
            _logger.LogInformation(
                "GET /api/LabourChargeInvoice/{Id} - Company: {CompanyCode}", id, companyCode);

            var result = await _mediator.Send(new GetLabourChargeInvoiceByIdQuery
            {
                InvoiceCode = id,
                CompanyCode = companyCode
            });

            if (result == null)
                return NotFound(new { message = $"Labour Charge Invoice with ID '{id}' not found." });

            return Ok(result);
        }

        /// <summary>
        /// Create a new Labour Charge Invoice
        /// </summary>
        [HttpPost]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.Add)]
        public async Task<IActionResult> Create([FromBody] CreateLabourChargeInvoiceCommand command)
        {
            _logger.LogInformation(
                "POST /api/LabourChargeInvoice - Company: {CompanyCode}, Customer: {CustomerCode}",
                command.CompanyCode, command.CustomerCode);

            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetById),
                new { id = result.InvoiceCode, companyCode = result.CompanyCode },
                result);
        }

        /// <summary>
        /// Update an existing Labour Charge Invoice
        /// </summary>
        [HttpPut("{id}")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.Edit)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLabourChargeInvoiceCommand command)
        {
            command.InvoiceCode = id;

            _logger.LogInformation(
                "PUT /api/LabourChargeInvoice/{Id} - Updating Labour Charge Invoice", id);

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Soft delete a Labour Charge Invoice
        /// </summary>
        [HttpDelete("{id}")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.Delete)]
        public async Task<IActionResult> Delete(int id, [FromQuery] int companyCode)
        {
            _logger.LogInformation(
                "DELETE /api/LabourChargeInvoice/{Id} - Company: {CompanyCode}", id, companyCode);

            var result = await _mediator.Send(new DeleteLabourChargeInvoiceCommand
            {
                InvoiceCode = id,
                CompanyCode = companyCode
            });

            if (!result)
                return NotFound(new { message = $"Labour Charge Invoice with ID '{id}' not found or could not be deleted." });

            return NoContent();
        }

        /// <summary>
        /// Lock a Labour Charge Invoice for modification
        /// </summary>
        [HttpPost("{id}/lock")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.Edit)]
        public async Task<IActionResult> Lock(int id, [FromQuery] int companyCode)
        {
            _logger.LogInformation(
                "POST /api/LabourChargeInvoice/{Id}/lock - Company: {CompanyCode}", id, companyCode);

            var userCode = int.TryParse(User.FindFirst("user_code")?.Value, out var uc) ? uc : 0;
            var result = await _mediator.Send(new LockLabourChargeInvoiceCommand
            {
                InvoiceCode    = id,
                CompanyCode    = companyCode,
                LockedByUserId = userCode
            });

            if (!result)
                return NotFound(new { message = $"Labour Charge Invoice with ID '{id}' not found." });

            return Ok(new { message = $"Labour Charge Invoice {id} locked successfully." });
        }

        /// <summary>
        /// Unlock a Labour Charge Invoice
        /// </summary>
        [HttpPost("{id}/unlock")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.Edit)]
        public async Task<IActionResult> Unlock(int id, [FromQuery] int companyCode)
        {
            _logger.LogInformation(
                "POST /api/LabourChargeInvoice/{Id}/unlock - Company: {CompanyCode}", id, companyCode);

            var result = await _mediator.Send(new UnlockLabourChargeInvoiceCommand
            {
                InvoiceCode = id,
                CompanyCode = companyCode
            });

            if (!result)
                return NotFound(new { message = $"Labour Charge Invoice with ID '{id}' not found." });

            return Ok(new { message = $"Labour Charge Invoice {id} unlocked successfully." });
        }

        /// <summary>
        /// Get items available for a customer (same SP as TaxInvoice)
        /// </summary>
        [HttpGet("items")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
        public async Task<IActionResult> GetItems([FromQuery] int customerCode, [FromQuery] int companyCode)
        {
            var result = await _mediator.Send(new GetTaxInvoiceItemsByCustomerQuery
            {
                CustomerCode = customerCode,
                CompanyCode = companyCode
            });
            return Ok(result);
        }

        /// <summary>
        /// Get item details (UOM, stock from job-work store, HSN) for entry panel
        /// </summary>
        [HttpGet("item-details/{itemCode}")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
        public async Task<IActionResult> GetItemDetails(int itemCode, [FromQuery] int companyCode)
        {
            var result = await _mediator.Send(new GetTaxInvoiceItemDetailsQuery
            {
                ItemCode = itemCode,
                CompanyCode = companyCode
            });
            if (result == null)
                return NotFound(new { message = $"Item {itemCode} not found." });
            return Ok(result);
        }

        /// <summary>
        /// Get PO options for a given item + customer (rate, pending qty)
        /// </summary>
        [HttpGet("pos")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.View)]
        public async Task<IActionResult> GetPos(
            [FromQuery] int itemCode,
            [FromQuery] int customerCode,
            [FromQuery] int companyCode,
            [FromQuery] int? invoiceCode)
        {
            var result = await _mediator.Send(new GetTaxInvoicePOsQuery
            {
                ItemCode = itemCode,
                CustomerCode = customerCode,
                CompanyCode = companyCode,
                InvoiceCode = invoiceCode
            });
            return Ok(result);
        }

        /// <summary>
        /// Print Labour Charge Invoice as PDF
        /// </summary>
        [HttpGet("{id:int}/print")]
        [RequirePermission(ModuleCodes.Sales, PermissionBit.Print)]
        public async Task<IActionResult> Print(int id, [FromQuery] int companyCode)
        {
            _logger.LogInformation(
                "GET /api/LabourChargeInvoice/{Id}/print - Company: {CompanyCode}", id, companyCode);

            var printData = await _mediator.Send(new GetLabourChargeInvoicePrintDataQuery
            {
                InvoiceCode = id,
                CompanyCode = companyCode
            });

            if (printData == null)
                return NotFound(new { message = $"Labour Charge Invoice with ID '{id}' not found." });

            var pdfBytes = _pdfService.GenerateLabourChargeInvoicePdf(printData);
            return File(pdfBytes, "application/pdf", $"LabourChargeInvoice_{printData.InvoiceNumber}.pdf");
        }
    }
}
