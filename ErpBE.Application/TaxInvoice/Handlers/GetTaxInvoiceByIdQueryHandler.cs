using ErpBE.Application.DTOs;
using ErpBE.Application.TaxInvoice.Queries;
using ErpBE.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.TaxInvoice.Handlers
{
    public class GetTaxInvoiceByIdQueryHandler : IRequestHandler<GetTaxInvoiceByIdQuery, TaxInvoiceMasterDto?>
    {
        private readonly ITaxInvoiceRepository _repository;
        private readonly ILogger<GetTaxInvoiceByIdQueryHandler> _logger;

        public GetTaxInvoiceByIdQueryHandler(
            ITaxInvoiceRepository repository,
            ILogger<GetTaxInvoiceByIdQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<TaxInvoiceMasterDto?> Handle(GetTaxInvoiceByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching Tax Invoice by ID: {InvoiceCode}, Company: {CompanyCode}", 
                request.InvoiceCode, request.CompanyCode);

            try
            {
                var invoice = await _repository.GetTaxInvoiceByIdAsync(request.InvoiceCode, request.CompanyCode);

                if (invoice == null)
                {
                    _logger.LogWarning("Tax Invoice not found. Invoice Code: {InvoiceCode}", request.InvoiceCode);
                }
                else
                {
                    _logger.LogInformation("Tax Invoice fetched successfully. Invoice Code: {InvoiceCode}, Invoice Number: {InvoiceNumber}", 
                        invoice.InvoiceCode, invoice.InvoiceNumber);
                }

                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Tax Invoice by ID: {InvoiceCode}", request.InvoiceCode);
                throw;
            }
        }
    }
}

