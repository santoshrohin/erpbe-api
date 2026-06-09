using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.CustomerPo.Commands;

public class DeleteCustomerPoCommandHandler : IRequestHandler<DeleteCustomerPoCommand, bool>
{
    private readonly ICustomerPoRepository _repository;
    private readonly IActivityLogService   _activityLog;
    private readonly ICompanyContext       _ctx;

    public DeleteCustomerPoCommandHandler(
        ICustomerPoRepository repository,
        IActivityLogService   activityLog,
        ICompanyContext       ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<bool> Handle(DeleteCustomerPoCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.PoCode, request.CompanyId);
        if (existing?.IsLocked == true)
            throw new InvalidOperationException($"Customer PO {request.PoCode} is locked and cannot be deleted.");

        var deleted = await _repository.DeleteAsync(request.PoCode, request.CompanyId);

        if (deleted)
        {
            await _activityLog.WriteLogAsync(
                companyId: request.CompanyId,
                source:    "CustomerPo",
                @event:    "DELETE",
                docName:   "Customer PO",
                docNo:     string.Empty,
                docCode:   request.PoCode,
                userName:  _ctx.Username,
                userCode:  _ctx.UserCode,
                cancellationToken: cancellationToken);
        }

        return deleted;
    }
}

