namespace ErpBE.Application.DTOs.LabourChargeInvoice
{
    public class LabourChargeInvoiceDetailDto
    {
        public int InvoiceMasterCode { get; set; }
        public int? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int? UomCode { get; set; }
        public int? CustomerPoCode { get; set; }
        public decimal InvoiceQuantity { get; set; }
        public double? Rate { get; set; }
        public double? Amount { get; set; }
        public string? HsnCode { get; set; }
        public string? BatchNumber { get; set; }
        public double? CgstPercentage { get; set; }
        public double? SgstPercentage { get; set; }
        public double? IgstPercentage { get; set; }
        public string? NumberOfPackages { get; set; }
        public string? Remarks { get; set; }
        public string? SubHeading { get; set; }
        public double? ExciseAmount { get; set; }
        public double? AmortRate { get; set; }
        public double? AmortAmount { get; set; }
    }
}
