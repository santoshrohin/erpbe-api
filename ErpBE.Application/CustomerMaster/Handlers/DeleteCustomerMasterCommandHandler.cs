using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.CustomerMaster.Interfaces;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.CustomerMaster.Handlers;

public class DeleteCustomerMasterCommandHandler : IRequestHandler<DeleteCustomerMasterCommand, Unit>
{
    private readonly ICustomerMasterRepository _repository;
    private readonly IActivityLogService       _activityLog;
    private readonly ICompanyContext           _ctx;

    public DeleteCustomerMasterCommandHandler(
        ICustomerMasterRepository repository,
        IActivityLogService       activityLog,
        ICompanyContext           ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<Unit> Handle(DeleteCustomerMasterCommand request, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(request.Id, request.CompanyId);

        await _activityLog.WriteLogAsync(
            companyId: request.CompanyId,
            source:    "CustomerMaster",
            @event:    "DELETE",
            docName:   "Customer Master",
            docNo:     string.Empty,
            docCode:   request.Id,
            userName:  _ctx.Username,
            userCode:  _ctx.UserCode,
            cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
