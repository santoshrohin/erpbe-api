using ErpBE.Application.DTOs;

namespace ErpBE.Application.Interfaces;

/// <summary>
/// Repository interface for Customer PO operations
/// </summary>
public interface ICustomerPoRepository
{
    /// <summary>
    /// Creates a new Customer PO with details (transactional)
    /// </summary>
    Task<CustomerPoMasterDto> CreateAsync(CustomerPoMasterDto po, IEnumerable<CustomerPoDetailDto> details);

    /// <summary>
    /// Updates an existing Customer PO with details (transactional)
    /// </summary>
    Task<CustomerPoMasterDto> UpdateAsync(CustomerPoMasterDto po, IEnumerable<CustomerPoDetailDto> details);

    /// <summary>
    /// Soft deletes a Customer PO and all its details
    /// </summary>
    Task<bool> DeleteAsync(int poCode, int companyId);

    /// <summary>
    /// Gets a Customer PO by ID with all details
    /// </summary>
    Task<CustomerPoMasterDto?> GetByIdAsync(int poCode, int companyId);

    /// <summary>
    /// Gets all Customer POs with pagination, filtering, and sorting
    /// </summary>
    Task<(IEnumerable<CustomerPoMasterDto> Data, int TotalCount)> GetAllAsync(CustomerPoQueryParameters parameters);

    /// <summary>
    /// Atomically acquires the lock. Returns true if acquired, false if another user holds an
    /// active lock. Expired locks (older than the SP's timeout) are overwritten.
    /// </summary>
    Task<bool> LockAsync(int poCode, int lockedByUserId);

    /// <summary>
    /// Releases the lock and clears timestamp/user columns.
    /// </summary>
    Task UnlockAsync(int poCode);

    /// <summary>
    /// Gets Customer PO print data with all required information for PDF generation
    /// </summary>
    Task<CustomerPoPrintDto?> GetPrintDataAsync(int poCode, int companyId, int companyCode, PoCopyType copyType);

    /// <summary>
    /// Returns true if a PO with the given number already exists for the company (case-insensitive).
    /// Pass excludePoCode to ignore the current record when checking on update.
    /// </summary>
    Task<bool> PoNumberExistsAsync(string poNumber, int companyId, int? excludePoCode = null);

    /// <summary>
    /// Returns true if the PO is referenced by a live Work Order (ES_DELETE=0).
    /// Legacy ViewCustomerPO.aspx.cs blocks MODIFY when a WO references the PO.
    /// </summary>
    Task<bool> IsUsedInWorkOrderAsync(int poCode);

    /// <summary>
    /// Amends an existing Customer PO: archives master+details to CUSTPO_AM_MASTER/CUSTPO_AMD_DETAIL,
    /// increments CPOM_AM_COUNT, updates master with new data, deletes old details,
    /// then re-inserts the new detail lines. Mirrors legacy CustomerPO.aspx.cs AMEND path.
    /// Returns the updated AmendmentCount.
    /// </summary>
    Task<int> AmendAsync(CustomerPoMasterDto po, IEnumerable<CustomerPoDetailDto> details);
}

