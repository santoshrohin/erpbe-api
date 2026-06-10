using MediatR;

namespace ErpBE.Application.LabourChargeInvoice.Commands
{
    public class LockLabourChargeInvoiceCommand : IRequest<bool>
    {
        public int InvoiceCode    { get; set; }
        public int CompanyCode    { get; set; }
        public int LockedByUserId { get; set; }
    }
}
