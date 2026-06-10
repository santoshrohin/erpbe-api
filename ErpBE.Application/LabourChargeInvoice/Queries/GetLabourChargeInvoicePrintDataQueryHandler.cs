using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.LabourChargeInvoice.Queries;

public class GetLabourChargeInvoicePrintDataQueryHandler
    : IRequestHandler<GetLabourChargeInvoicePrintDataQuery, LabourChargeInvoicePrintDto?>
{
    private readonly ILabourChargeInvoiceRepository _repository;

    public GetLabourChargeInvoicePrintDataQueryHandler(ILabourChargeInvoiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<LabourChargeInvoicePrintDto?> Handle(
        GetLabourChargeInvoicePrintDataQuery request,
        CancellationToken cancellationToken)
    {
        return await _repository.GetPrintDataAsync(request.InvoiceCode, request.CompanyCode);
    }
}
