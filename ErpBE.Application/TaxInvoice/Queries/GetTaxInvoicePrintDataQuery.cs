using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.TaxInvoice.Queries;

/// <summary>
/// Query to get comprehensive Tax Invoice data for printing
/// </summary>
public class GetTaxInvoicePrintDataQuery : IRequest<TaxInvoicePrintDto?>
{
    public long InvoiceCode { get; set; }
    public int CompanyId { get; set; }
    public InvoiceCopyType CopyType { get; set; } = InvoiceCopyType.Original;
}

