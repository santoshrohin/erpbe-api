using ErpBE.Application.TaxInvoice.Commands;
using ErpBE.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.TaxInvoice.Handlers
{
    public class DeleteTaxInvoiceCommandHandler : IRequestHandler<DeleteTaxInvoiceCommand, bool>
    {
        private readonly ITaxInvoiceRepository _repository;
        private readonly ILogger<DeleteTaxInvoiceCommandHandler> _logger;

        public DeleteTaxInvoiceCommandHandler(
            ITaxInvoiceRepository repository,
            ILogger<DeleteTaxInvoiceCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteTaxInvoiceCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting Tax Invoice: {InvoiceCode} for Company: {CompanyCode}", 
                request.InvoiceCode, request.CompanyCode);

            try
            {
                // 1. Check if invoice exists
                var existingInvoice = await _repository.GetTaxInvoiceByIdAsync(request.InvoiceCode, request.CompanyCode);
                if (existingInvoice == null)
                {
                    // Return false if invoice not found instead of throwing exception
                    return false;
                }

                // 2. Check if invoice is locked
                var isLocked = await _repository.IsInvoiceLockedAsync(request.InvoiceCode);
                if (isLocked)
                {
                    throw new InvalidOperationException("This invoice is currently being modified by another user. Cannot delete.");
                }

                // 3. Soft delete (sets ES_DELETE = 1 and reverses stock)
                var result = await _repository.DeleteTaxInvoiceAsync(request.InvoiceCode, request.CompanyCode);

                if (result)
                {
                    _logger.LogInformation("Tax Invoice deleted successfully. Invoice Code: {InvoiceCode}", request.InvoiceCode);
                }
                else
                {
                    _logger.LogWarning("Failed to delete Tax Invoice. Invoice Code: {InvoiceCode}", request.InvoiceCode);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Tax Invoice: {InvoiceCode}", request.InvoiceCode);
                throw;
            }
        }
    }
}

