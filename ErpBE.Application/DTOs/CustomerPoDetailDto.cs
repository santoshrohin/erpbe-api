namespace ErpBE.Application.DTOs;

/// <summary>
/// Customer PO Detail DTO representing CUSTPO_DETAIL table
/// </summary>
public class CustomerPoDetailDto
{
    // Foreign Key
    public int PoCode { get; set; }

    // Core Item Information
    public int ItemCode { get; set; }
    public string? ItemName { get; set; } // Joined from ITEM_MASTER
    public int UomCode { get; set; }
    public string? UomName { get; set; } // Joined from UNIT_MASTER
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
    public string? StoreName { get; set; } // Joined from STORE_MASTER
    public int? CurrencyCode { get; set; }
    public string? CurrencyName { get; set; } // Joined from CURRENCY_MASTER

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

