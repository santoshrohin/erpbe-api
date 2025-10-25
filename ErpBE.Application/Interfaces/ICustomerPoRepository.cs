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
    /// Checks if a PO is locked for editing
    /// </summary>
    Task<bool> IsLockedAsync(int poCode);

    /// <summary>
    /// Locks a PO for editing
    /// </summary>
    Task LockAsync(int poCode);

    /// <summary>
    /// Unlocks a PO after editing
    /// </summary>
    Task UnlockAsync(int poCode);
}

