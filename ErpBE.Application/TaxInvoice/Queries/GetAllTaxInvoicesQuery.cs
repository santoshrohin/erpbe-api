using ErpBE.Application.DTOs.TaxInvoice;
using MediatR;

namespace ErpBE.Application.TaxInvoice.Queries
{
    public class GetAllTaxInvoicesQuery : IRequest<TaxInvoicePagedResponse>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int CompanyId { get; set; }
        public int? CustomerId { get; set; }
        public int? CustomerPoCode { get; set; }
        public DateTime? InvoiceDateFrom { get; set; }
        public DateTime? InvoiceDateTo { get; set; }
        public string? InvoiceNumber { get; set; }
        public byte? InvoiceType { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsSupplementary { get; set; }
        public bool? ExportFlag { get; set; }
        public string? SearchTerm { get; set; }
        public string SortBy { get; set; } = "InvoiceDate";
        public string SortOrder { get; set; } = "desc";
    }
}

