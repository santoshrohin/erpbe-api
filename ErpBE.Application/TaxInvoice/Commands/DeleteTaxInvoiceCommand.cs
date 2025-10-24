using MediatR;

namespace ErpBE.Application.TaxInvoice.Commands
{
    public class DeleteTaxInvoiceCommand : IRequest<bool>
    {
        public int InvoiceCode { get; set; }
        public int CompanyCode { get; set; }
    }
}

