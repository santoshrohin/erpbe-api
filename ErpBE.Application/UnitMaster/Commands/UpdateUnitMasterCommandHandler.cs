using MediatR;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;

namespace ErpBE.Application.UnitMaster.Commands
{
    public class UpdateUnitMasterCommandHandler : IRequestHandler<UpdateUnitMasterCommand, bool>
    {
        private readonly IUnitMasterService  _unitMasterService;
        private readonly IActivityLogService _activityLog;
        private readonly ICompanyContext     _ctx;

        public UpdateUnitMasterCommandHandler(
            IUnitMasterService  unitMasterService,
            IActivityLogService activityLog,
            ICompanyContext     ctx)
        {
            _unitMasterService = unitMasterService;
            _activityLog       = activityLog;
            _ctx               = ctx;
        }

        public async Task<bool> Handle(UpdateUnitMasterCommand request, CancellationToken cancellationToken)
        {
            var result = await _unitMasterService.UpdateUnitMasterAsync(request.Request);

            if (result)
            {
                await _activityLog.WriteLogAsync(
                    companyId: _ctx.CompanyId,
                    source:    "UnitMaster",
                    @event:    "UPDATE",
                    docName:   "Unit Master",
                    docNo:     request.Request.UnitName,
                    docCode:   request.Request.Id,
                    userName:  _ctx.Username,
                    userCode:  _ctx.UserCode,
                    cancellationToken: cancellationToken);
            }

            return result;
        }
    }
}
