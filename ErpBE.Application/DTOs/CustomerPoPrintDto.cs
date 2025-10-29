namespace ErpBE.Application.DTOs;

/// <summary>
/// Main DTO containing all data required for printing a Customer PO (Sales Order)
/// Matches EXACT format from D:\Invoice\CustomerPO_page-0001.jpg
/// </summary>
public class CustomerPoPrintDto
{
    // Company Information
    public CompanyPrintInfo Company { get; set; } = new();
    
    // PO Header Data
    public PoHeaderPrintInfo PoHeader { get; set; } = new();
    
    // Customer Details
    public CustomerPrintInfo Customer { get; set; } = new();
    
    // PO Line Items
    public List<PoDetailPrintInfo> LineItems { get; set; } = new();
    
    // Totals and Summary
    public PoTotalsPrintInfo Totals { get; set; } = new();
    
    // Copy Type
    public PoCopyType CopyType { get; set; }
    
    // Additional Fields
    public string? Consignee { get; set; }
    public string? TransportThrough { get; set; }
    public string? DeliveryTerms { get; set; }
    public string? Narrations { get; set; }
    public string? FormNumber { get; set; } = "FORM NO : MK/F/05,REV.0";
}

/// <summary>
/// PO header information
/// </summary>
public class PoHeaderPrintInfo
{
    public int SaleOrderNo { get; set; }                      // CPOM_CODE or CPOM_SONO
    public DateTime SaleOrderDate { get; set; }               // CPOM_DATE
    public string? PoNo { get; set; }                         // CPOM_PONO
    public DateTime? PoDate { get; set; }                     // CPOM_PODATE
}

/// <summary>
/// Customer information for "Invoice To" section
/// </summary>
public class CustomerPrintInfo
{
    public string Name { get; set; } = string.Empty;          // P_NAME
    public string Address { get; set; } = string.Empty;       // P_ADD1, P_ADD2, P_CITY, P_PIN_CODE
}

/// <summary>
/// PO line item detail
/// Maps to table: Sr. No | Item Name | Qty | Unit | Rate | Amount
/// </summary>
public class PoDetailPrintInfo
{
    public int SrNo { get; set; }                             // Row number
    public string ItemName { get; set; } = string.Empty;      // Item code + name
    public decimal Qty { get; set; }                          // CPD_OQTY
    public string Unit { get; set; } = string.Empty;          // UOM
    public decimal Rate { get; set; }                         // CPD_RATE
    public decimal Amount { get; set; }                       // CPD_AMT
}

/// <summary>
/// Totals and summary information
/// </summary>
public class PoTotalsPrintInfo
{
    public decimal TotalQty { get; set; }                     // Sum of all quantities
    public decimal AssessableValue { get; set; }              // Total before tax
    public decimal CentralTax { get; set; }                   // CGST amount
    public decimal StateUnionTerritoryTax { get; set; }       // SGST amount
    public decimal TotalAmount { get; set; }                  // CPOM_G_AMT (Grand Total)
    public string AmountInWords { get; set; } = string.Empty; // Amount in words
}

/// <summary>
/// PO copy type (same as Invoice)
/// </summary>
public enum PoCopyType
{
    Original = 0,        // 1 copy: Original
    Duplicate = 1,       // 2 copies: Original, Duplicate
    Triplicate = 2,      // 3 copies: Original, Duplicate, Triplicate
    Quadruplicate = 3    // 4 copies: Original, Duplicate, Triplicate, Quadruplicate
}

