using ErpBE.Application.DTOs.LabourChargeInvoice;
using ErpBE.Application.Interfaces;
using ErpBE.Application.LabourChargeInvoice.Queries;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.LabourChargeInvoice.Handlers
{
    public class GetAllLabourChargeInvoicesQueryHandler
        : IRequestHandler<GetAllLabourChargeInvoicesQuery, LabourChargeInvoicePagedResponse>
    {
        private readonly ILabourChargeInvoiceRepository _repository;
        private readonly ILogger<GetAllLabourChargeInvoicesQueryHandler> _logger;

        public GetAllLabourChargeInvoicesQueryHandler(
            ILabourChargeInvoiceRepository repository,
            ILogger<GetAllLabourChargeInvoicesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<LabourChargeInvoicePagedResponse> Handle(
            GetAllLabourChargeInvoicesQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Fetching Labour Charge Invoices - Company: {CompanyCode}, Page: {PageNumber}, PageSize: {PageSize}",
                request.Parameters.CompanyCode, request.Parameters.PageNumber, request.Parameters.PageSize);

            return await _repository.GetAllAsync(request.Parameters);
        }
    }
}
