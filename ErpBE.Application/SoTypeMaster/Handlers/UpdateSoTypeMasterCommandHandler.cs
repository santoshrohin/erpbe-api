using ErpBE.Application.Interfaces;
using ErpBE.Application.SoTypeMaster.Commands;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Handlers
{
    public class UpdateSoTypeMasterCommandHandler : IRequestHandler<UpdateSoTypeMasterCommand, Unit>
    {
        private readonly ISoTypeMasterRepository _repository;
        private readonly IActivityLogService     _activityLog;
        private readonly ICompanyContext         _ctx;

        public UpdateSoTypeMasterCommandHandler(
            ISoTypeMasterRepository repository,
            IActivityLogService     activityLog,
            ICompanyContext         ctx)
        {
            _repository  = repository;
            _activityLog = activityLog;
            _ctx         = ctx;
        }

        public async Task<Unit> Handle(UpdateSoTypeMasterCommand request, CancellationToken cancellationToken)
        {
            var existing = await _repository.GetByIdAsync(request.Request.Id);
            if (existing == null)
                throw new KeyNotFoundException($"SO Type Master with ID '{request.Request.Id}' not found.");

            request.Request.ShortName   = request.Request.ShortName.Trim();
            request.Request.Description = request.Request.Description.Trim();
            request.Request.FirstLetter = request.Request.FirstLetter.Trim();

            var isUnique = await _repository.IsShortNameUniqueAsync(
                request.Request.ShortName,
                request.Request.CompanyId,
                request.Request.Id);

            if (!isUnique)
                throw new InvalidOperationException("Short Name Already Exists");

            var success = await _repository.UpdateAsync(request.Request);
            if (!success)
                throw new InvalidOperationException("Failed to update SO Type Master.");

            await _activityLog.WriteLogAsync(
                companyId: request.Request.CompanyId,
                source:    "SoTypeMaster",
                @event:    "UPDATE",
                docName:   "SO Type Master",
                docNo:     request.Request.ShortName,
                docCode:   request.Request.Id,
                userName:  _ctx.Username,
                userCode:  _ctx.UserCode,
                cancellationToken: cancellationToken);

            return Unit.Value;
        }
    }
}
