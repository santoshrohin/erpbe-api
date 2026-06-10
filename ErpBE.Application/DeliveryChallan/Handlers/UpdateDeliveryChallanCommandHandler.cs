using ErpBE.Application.DeliveryChallan.Commands;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.DeliveryChallan.Handlers;

public class UpdateDeliveryChallanCommandHandler : IRequestHandler<UpdateDeliveryChallanCommand, DeliveryChallanMasterDto>
{
    private readonly IDeliveryChallanRepository _repository;
    private readonly IActivityLogService        _activityLog;
    private readonly ICompanyContext            _ctx;

    public UpdateDeliveryChallanCommandHandler(
        IDeliveryChallanRepository repository,
        IActivityLogService        activityLog,
        ICompanyContext            ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<DeliveryChallanMasterDto> Handle(
        UpdateDeliveryChallanCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByIdAsync(request.ChallanCode, request.CompanyCode);
        if (existing?.IsModifyLocked == true)
            throw new InvalidOperationException($"Delivery Challan {request.ChallanCode} is locked and cannot be modified.");

        var updateRequest = new UpdateDeliveryChallanRequest
        {
            ChallanCode   = request.ChallanCode,
            CompanyCode   = request.CompanyCode,
            CustomerCode  = request.CustomerCode,
            Type          = request.Type,
            ChallanDate   = request.ChallanDate,
            InvoiceNumber = request.InvoiceNumber,
            Through       = request.Through,
            VehicleNumber = request.VehicleNumber,
            LrNumber      = request.LrNumber,
            OrderNumber   = request.OrderNumber,
            OrderDate     = request.OrderDate,
            MaterialType  = request.MaterialType,
            IsReturnable  = request.IsReturnable,
            Details       = request.Details.Select(d => new CreateDeliveryChallanDetailRequest
            {
                ItemCode        = d.ItemCode,
                OrderedQuantity = d.OrderedQuantity,
                BatchNumber     = d.BatchNumber,
                NumberOfPacks   = d.NumberOfPacks,
                UomCode         = d.UomCode,
                Remark          = d.Remark
            }).ToList()
        };

        var result = await _repository.UpdateAsync(updateRequest);

        await _activityLog.WriteLogAsync(
            companyId: request.CompanyCode,
            source:    "DeliveryChallan",
            @event:    "UPDATE",
            docName:   "Delivery Challan",
            docNo:     result.ChallanNumber?.ToString() ?? string.Empty,
            docCode:   request.ChallanCode,
            userName:  _ctx.Username,
            userCode:  _ctx.UserCode,
            cancellationToken: cancellationToken);

        return result;
    }
}
