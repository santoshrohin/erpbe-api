using ErpBE.Application.DTOs;
using ErpBE.Application.DTOs.LabourChargeInvoice;

namespace ErpBE.Application.Interfaces
{
    public interface ILabourChargeInvoiceRepository
    {
        Task<LabourChargeInvoicePagedResponse> GetAllAsync(LabourChargeInvoiceQueryParameters p);
        Task<LabourChargeInvoiceMasterDto?> GetByIdAsync(int id, int companyCode);
        Task<LabourChargeInvoiceMasterDto> CreateAsync(CreateLabourChargeInvoiceRequest req);
        Task<LabourChargeInvoiceMasterDto?> UpdateAsync(UpdateLabourChargeInvoiceRequest req);
        Task<bool> DeleteAsync(int id, int companyCode);
        Task<bool> LockAsync(int id, int companyCode, int lockedByUserId);
        Task<bool> UnlockAsync(int id, int companyCode);
        Task<LabourChargeInvoicePrintDto?> GetPrintDataAsync(int invoiceCode, int companyCode);
    }
}
