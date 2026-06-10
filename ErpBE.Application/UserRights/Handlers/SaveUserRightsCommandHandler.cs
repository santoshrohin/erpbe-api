using ErpBE.Application.Interfaces;
using ErpBE.Application.UserRights.Commands;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.UserRights.Handlers;

public class SaveUserRightsCommandHandler : IRequestHandler<SaveUserRightsCommand, bool>
{
    private readonly IUserRightRepository _repository;
    private readonly IActivityLogService  _activityLog;
    private readonly ICompanyContext      _ctx;

    public SaveUserRightsCommandHandler(
        IUserRightRepository repository,
        IActivityLogService  activityLog,
        ICompanyContext      ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<bool> Handle(SaveUserRightsCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.UpsertUserRightsAsync(request.UserCode, request.Rights, cancellationToken);

        if (result)
            await _activityLog.WriteLogAsync(
                companyId: _ctx.CompanyId, source: "UserRights", @event: "SAVE",
                docName: "User Rights", docNo: string.Empty, docCode: request.UserCode,
                userName: _ctx.Username, userCode: _ctx.UserCode,
                cancellationToken: cancellationToken);

        return result;
    }
}

public class CopyUserRightsCommandHandler : IRequestHandler<CopyUserRightsCommand, bool>
{
    private readonly IUserRightRepository _repository;
    private readonly IActivityLogService  _activityLog;
    private readonly ICompanyContext      _ctx;

    public CopyUserRightsCommandHandler(
        IUserRightRepository repository,
        IActivityLogService  activityLog,
        ICompanyContext      ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<bool> Handle(CopyUserRightsCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.CopyRightsAsync(request.FromUserCode, request.ToUserCode, cancellationToken);

        if (result)
            await _activityLog.WriteLogAsync(
                companyId: _ctx.CompanyId, source: "UserRights", @event: "COPY",
                docName: "User Rights", docNo: $"from:{request.FromUserCode}", docCode: request.ToUserCode,
                userName: _ctx.Username, userCode: _ctx.UserCode,
                cancellationToken: cancellationToken);

        return result;
    }
}

public class DeleteUserRightsCommandHandler : IRequestHandler<DeleteUserRightsCommand, bool>
{
    private readonly IUserRightRepository _repository;
    private readonly IActivityLogService  _activityLog;
    private readonly ICompanyContext      _ctx;

    public DeleteUserRightsCommandHandler(
        IUserRightRepository repository,
        IActivityLogService  activityLog,
        ICompanyContext      ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<bool> Handle(DeleteUserRightsCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.DeleteUserRightsAsync(request.UserCode, cancellationToken);

        if (result)
            await _activityLog.WriteLogAsync(
                companyId: _ctx.CompanyId, source: "UserRights", @event: "DELETE",
                docName: "User Rights", docNo: string.Empty, docCode: request.UserCode,
                userName: _ctx.Username, userCode: _ctx.UserCode,
                cancellationToken: cancellationToken);

        return result;
    }
}
