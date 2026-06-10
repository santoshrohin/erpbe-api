using ErpBE.Application.Interfaces;
using ErpBE.Application.LabourChargeInvoice.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.LabourChargeInvoice.Handlers
{
    public class UnlockLabourChargeInvoiceCommandHandler
        : IRequestHandler<UnlockLabourChargeInvoiceCommand, bool>
    {
        private readonly ILabourChargeInvoiceRepository _repository;
        private readonly ILogger<UnlockLabourChargeInvoiceCommandHandler> _logger;

        public UnlockLabourChargeInvoiceCommandHandler(
            ILabourChargeInvoiceRepository repository,
            ILogger<UnlockLabourChargeInvoiceCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<bool> Handle(
            UnlockLabourChargeInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Unlocking Labour Charge Invoice: {InvoiceCode} for Company: {CompanyCode}",
                request.InvoiceCode, request.CompanyCode);

            return await _repository.UnlockAsync(request.InvoiceCode, request.CompanyCode);
        }
    }
}
