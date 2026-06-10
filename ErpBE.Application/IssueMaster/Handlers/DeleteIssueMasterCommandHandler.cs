using ErpBE.Application.Interfaces;
using ErpBE.Application.IssueMaster.Commands;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.IssueMaster.Handlers;

public class DeleteIssueMasterCommandHandler : IRequestHandler<DeleteIssueMasterCommand, bool>
{
    private readonly IIssueMasterRepository _repository;
    private readonly IActivityLogService    _activityLog;
    private readonly ICompanyContext        _ctx;

    public DeleteIssueMasterCommandHandler(
        IIssueMasterRepository repository,
        IActivityLogService    activityLog,
        ICompanyContext        ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<bool> Handle(DeleteIssueMasterCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(request.IssueCode, request.CompanyCode);

        if (deleted)
        {
            await _activityLog.WriteLogAsync(
                companyId: request.CompanyCode,
                source:    "IssueMaster",
                @event:    "DELETE",
                docName:   "Issue to Production",
                docNo:     string.Empty,
                docCode:   request.IssueCode,
                userName:  _ctx.Username,
                userCode:  _ctx.UserCode,
                cancellationToken: cancellationToken);
        }

        return deleted;
    }
}
