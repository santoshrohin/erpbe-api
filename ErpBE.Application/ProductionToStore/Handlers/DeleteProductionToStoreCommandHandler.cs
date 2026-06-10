using ErpBE.Application.Interfaces;
using ErpBE.Application.ProductionToStore.Commands;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.ProductionToStore.Handlers;

public class DeleteProductionToStoreCommandHandler : IRequestHandler<DeleteProductionToStoreCommand, bool>
{
    private readonly IProductionToStoreRepository _repository;
    private readonly IActivityLogService          _activityLog;
    private readonly ICompanyContext              _ctx;

    public DeleteProductionToStoreCommandHandler(
        IProductionToStoreRepository repository,
        IActivityLogService          activityLog,
        ICompanyContext              ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<bool> Handle(DeleteProductionToStoreCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(request.ProductionCode, request.CompanyCode);

        if (deleted)
        {
            await _activityLog.WriteLogAsync(
                companyId: request.CompanyCode,
                source:    "ProductionToStore",
                @event:    "DELETE",
                docName:   "Production To Store",
                docNo:     string.Empty,
                docCode:   request.ProductionCode,
                userName:  _ctx.Username,
                userCode:  _ctx.UserCode,
                cancellationToken: cancellationToken);
        }

        return deleted;
    }
}
