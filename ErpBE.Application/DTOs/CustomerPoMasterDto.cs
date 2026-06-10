namespace ErpBE.Application.DTOs;

/// <summary>
/// Customer PO Master DTO representing CUSTPO_MASTER table
/// </summary>
public class CustomerPoMasterDto
{
    // Primary Key
    public int PoCode { get; set; }

    // Core PO Information
    public int CustomerCode { get; set; }
    public string? CustomerName { get; set; } // Joined from PARTY_MASTER
    public string PoNumber { get; set; } = string.Empty;
    public int DocumentNumber { get; set; }
    public int PoType { get; set; }
    public string? PoTypeName { get; set; } // Joined from master
    public DateTime PoDate { get; set; }
    public int CreditDays { get; set; }
    public int CompanyId { get; set; }
    public string? WorkOrderNumber { get; set; }

    // System Fields
    public bool IsLocked { get; set; }
    public bool IsDeleted { get; set; }

    // Payment & Terms
    public string? PaymentTerms { get; set; }
    public bool IsAuthorized { get; set; }
    public DateTime? CustomerPoDate { get; set; }

    // Quotation Reference
    public int? QuotationCode { get; set; }

    // Tax Information
    public string? TaxName { get; set; }
    public double? TaxPercentage { get; set; }
    public double? TaxAmount { get; set; }
    public double? ExcisePercentage { get; set; } // Legacy
    public double? ExciseEducationPercentage { get; set; } // Legacy
    public double? ExciseHigherEducationPercentage { get; set; } // Legacy

    // Amount Fields
    public double? BasicAmount { get; set; }
    public double? DiscountPercentage { get; set; }
    public double? DiscountAmount { get; set; }
    public string? DiscountReason { get; set; }
    public double? DeviationAmount { get; set; }
    public string? DeviationReason { get; set; }
    public double? PackingAmount { get; set; }
    public double? ExciseAmount { get; set; } // Legacy

    // Totals
    public double? RoundingAmount { get; set; }
    public double? GrandTotal { get; set; }

    // Invoice Reference
    public bool InvoiceGenerated { get; set; }
    public int AmendmentCount { get; set; }

    // Listing grid computed columns
    public string? CustomerPartNo { get; set; } // CPOD_CUST_I_CODE from first detail — shown in ViewCustomerPO grid

    // Export Information
    public string? FinalDestination { get; set; }
    public string? PreCarriageBy { get; set; }
    public string? PortOfLoading { get; set; }
    public string? PortOfDischarge { get; set; }
    public string? PlaceOfDelivery { get; set; }
    public string? BuyerName { get; set; }
    public string? BuyerAddress { get; set; }

    // Currency
    public int? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; } // Joined from master

    // Amendment
    public DateTime? AmendmentDate { get; set; }
    public int? InquiryCode { get; set; }

    // Miscellaneous
    public bool IsVerbalOrder { get; set; }
    public int? ProjectCode { get; set; }
    public string? ProjectName { get; set; }

    // Navigation Property
    public List<CustomerPoDetailDto> Details { get; set; } = new();
}

