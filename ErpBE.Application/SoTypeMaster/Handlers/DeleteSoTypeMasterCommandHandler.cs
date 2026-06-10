using ErpBE.Application.Interfaces;
using ErpBE.Application.SoTypeMaster.Commands;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Handlers
{
    public class DeleteSoTypeMasterCommandHandler : IRequestHandler<DeleteSoTypeMasterCommand, Unit>
    {
        private readonly ISoTypeMasterRepository _repository;
        private readonly IActivityLogService     _activityLog;
        private readonly ICompanyContext         _ctx;

        public DeleteSoTypeMasterCommandHandler(
            ISoTypeMasterRepository repository,
            IActivityLogService     activityLog,
            ICompanyContext         ctx)
        {
            _repository  = repository;
            _activityLog = activityLog;
            _ctx         = ctx;
        }

        public async Task<Unit> Handle(DeleteSoTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var existing = await _repository.GetByIdAsync(request.Id);
            if (existing == null)
                throw new KeyNotFoundException($"SO Type Master with ID '{request.Id}' not found.");

            var isFixed = await _repository.IsFixedRecordAsync(request.Id);
            if (isFixed)
                throw new InvalidOperationException("These record is fixed");

            var isUsed = await _repository.IsUsedInCustomerPOAsync(request.Id);
            if (isUsed)
                throw new InvalidOperationException("You cant delete this record it has used in Sales Order");

            var success = await _repository.DeleteAsync(request.Id);
            if (!success)
                throw new InvalidOperationException("Failed to delete SO Type Master.");

            await _activityLog.WriteLogAsync(
                companyId: existing.CompanyId,
                source:    "SoTypeMaster",
                @event:    "DELETE",
                docName:   "SO Type Master",
                docNo:     string.Empty,
                docCode:   request.Id,
                userName:  _ctx.Username,
                userCode:  _ctx.UserCode,
                cancellationToken: cancellationToken);

            return Unit.Value;
        }
    }
}
