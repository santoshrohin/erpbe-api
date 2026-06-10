using MediatR;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;

namespace ErpBE.Application.UnitMaster.Commands
{
    public class CreateUnitMasterCommandHandler : IRequestHandler<CreateUnitMasterCommand, int>
    {
        private readonly IUnitMasterService  _unitMasterService;
        private readonly IActivityLogService _activityLog;
        private readonly ICompanyContext     _ctx;

        public CreateUnitMasterCommandHandler(
            IUnitMasterService  unitMasterService,
            IActivityLogService activityLog,
            ICompanyContext     ctx)
        {
            _unitMasterService = unitMasterService;
            _activityLog       = activityLog;
            _ctx               = ctx;
        }

        public async Task<int> Handle(CreateUnitMasterCommand request, CancellationToken cancellationToken)
        {
            var result = await _unitMasterService.CreateUnitMasterAsync(request.Request);

            await _activityLog.WriteLogAsync(
                companyId: request.Request.CompanyId,
                source:    "UnitMaster",
                @event:    "INSERT",
                docName:   "Unit Master",
                docNo:     request.Request.UnitName,
                docCode:   result,
                userName:  _ctx.Username,
                userCode:  _ctx.UserCode,
                cancellationToken: cancellationToken);

            return result;
        }
    }
}
