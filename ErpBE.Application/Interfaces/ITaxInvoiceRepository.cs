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
        Task<bool> DeleteTaxInvoiceAsync(int invoiceCode, int companyCode);
        
        /// <summary>
        /// Gets a Tax Invoice by ID with all line items
        /// </summary>
        Task<TaxInvoiceMasterDto?> GetTaxInvoiceByIdAsync(int invoiceCode, int companyCode);
        
        /// <summary>
        /// Gets all Tax Invoices with filtering, searching, sorting, and pagination
        /// </summary>
        Task<TaxInvoicePagedResponse> GetAllTaxInvoicesAsync(GetAllTaxInvoicesQuery query);
        
        /// <summary>
        /// Checks if an invoice is locked for modification
        /// </summary>
        Task<bool> IsInvoiceLockedAsync(int invoiceCode);
        
        /// <summary>
        /// Locks an invoice for modification (Concurrency Control)
        /// </summary>
        Task<bool> LockInvoiceAsync(int invoiceCode);
        
        /// <summary>
        /// Unlocks an invoice after modification
        /// </summary>
        Task<bool> UnlockInvoiceAsync(int invoiceCode);
        
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
        Task<TaxInvoicePrintDto?> GetPrintDataAsync(int invoiceCode, int companyId);
    }
}

