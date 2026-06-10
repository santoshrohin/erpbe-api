using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Application.ProductionToStore.Commands;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.ProductionToStore.Handlers;

public class UpdateProductionToStoreCommandHandler : IRequestHandler<UpdateProductionToStoreCommand, ProductionToStoreMasterDto>
{
    private readonly IProductionToStoreRepository _repository;
    private readonly IActivityLogService          _activityLog;
    private readonly ICompanyContext              _ctx;

    public UpdateProductionToStoreCommandHandler(
        IProductionToStoreRepository repository,
        IActivityLogService          activityLog,
        ICompanyContext              ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<ProductionToStoreMasterDto> Handle(
        UpdateProductionToStoreCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateProductionToStoreRequest
        {
            ProductionCode = request.ProductionCode,
            CompanyCode    = request.CompanyCode,
            GinDate        = request.GinDate,
            Type           = request.Type,
            PersonName     = request.PersonName,
            MrCode         = request.MrCode,
            CustomerCode   = request.CustomerCode,
            BatchNo        = request.BatchNo,
            Details        = request.Details.Select(d => new CreateProductionToStoreDetailRequest
            {
                ItemCode = d.ItemCode,
                Quantity = d.Quantity,
                Remark   = d.Remark
            }).ToList()
        };

        var result = await _repository.UpdateAsync(updateRequest);

        await _activityLog.WriteLogAsync(
            companyId: request.CompanyCode,
            source:    "ProductionToStore",
            @event:    "UPDATE",
            docName:   "Production To Store",
            docNo:     result.GinNumber.ToString(),
            docCode:   request.ProductionCode,
            userName:  _ctx.Username,
            userCode:  _ctx.UserCode,
            cancellationToken: cancellationToken);

        return result;
    }
}
