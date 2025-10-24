using ErpBE.Application.DTOs.TaxInvoice;
using ErpBE.Application.TaxInvoice.Queries;
using ErpBE.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.TaxInvoice.Handlers
{
    public class GetAllTaxInvoicesQueryHandler : IRequestHandler<GetAllTaxInvoicesQuery, TaxInvoicePagedResponse>
    {
        private readonly ITaxInvoiceRepository _repository;
        private readonly ILogger<GetAllTaxInvoicesQueryHandler> _logger;

        public GetAllTaxInvoicesQueryHandler(
            ITaxInvoiceRepository repository,
            ILogger<GetAllTaxInvoicesQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<TaxInvoicePagedResponse> Handle(GetAllTaxInvoicesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching Tax Invoices - Company: {CompanyId}, Page: {PageNumber}, PageSize: {PageSize}", 
                request.CompanyId, request.PageNumber, request.PageSize);

            try
            {
                var result = await _repository.GetAllTaxInvoicesAsync(request);

                _logger.LogInformation("Tax Invoices fetched successfully. Total: {TotalCount}, Page: {PageNumber}/{TotalPages}", 
                    result.TotalCount, result.PageNumber, result.TotalPages);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Tax Invoices for Company: {CompanyId}", request.CompanyId);
                throw;
            }
        }
    }
}

