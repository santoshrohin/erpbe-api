namespace ErpBE.Application.DTOs;

/// <summary>
/// Query parameters for Customer PO list with pagination, filtering, and sorting
/// </summary>
public class CustomerPoQueryParameters
{
    public int CompanyId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool IsActive { get; set; } = true;

    // Search/Filter Fields
    public string? SearchTerm { get; set; }
    public string? PoNumber { get; set; }
    public int? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? WorkOrderNumber { get; set; }
    public string? CustomerItemCode { get; set; }
    public int? PoType { get; set; }
    public int? ProjectCode { get; set; }
    public bool? InvoiceGenerated { get; set; }
    public bool? HasAmendment { get; set; }
    public bool? IsVerbalOrder { get; set; }

    // Sorting
    public string SortBy { get; set; } = "PoCode";
    public string SortOrder { get; set; } = "DESC";
}

