using ErpBE.Application.DeliveryChallan.Commands;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.DeliveryChallan.Handlers;

public class CreateDeliveryChallanCommandHandler : IRequestHandler<CreateDeliveryChallanCommand, DeliveryChallanMasterDto>
{
    private readonly IDeliveryChallanRepository _repository;
    private readonly IActivityLogService        _activityLog;
    private readonly ICompanyContext            _ctx;

    public CreateDeliveryChallanCommandHandler(
        IDeliveryChallanRepository repository,
        IActivityLogService        activityLog,
        ICompanyContext            ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<DeliveryChallanMasterDto> Handle(
        CreateDeliveryChallanCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new CreateDeliveryChallanRequest
        {
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

        var result = await _repository.CreateAsync(createRequest);

        await _activityLog.WriteLogAsync(
            companyId: request.CompanyCode,
            source:    "DeliveryChallan",
            @event:    "INSERT",
            docName:   "Delivery Challan",
            docNo:     result.ChallanNumber?.ToString() ?? string.Empty,
            docCode:   result.ChallanCode,
            userName:  _ctx.Username,
            userCode:  _ctx.UserCode,
            cancellationToken: cancellationToken);

        return result;
    }
}
