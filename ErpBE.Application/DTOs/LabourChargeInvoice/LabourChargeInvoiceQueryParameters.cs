namespace ErpBE.Application.DTOs.LabourChargeInvoice
{
    public class LabourChargeInvoiceQueryParameters
    {
        public int? CompanyCode { get; set; }
        public string? SearchText { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}
