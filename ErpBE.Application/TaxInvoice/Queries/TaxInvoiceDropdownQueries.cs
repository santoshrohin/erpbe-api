using ErpBE.Application.DTOs.TaxInvoice;
using MediatR;

namespace ErpBE.Application.TaxInvoice.Queries
{
    public class GetTaxInvoiceCustomersQuery : IRequest<List<TaxInvoiceCustomerDto>>
    {
        public int CompanyCode { get; set; }
    }

    public class GetTaxInvoiceItemsByCustomerQuery : IRequest<List<TaxInvoiceItemDto>>
    {
        public int CustomerCode { get; set; }
        public int CompanyCode { get; set; }
    }

    public class GetTaxInvoiceItemDetailsQuery : IRequest<TaxInvoiceItemDetailsDto?>
    {
        public int ItemCode { get; set; }
        public int CompanyCode { get; set; }
    }

    public class GetTaxInvoicePOsQuery : IRequest<List<TaxInvoicePoDto>>
    {
        public int ItemCode { get; set; }
        public int CustomerCode { get; set; }
        public int CompanyCode { get; set; }
        public int? InvoiceCode { get; set; }
    }

    public class GetCompanyStateQuery : IRequest<CompanyStateDto?>
    {
        public int CompanyCode { get; set; }
    }

    public class GetSalesTaxMasterQuery : IRequest<List<SalesTaxDto>>
    {
        public int CompanyCode { get; set; }
    }
}
