using ErpBE.Application.Interfaces;
using ErpBE.Application.TaxInvoice.Commands;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.TaxInvoice.Handlers;

public class ApproveTaxInvoiceCommandHandler : IRequestHandler<ApproveTaxInvoiceCommand, bool>
{
    private readonly ITaxInvoiceRepository _repository;
    private readonly IActivityLogService   _activityLog;
    private readonly ICompanyContext       _ctx;

    public ApproveTaxInvoiceCommandHandler(
        ITaxInvoiceRepository repository,
        IActivityLogService   activityLog,
        ICompanyContext       ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<bool> Handle(ApproveTaxInvoiceCommand request, CancellationToken cancellationToken)
    {
        var approved = await _repository.ApproveAsync(request.InvoiceCode, request.CompanyCode);

        if (approved)
        {
            await _activityLog.WriteLogAsync(
                companyId: request.CompanyCode,
                source:    "TaxInvoice",
                @event:    "APPROVE",
                docName:   "Tax Invoice",
                docNo:     string.Empty,
                docCode:   request.InvoiceCode,
                userName:  _ctx.Username,
                userCode:  _ctx.UserCode,
                cancellationToken: cancellationToken);
        }

        return approved;
    }
}
