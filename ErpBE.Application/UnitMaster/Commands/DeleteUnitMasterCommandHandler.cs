using MediatR;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;

namespace ErpBE.Application.UnitMaster.Commands
{
    public class DeleteUnitMasterCommandHandler : IRequestHandler<DeleteUnitMasterCommand, bool>
    {
        private readonly IUnitMasterService  _unitMasterService;
        private readonly IActivityLogService _activityLog;
        private readonly ICompanyContext     _ctx;

        public DeleteUnitMasterCommandHandler(
            IUnitMasterService  unitMasterService,
            IActivityLogService activityLog,
            ICompanyContext     ctx)
        {
            _unitMasterService = unitMasterService;
            _activityLog       = activityLog;
            _ctx               = ctx;
        }

        public async Task<bool> Handle(DeleteUnitMasterCommand request, CancellationToken cancellationToken)
        {
            var result = await _unitMasterService.DeleteUnitMasterAsync(request.Id);

            if (result)
            {
                await _activityLog.WriteLogAsync(
                    companyId: _ctx.CompanyId,
                    source:    "UnitMaster",
                    @event:    "DELETE",
                    docName:   "Unit Master",
                    docNo:     string.Empty,
                    docCode:   request.Id,
                    userName:  _ctx.Username,
                    userCode:  _ctx.UserCode,
                    cancellationToken: cancellationToken);
            }

            return result;
        }
    }
}
