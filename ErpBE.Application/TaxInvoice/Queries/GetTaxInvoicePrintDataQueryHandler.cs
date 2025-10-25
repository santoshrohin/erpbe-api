using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.TaxInvoice.Queries;

/// <summary>
/// Handler for getting Tax Invoice print data
/// </summary>
public class GetTaxInvoicePrintDataQueryHandler : IRequestHandler<GetTaxInvoicePrintDataQuery, TaxInvoicePrintDto?>
{
    private readonly ITaxInvoiceRepository _repository;

    public GetTaxInvoicePrintDataQueryHandler(ITaxInvoiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<TaxInvoicePrintDto?> Handle(GetTaxInvoicePrintDataQuery request, CancellationToken cancellationToken)
    {
        var printData = await _repository.GetPrintDataAsync(request.InvoiceCode, request.CompanyId);
        
        if (printData != null)
        {
            printData.CopyType = request.CopyType;
        }
        
        return printData;
    }
}

