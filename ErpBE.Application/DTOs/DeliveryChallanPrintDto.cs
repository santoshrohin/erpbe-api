namespace ErpBE.Application.DTOs;

public class DeliveryChallanPrintDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string? CompanyGstin { get; set; }

    public int ChallanCode { get; set; }
    public int ChallanNumber { get; set; }
    public DateTime ChallanDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public string? Through { get; set; }
    public string? VehicleNumber { get; set; }
    public string? LrNumber { get; set; }
    public string? OrderNumber { get; set; }
    public DateTime? OrderDate { get; set; }
    public bool IsReturnable { get; set; }

    public List<DcDetailPrintInfo> LineItems { get; set; } = new();
}

public class DcDetailPrintInfo
{
    public int SrNo { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Uom { get; set; } = string.Empty;
    public decimal OrderedQuantity { get; set; }
    public string? BatchNumber { get; set; }
    public string? NumberOfPacks { get; set; }
    public string? Remark { get; set; }
}
