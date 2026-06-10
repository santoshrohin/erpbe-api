using MediatR;

namespace ErpBE.Application.TaxInvoice.Commands
{
    public class DeleteTaxInvoiceCommand : IRequest<bool>
    {
        public long InvoiceCode { get; set; }
        public int CompanyCode { get; set; }
    }
}

