using ErpBE.Application.DeliveryChallan.Commands;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.DeliveryChallan.Handlers;

public class DeleteDeliveryChallanCommandHandler : IRequestHandler<DeleteDeliveryChallanCommand, bool>
{
    private readonly IDeliveryChallanRepository _repository;
    private readonly IActivityLogService        _activityLog;
    private readonly ICompanyContext            _ctx;

    public DeleteDeliveryChallanCommandHandler(
        IDeliveryChallanRepository repository,
        IActivityLogService        activityLog,
        ICompanyContext            ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<bool> Handle(DeleteDeliveryChallanCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.ChallanCode, request.CompanyCode);
        if (existing?.IsModifyLocked == true)
            throw new InvalidOperationException($"Delivery Challan {request.ChallanCode} is locked and cannot be deleted.");

        var deleted = await _repository.DeleteAsync(request.ChallanCode, request.CompanyCode);

        if (deleted)
        {
            await _activityLog.WriteLogAsync(
                companyId: request.CompanyCode,
                source:    "DeliveryChallan",
                @event:    "DELETE",
                docName:   "Delivery Challan",
                docNo:     string.Empty,
                docCode:   request.ChallanCode,
                userName:  _ctx.Username,
                userCode:  _ctx.UserCode,
                cancellationToken: cancellationToken);
        }

        return deleted;
    }
}
