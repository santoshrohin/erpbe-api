namespace ErpBE.Application.DTOs.TaxInvoice
{
    /// <summary>
    /// Query parameters for filtering and searching Tax Invoices
    /// </summary>
    public class TaxInvoiceQueryParameters
    {
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        // Required Filters
        public int CompanyId { get; set; }
        
        // Optional Filters
        public int? CustomerId { get; set; }
        public int? CustomerPoCode { get; set; }
        public DateTime? InvoiceDateFrom { get; set; }
        public DateTime? InvoiceDateTo { get; set; }
        public string? InvoiceNumber { get; set; }
        public byte? InvoiceType { get; set; } // 0 for Tax Invoice
        public bool? IsDeleted { get; set; }
        public bool? IsSupplementary { get; set; }
        public bool? ExportFlag { get; set; }
        
        // Search
        public string? SearchTerm { get; set; } // Search by Invoice No, Customer Name, PO Number
        
        // Sorting
        public string SortBy { get; set; } = "InvoiceDate"; // Default sort by date
        public string SortOrder { get; set; } = "desc"; // desc or asc
    }
}

