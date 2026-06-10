using ErpBE.Application.DTOs.LabourChargeInvoice;
using MediatR;

namespace ErpBE.Application.LabourChargeInvoice.Queries
{
    public class GetLabourChargeInvoiceByIdQuery : IRequest<LabourChargeInvoiceMasterDto?>
    {
        public int InvoiceCode { get; set; }
        public int CompanyCode { get; set; }
    }
}
