using ErpBE.Application.DTOs;
using ErpBE.Application.DTOs.TaxInvoice;
using ErpBE.Application.TaxInvoice.Queries;

namespace ErpBE.Application.Interfaces
{
    public interface ITaxInvoiceRepository
    {
        /// <summary>
        /// Creates a new Tax Invoice with line items
        /// Returns the created invoice with generated Invoice Number
        /// </summary>
        Task<TaxInvoiceMasterDto> CreateTaxInvoiceAsync(TaxInvoiceMasterDto invoice);
        
        /// <summary>
        /// Updates an existing Tax Invoice with line items
        /// </summary>
        Task<TaxInvoiceMasterDto> UpdateTaxInvoiceAsync(TaxInvoiceMasterDto invoice);
        
        /// <summary>
        /// Soft deletes a Tax Invoice (sets ES_DELETE = 1)
        /// Also reverses stock updates
        /// </summary>
        Task<bool> DeleteTaxInvoiceAsync(long invoiceCode, int companyCode);
        
        /// <summary>
        /// Gets a Tax Invoice by ID with all line items
        /// </summary>
        Task<TaxInvoiceMasterDto?> GetTaxInvoiceByIdAsync(long invoiceCode, int companyCode);
        
        /// <summary>
        /// Gets all Tax Invoices with filtering, searching, sorting, and pagination
        /// </summary>
        Task<TaxInvoicePagedResponse> GetAllTaxInvoicesAsync(GetAllTaxInvoicesQuery query);
        
        /// <summary>
        /// Returns true if the invoice is currently locked by any user.
        /// Used by Update/Delete handlers as a pre-condition guard.
        /// </summary>
        Task<bool> IsInvoiceLockedAsync(long invoiceCode);

        /// <summary>
        /// Atomically acquires the lock. Returns true if acquired, false if another user holds an
        /// active lock. Expired locks (older than the SP's timeout) are overwritten.
        /// </summary>
        Task<bool> LockInvoiceAsync(long invoiceCode, int lockedByUserId);
        
        /// <summary>
        /// Unlocks an invoice after modification
        /// </summary>
        Task<bool> UnlockInvoiceAsync(long invoiceCode);
        
        /// <summary>
        /// Gets available items from a Customer PO
        /// </summary>
        Task<List<TaxInvoiceDetailDto>> GetAvailableItemsFromPoAsync(int customerPoCode, int companyCode);
        
        /// <summary>
        /// Validates if an item has sufficient stock for invoice quantity
        /// </summary>
        Task<bool> ValidateItemStockAsync(int itemCode, int companyCode, double requiredQuantity);
        
        /// <summary>
        /// Generates next Invoice Number for a company
        /// </summary>
        Task<int> GenerateInvoiceNumberAsync(int companyCode);
        
        /// <summary>
        /// Gets comprehensive Tax Invoice data for printing (includes company, customer, items, taxes, e-invoice)
        /// </summary>
        Task<TaxInvoicePrintDto?> GetPrintDataAsync(long invoiceCode, int companyId);

        /// <summary>
        /// Approves a Tax Invoice (sets INM_IS_AUTHORIZED = 1).
        /// Returns false if the invoice was not found.
        /// </summary>
        Task<bool> ApproveAsync(long invoiceCode, int companyCode);

        // ── Dropdown / lookup helpers ───────────────────────────────────────

        /// <summary>Customers that have active POs with pending dispatch qty.</summary>
        Task<List<TaxInvoiceCustomerDto>> GetCustomersWithActivePosAsync(int companyCode);

        /// <summary>Items with pending qty for a specific customer.</summary>
        Task<List<TaxInvoiceItemDto>> GetItemsByCustomerAsync(int customerCode, int companyCode);

        /// <summary>Item details: UOM, stock qty, HSN, CGST/SGST/IGST rates.</summary>
        Task<TaxInvoiceItemDetailsDto?> GetItemDetailsAsync(int itemCode, int companyCode);

        /// <summary>POs for a given item + customer, with pending qty and rate info.</summary>
        Task<List<TaxInvoicePoDto>> GetPOsByItemCustomerAsync(int itemCode, int customerCode, int companyCode, int? invoiceCode);

        /// <summary>Company state code (used to decide CGST+SGST vs IGST).</summary>
        Task<CompanyStateDto?> GetCompanyStateAsync(int companyCode);

        /// <summary>Sales Tax names and rates for the Tax Name dropdown.</summary>
        Task<List<SalesTaxDto>> GetSalesTaxMasterAsync(int companyCode);
    }
}

