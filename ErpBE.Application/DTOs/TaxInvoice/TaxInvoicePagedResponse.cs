namespace ErpBE.Application.DTOs.TaxInvoice
{
    /// <summary>
    /// Paged response for Tax Invoice list
    /// </summary>
    public class TaxInvoicePagedResponse
    {
        public List<TaxInvoiceMasterDto> Data { get; set; } = new List<TaxInvoiceMasterDto>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}

