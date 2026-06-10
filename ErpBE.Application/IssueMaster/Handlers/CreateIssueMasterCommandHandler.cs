using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Application.IssueMaster.Commands;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.IssueMaster.Handlers;

public class CreateIssueMasterCommandHandler : IRequestHandler<CreateIssueMasterCommand, IssueMasterDto>
{
    private readonly IIssueMasterRepository _repository;
    private readonly IActivityLogService    _activityLog;
    private readonly ICompanyContext        _ctx;

    public CreateIssueMasterCommandHandler(
        IIssueMasterRepository repository,
        IActivityLogService    activityLog,
        ICompanyContext        ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<IssueMasterDto> Handle(
        CreateIssueMasterCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new CreateIssueMasterRequest
        {
            CompanyCode    = request.CompanyCode,
            IssueDate      = request.IssueDate,
            IssueType      = request.IssueType,
            MaterialReqNo  = request.MaterialReqNo,
            IssuedBy       = request.IssuedBy,
            RequestedBy    = request.RequestedBy,
            UserMasterCode = request.UserMasterCode,
            FromStore      = request.FromStore,
            Details        = request.Details.Select(d => new CreateIssueMasterDetailRequest
            {
                ItemCode     = d.ItemCode,
                UomCode      = d.UomCode,
                CurrentStock = d.CurrentStock,
                RequestedQty = d.RequestedQty,
                IssuedQty    = d.IssuedQty,
                Remark       = d.Remark,
                Rate         = d.Rate,
                Amount       = d.Amount,
                ToStore      = d.ToStore
            }).ToList()
        };

        var result = await _repository.CreateAsync(createRequest);

        await _activityLog.WriteLogAsync(
            companyId: request.CompanyCode,
            source:    "IssueMaster",
            @event:    "INSERT",
            docName:   "Issue to Production",
            docNo:     result.IssueNumber.ToString(),
            docCode:   result.IssueCode,
            userName:  _ctx.Username,
            userCode:  _ctx.UserCode,
            cancellationToken: cancellationToken);

        return result;
    }
}
