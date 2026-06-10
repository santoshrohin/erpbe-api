using ErpBE.Application.Interfaces;
using ErpBE.Application.SoTypeMaster.Commands;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Handlers
{
    public class CreateSoTypeMasterCommandHandler : IRequestHandler<CreateSoTypeMasterCommand, int>
    {
        private readonly ISoTypeMasterRepository _repository;
        private readonly IActivityLogService     _activityLog;
        private readonly ICompanyContext         _ctx;

        public CreateSoTypeMasterCommandHandler(
            ISoTypeMasterRepository repository,
            IActivityLogService     activityLog,
            ICompanyContext         ctx)
        {
            _repository  = repository;
            _activityLog = activityLog;
            _ctx         = ctx;
        }

        public async Task<int> Handle(CreateSoTypeMasterCommand request, CancellationToken cancellationToken)
        {
            request.Request.ShortName   = request.Request.ShortName.Trim();
            request.Request.Description = request.Request.Description.Trim();
            request.Request.FirstLetter = request.Request.FirstLetter.Trim();

            var isUnique = await _repository.IsShortNameUniqueAsync(
                request.Request.ShortName,
                request.Request.CompanyId);

            if (!isUnique)
                throw new InvalidOperationException("Short Name Already Exists");

            var result = await _repository.CreateAsync(request.Request);

            await _activityLog.WriteLogAsync(
                companyId: request.Request.CompanyId,
                source:    "SoTypeMaster",
                @event:    "INSERT",
                docName:   "SO Type Master",
                docNo:     request.Request.ShortName,
                docCode:   result,
                userName:  _ctx.Username,
                userCode:  _ctx.UserCode,
                cancellationToken: cancellationToken);

            return result;
        }
    }
}
