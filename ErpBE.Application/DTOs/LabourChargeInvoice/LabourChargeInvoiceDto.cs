namespace ErpBE.Application.DTOs.LabourChargeInvoice
{
    public class LabourChargeInvoiceMasterDto
    {
        public int InvoiceCode { get; set; }
        public decimal InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int CustomerCode { get; set; }
        public string? CustomerName { get; set; }
        public int? CustomerPoCode { get; set; }
        public byte? InvoiceType { get; set; }
        public double? NetAmount { get; set; }
        public double? DiscountPercentage { get; set; }
        public double? DiscountAmount { get; set; }
        public double? ServiceTaxPercentage { get; set; }
        public double? ServiceTaxAmount { get; set; }
        public double? TcsPercentage { get; set; }
        public double? TcsAmount { get; set; }
        public double? PackingAmount { get; set; }
        public double? GrossAmount { get; set; }
        public int? TaxCode { get; set; }
        public string? VehicleNumber { get; set; }
        public string? TransportName { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? RemovalDate { get; set; }
        public string? Remarks { get; set; }
        public string? LrNumber { get; set; }
        public DateTime? LrDate { get; set; }
        public double? TaxableAmount { get; set; }
        public double? RoundingAmount { get; set; }
        public double? OtherAmount { get; set; }
        public double? FreightCharges { get; set; }
        public double? InsuranceAmount { get; set; }
        public double? TransportAmount { get; set; }
        public double? OctriAmount { get; set; }
        public int? CreditDays { get; set; }
        public string? HsnCode { get; set; }
        public int CompanyCode { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsModifyLocked { get; set; }
        public List<LabourChargeInvoiceDetailDto> Details { get; set; } = new();
    }
}
