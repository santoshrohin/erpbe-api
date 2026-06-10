namespace ErpBE.Application.DTOs.TaxInvoice
{
    public class TaxInvoiceCustomerDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public int? StateCode { get; set; }
        public string? StateName { get; set; }
        public string? GstNumber { get; set; }
        public bool? GstApplicable { get; set; }
    }

    public class TaxInvoiceItemDto
    {
        public int ItemCode { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCodeDisplay { get; set; }
        public int? UomCode { get; set; }
        public string? UomName { get; set; }
    }

    public class TaxInvoiceItemDetailsDto
    {
        public int ItemCode { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? ItemCodeDisplay { get; set; }
        public int? UomCode { get; set; }
        public string? UomName { get; set; }
        public double StockQuantity { get; set; }
        public string? HsnCode { get; set; }
        public double CgstPercentage { get; set; }
        public double SgstPercentage { get; set; }
        public double IgstPercentage { get; set; }
    }

    public class TaxInvoicePoDto
    {
        public int PoCode { get; set; }
        public string? PoNumber { get; set; }
        public DateTime? PoDate { get; set; }
        public double PendingQuantity { get; set; }
        public double OrderedQuantity { get; set; }
        public double DispatchedQuantity { get; set; }
        public double Rate { get; set; }
        public double DiscountAmount { get; set; }
        public double NetRate { get; set; }
        public double AmortRate { get; set; }
        public int? TaxCode { get; set; }
        public int? UomCode { get; set; }
    }

    public class CompanyStateDto
    {
        public int CompanyCode { get; set; }
        public int? StateCode { get; set; }
        public string? StateName { get; set; }
    }

    public class SalesTaxDto
    {
        public int TaxCode { get; set; }
        public string TaxName { get; set; } = string.Empty;
        public double TaxRate { get; set; }
    }
}
