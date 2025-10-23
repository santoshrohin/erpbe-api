using ErpBE.Domain.DTOs;
using MediatR;

namespace ErpBE.Application.Audit.Queries
{
    public class GetAuditTrailQuery : IRequest<List<AuditTrailDto>>
    {
        public string TableName { get; set; } = string.Empty;
        public int? RecordId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}

