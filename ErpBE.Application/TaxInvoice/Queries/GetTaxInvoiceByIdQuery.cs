using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.TaxInvoice.Queries
{
    public class GetTaxInvoiceByIdQuery : IRequest<TaxInvoiceMasterDto?>
    {
        public int InvoiceCode { get; set; }
        public int CompanyCode { get; set; }
    }
}

