namespace ErpBE.Application.DTOs;

/// <summary>
/// Main DTO containing all data required for printing a Tax Invoice
/// Matches EXACT format from actual invoice image (taxinvoice_page-0001.jpg)
/// </summary>
public class TaxInvoicePrintDto
{
    // Company Information
    public CompanyPrintInfo Company { get; set; } = new();
    
    // Invoice Header Data
    public InvoiceHeaderPrintInfo InvoiceHeader { get; set; } = new();
    
    // Recipient Details (Left side - "Details Of Recipient")
    public RecipientPrintInfo Recipient { get; set; } = new();
    
    // Delivery Details (Right side - "Details Of Delivery")
    public DeliveryPrintInfo Delivery { get; set; } = new();
    
    // Invoice Line Items
    public List<InvoiceDetailPrintInfo> LineItems { get; set; } = new();
    
    // Totals and Tax Summary
    public TotalsPrintInfo Totals { get; set; } = new();
    
    // E-Invoice Data
    public EInvoicePrintInfo? EInvoice { get; set; }
    
    // Copy Type
    public InvoiceCopyType CopyType { get; set; }
    
    // Declaration Text
    public string Declaration { get; set; } = "( Certify that particular given are true and correct amount represents the price actually charged & there is no flow of additional consideration directly or indirectly from the buyer )";
    
    // Terms and Conditions
    public List<string> TermsAndConditions { get; set; } = new();
}

/// <summary>
/// Company information for invoice header
/// Maps to: "SUN ELECTRO DEVICES PVT LTD." and address
/// </summary>
public class CompanyPrintInfo
{
    public string CompanyName { get; set; } = string.Empty;  // CM_NAME
    public string FullAddress { get; set; } = string.Empty;  // Combined: Plot No. A-44/1/2/19-20, Chakan MIDC...
    public string GstinNo { get; set; } = string.Empty;      // CM_GST_NO (for header section)
}

/// <summary>
/// Invoice header information (left and right columns)
/// </summary>
public class InvoiceHeaderPrintInfo
{
    // Left Column
    public DateTime DateOfInvoice { get; set; }              // INM_DATE
    public string InvoiceSerialNo { get; set; } = string.Empty;  // INM_TNO (e.g., "SUN252605801")
    public string GstinNo { get; set; } = string.Empty;      // Company GSTIN (27AANCS2439P1ZL)
    public string? EWayBillNo { get; set; }                  // INM_EWAY_BILL_NO or E-Invoice field
    
    // Right Column
    public string? TransporatationMode { get; set; }         // INM_TRANSPORT (note: typo "Transporatation" in actual invoice!)
    public string? VehicleNo { get; set; }                   // INM_VEH_NO
    public string? PoNo { get; set; }                        // From CUSTPO_MASTER.CPOM_PONO
    public DateTime? DateAndTimeOfSupply { get; set; }       // INM_DATE with time
    public string? PlaceOfSupply { get; set; }               // INM_PLACE_OF_SUPPLY or state
}

/// <summary>
/// Recipient details (left side - "Details Of Recipient")
/// </summary>
public class RecipientPrintInfo
{
    public string Name { get; set; } = string.Empty;         // P_NAME
    public string Address { get; set; } = string.Empty;      // P_ADD1, P_ADD2, P_CITY, P_PIN_CODE
    public string StateName { get; set; } = string.Empty;    // P_STATE
    public string StateCode { get; set; } = string.Empty;    // State code (need mapping)
    public string GstinNo { get; set; } = string.Empty;      // P_GST_NO
}

/// <summary>
/// Delivery details (right side - "Details Of Delivery")
/// Usually same as Recipient, but can be different
/// </summary>
public class DeliveryPrintInfo
{
    public string Name { get; set; } = string.Empty;         // Shipping party name
    public string Address { get; set; } = string.Empty;      // Shipping address
    public string StateName { get; set; } = string.Empty;    // Shipping state
    public string StateCode { get; set; } = string.Empty;    // Shipping state code
    public string GstinNo { get; set; } = string.Empty;      // Shipping GSTIN
}

/// <summary>
/// Invoice line item detail
/// Maps to table: Sr. No | Description | HSN/SAC | UOM | Qty | Rate/Unit | Taxable Value
/// </summary>
public class InvoiceDetailPrintInfo
{
    public int SrNo { get; set; }                            // Row number
    public string DescriptionOfGoodsOrServices { get; set; } = string.Empty;  // Item code + name
    public string HsnSac { get; set; } = string.Empty;       // IND_HSN_CODE
    public string Uom { get; set; } = string.Empty;          // Unit of measurement
    public decimal Qty { get; set; }                         // IND_INQTY
    public decimal RatePerUnit { get; set; }                 // IND_RATE
    public decimal TaxableValue { get; set; }                // IND_AMT
    
    // Tax percentages (for tax summary)
    public decimal CgstPercentage { get; set; }              // E_BASIC_CentralT
    public decimal SgstPercentage { get; set; }              // E_EDU_CESS_State
    public decimal IgstPercentage { get; set; }              // E_H_EDU_Integrated
}

/// <summary>
/// Totals and tax summary
/// Matches the exact format: Less/Add sections, then taxes, then grand total
/// </summary>
public class TotalsPrintInfo
{
    // Less/Add sections
    public decimal Discount { get; set; }                    // INM_DISC_AMT
    public decimal PackingAndForwardingCharges { get; set; } // INM_PACK_AMT
    public decimal FrieghtAndInsurance { get; set; }         // INM_FREIGHT + INM_INSURANCE (note: typo "Frieght" in actual!)
    public decimal OtherCharges { get; set; }                // INM_OTHER_AMT
    
    // Taxable Value
    public decimal TaxableValue { get; set; }                // Sum after Less/Add
    
    // Taxes (Note: Labels are "Central Tax" and "State/Union Territory Tax", NOT CGST/SGST)
    public decimal CentralTaxPercentage { get; set; }        // e.g., 9.00
    public decimal CentralTaxAmount { get; set; }            // Calculated CGST
    public decimal StateUnionTerritoryTaxPercentage { get; set; }  // e.g., 9.00
    public decimal StateUnionTerritoryTaxAmount { get; set; }      // Calculated SGST
    public decimal IntegratedTaxPercentage { get; set; }     // e.g., 0.00
    public decimal IntegratedTaxAmount { get; set; }         // Calculated IGST
    
    // Grand Total
    public decimal GrandTotal { get; set; }                  // INM_G_AMT
    public string AmountInWords { get; set; } = string.Empty;  // "Forty-Three Thousand Seven Hundred Four Only"
}

/// <summary>
/// E-Invoice information (bottom left section with QR code)
/// </summary>
public class EInvoicePrintInfo
{
    public string Irn { get; set; } = string.Empty;          // f33f18700034cc9bb32e9836c56fdfa0e...
    public string AckNo { get; set; } = string.Empty;        // 122529238217864
    public DateTime? AckDate { get; set; }                   // 2025-10-24 19:17:00
    public byte[]? QrCodeImage { get; set; }                 // QR code as image bytes
}

/// <summary>
/// Invoice copy type
/// </summary>
public enum InvoiceCopyType
{
    Original = 0,        // 1 copy: Original
    Duplicate = 1,       // 2 copies: Original, Duplicate
    Triplicate = 2,      // 3 copies: Original, Duplicate, Triplicate
    Quadruplicate = 3    // 4 copies: Original, Duplicate, Triplicate, Quadruplicate
}
