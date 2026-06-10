namespace ErpBE.Application.DTOs;

public class LabourChargeInvoicePrintDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyGstin { get; set; } = string.Empty;

    public int InvoiceCode { get; set; }
    public int InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public string InvoiceType { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string? CustomerGstin { get; set; }
    public string? VehicleNumber { get; set; }
    public string? TransportName { get; set; }
    public string? LrNumber { get; set; }
    public string? Remarks { get; set; }

    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PackingAmount { get; set; }
    public decimal FreightCharges { get; set; }
    public decimal OtherAmount { get; set; }
    public decimal TcsPercentage { get; set; }
    public decimal TcsAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal GrossAmount { get; set; }

    public List<LciDetailPrintInfo> LineItems { get; set; } = new();
}

public class LciDetailPrintInfo
{
    public int SrNo { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? HsnCode { get; set; }
    public string Uom { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public decimal CgstPercentage { get; set; }
    public decimal SgstPercentage { get; set; }
    public decimal IgstPercentage { get; set; }
}
