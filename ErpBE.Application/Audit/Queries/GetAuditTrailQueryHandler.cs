using ErpBE.Application.Audit.Queries;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.Audit.Handlers
{
    public class GetAuditTrailQueryHandler : IRequestHandler<GetAuditTrailQuery, List<AuditTrailDto>>
    {
        private readonly IAuditService _auditService;

        public GetAuditTrailQueryHandler(IAuditService auditService)
        {
            _auditService = auditService;
        }

        public async Task<List<AuditTrailDto>> Handle(GetAuditTrailQuery request, CancellationToken cancellationToken)
        {
            return await _auditService.GetAuditTrailAsync(
                request.TableName,
                request.RecordId,
                request.PageNumber,
                request.PageSize);
        }
    }
}

