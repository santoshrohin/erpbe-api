using ErpBE.Application.DTOs;

namespace ErpBE.Application.Audit
{
    public interface IAuditService
    {
        Task LogCreateAsync(string tableName, int recordId, object newValues, string createdBy, string? ipAddress = null, string? userAgent = null, string? sessionId = null);
        Task LogUpdateAsync(string tableName, int recordId, object oldValues, object newValues, string modifiedBy, string? ipAddress = null, string? userAgent = null, string? sessionId = null);
        Task LogDeleteAsync(string tableName, int recordId, object oldValues, string deletedBy, string? ipAddress = null, string? userAgent = null, string? sessionId = null);
        Task<List<AuditTrailDto>> GetAuditTrailAsync(string tableName, int? recordId = null, int pageNumber = 1, int pageSize = 50);
    }
}
