using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.CustomerPo.Commands;

/// <summary>
/// Command to update an existing Customer PO
/// </summary>
public class UpdateCustomerPoCommand : IRequest<CustomerPoMasterDto>
{
    public int PoCode { get; set; }

    // Core PO Information
    public int CustomerCode { get; set; }
    public string PoNumber { get; set; } = string.Empty;
    public int PoType { get; set; }
    public DateTime PoDate { get; set; }
    public int CreditDays { get; set; }
    public int CompanyId { get; set; }
    public string? WorkOrderNumber { get; set; }

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
    public double? ExcisePercentage { get; set; }
    public double? ExciseEducationPercentage { get; set; }
    public double? ExciseHigherEducationPercentage { get; set; }

    // Amount Fields
    public double? BasicAmount { get; set; }
    public double? DiscountPercentage { get; set; }
    public double? DiscountAmount { get; set; }
    public string? DiscountReason { get; set; }
    public double? DeviationAmount { get; set; }
    public string? DeviationReason { get; set; }
    public double? PackingAmount { get; set; }
    public double? ExciseAmount { get; set; }

    // Totals
    public double? RoundingAmount { get; set; }
    public double? GrandTotal { get; set; }

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

    // Amendment
    public int? InquiryCode { get; set; }

    // Miscellaneous
    public bool IsVerbalOrder { get; set; }
    public int? ProjectCode { get; set; }
    public string? ProjectName { get; set; }

    // Line Items
    public List<CreateCustomerPoDetailCommand> Details { get; set; } = new();
}

