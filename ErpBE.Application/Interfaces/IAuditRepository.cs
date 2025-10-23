using ErpBE.Application.DTOs;
using ErpBE.Application.Common.Models;
using ErpBE.Domain.Audit;

namespace ErpBE.Application.Interfaces
{
    public interface IAuditRepository
    {
        Task<int> CreateAsync(AuditEntry auditEntry);
        Task<AuditEntry?> GetByIdAsync(int id);
        Task<PagedResponse<AuditEntryDto>> GetPagedAsync(AuditQueryParameters parameters);
        Task<List<AuditEntryDto>> GetAuditHistoryAsync(string entityName, string entityId);
        Task<AuditConfiguration?> GetAuditConfigurationAsync(string endpoint, string httpMethod);
        Task<int> CreateConfigurationAsync(AuditConfiguration configuration);
        Task<bool> UpdateConfigurationAsync(AuditConfiguration configuration);
        Task<List<AuditConfiguration>> GetAllConfigurationsAsync();
    }
}
