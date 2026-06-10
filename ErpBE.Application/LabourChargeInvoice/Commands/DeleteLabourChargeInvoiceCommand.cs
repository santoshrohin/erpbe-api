using MediatR;

namespace ErpBE.Application.LabourChargeInvoice.Commands
{
    public class DeleteLabourChargeInvoiceCommand : IRequest<bool>
    {
        public int InvoiceCode { get; set; }
        public int CompanyCode { get; set; }
    }
}
