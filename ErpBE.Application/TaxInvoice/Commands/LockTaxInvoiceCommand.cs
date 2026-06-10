using MediatR;

namespace ErpBE.Application.TaxInvoice.Commands
{
    public class LockTaxInvoiceCommand : IRequest<bool>
    {
        public long InvoiceCode    { get; set; }
        public int  LockedByUserId { get; set; }
    }

    public class UnlockTaxInvoiceCommand : IRequest<bool>
    {
        public long InvoiceCode { get; set; }
    }
}
