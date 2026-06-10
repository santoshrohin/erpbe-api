using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.LabourChargeInvoice.Queries;

public class GetLabourChargeInvoicePrintDataQuery : IRequest<LabourChargeInvoicePrintDto?>
{
    public int InvoiceCode { get; set; }
    public int CompanyCode { get; set; }
}
