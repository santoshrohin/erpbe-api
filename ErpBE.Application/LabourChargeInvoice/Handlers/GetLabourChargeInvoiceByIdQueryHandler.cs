using ErpBE.Application.DTOs.LabourChargeInvoice;
using ErpBE.Application.Interfaces;
using ErpBE.Application.LabourChargeInvoice.Queries;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.LabourChargeInvoice.Handlers
{
    public class GetLabourChargeInvoiceByIdQueryHandler
        : IRequestHandler<GetLabourChargeInvoiceByIdQuery, LabourChargeInvoiceMasterDto?>
    {
        private readonly ILabourChargeInvoiceRepository _repository;
        private readonly ILogger<GetLabourChargeInvoiceByIdQueryHandler> _logger;

        public GetLabourChargeInvoiceByIdQueryHandler(
            ILabourChargeInvoiceRepository repository,
            ILogger<GetLabourChargeInvoiceByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<LabourChargeInvoiceMasterDto?> Handle(
            GetLabourChargeInvoiceByIdQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Fetching Labour Charge Invoice: {InvoiceCode} for Company: {CompanyCode}",
                request.InvoiceCode, request.CompanyCode);

            return await _repository.GetByIdAsync(request.InvoiceCode, request.CompanyCode);
        }
    }
}
