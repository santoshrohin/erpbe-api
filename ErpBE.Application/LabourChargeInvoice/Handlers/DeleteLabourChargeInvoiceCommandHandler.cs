using ErpBE.Application.Interfaces;
using ErpBE.Application.LabourChargeInvoice.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.LabourChargeInvoice.Handlers
{
    public class DeleteLabourChargeInvoiceCommandHandler
        : IRequestHandler<DeleteLabourChargeInvoiceCommand, bool>
    {
        private readonly ILabourChargeInvoiceRepository _repository;
        private readonly ILogger<DeleteLabourChargeInvoiceCommandHandler> _logger;

        public DeleteLabourChargeInvoiceCommandHandler(
            ILabourChargeInvoiceRepository repository,
            ILogger<DeleteLabourChargeInvoiceCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> Handle(
            DeleteLabourChargeInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Deleting Labour Charge Invoice: {InvoiceCode} for Company: {CompanyCode}",
                request.InvoiceCode, request.CompanyCode);

            var existing = await _repository.GetByIdAsync(request.InvoiceCode, request.CompanyCode);
            if (existing?.IsModifyLocked == true)
                throw new InvalidOperationException($"Labour Charge Invoice {request.InvoiceCode} is locked and cannot be deleted.");

            return await _repository.DeleteAsync(request.InvoiceCode, request.CompanyCode);
        }
    }
}
