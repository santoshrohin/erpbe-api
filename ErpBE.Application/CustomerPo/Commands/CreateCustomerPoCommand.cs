using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.CustomerPo.Commands;

/// <summary>
/// Command to create a new Customer PO with details
/// </summary>
public class CreateCustomerPoCommand : IRequest<CustomerPoMasterDto>
{
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

/// <summary>
/// Command to create a Customer PO detail line item
/// </summary>
public class CreateCustomerPoDetailCommand
{
    public int ItemCode { get; set; }
    public int UomCode { get; set; }
    public double OrderedQuantity { get; set; }
    public double Rate { get; set; }
    public double Amount { get; set; }
    public string? Description { get; set; }

    // Customer Item Reference
    public string? CustomerItemCode { get; set; }
    public string? CustomerItemName { get; set; }

    // Status & Dispatch
    public int Status { get; set; }
    public double DispatchedQuantity { get; set; }
    public bool IsOrder { get; set; }

    // Store & Currency
    public int? StoreCode { get; set; }
    public int? CurrencyCode { get; set; }

    // Work Order
    public double? WorkOrderQuantity { get; set; }

    // Modification Tracking
    public string? ModificationNumber { get; set; }
    public DateTime? ModificationDate { get; set; }

    // Amortization
    public double? AmortizationRate { get; set; }
    public double? DieAmortizationRate { get; set; }

    // Discount
    public double? DiscountPercentage { get; set; }
    public double? DiscountAmount { get; set; }
}

