using ErpBE.Application.CustomerMaster.Commands;
using ErpBE.Application.CustomerMaster.Interfaces;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.CustomerMaster.Handlers;

public class UpdateCustomerMasterCommandHandler : IRequestHandler<UpdateCustomerMasterCommand, Unit>
{
    private readonly ICustomerMasterRepository _repository;
    private readonly IActivityLogService       _activityLog;
    private readonly ICompanyContext           _ctx;

    public UpdateCustomerMasterCommandHandler(
        ICustomerMasterRepository repository,
        IActivityLogService       activityLog,
        ICompanyContext           ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<Unit> Handle(UpdateCustomerMasterCommand request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateCustomerMasterRequest
        {
            Id                  = request.Id,
            CompanyId           = request.CompanyId,
            PartyCode           = request.PartyCode,
            PartyName           = request.PartyName,
            ContactPerson       = request.ContactPerson,
            Abbreviation        = request.Abbreviation,
            VendorCode          = request.VendorCode,
            Address             = request.Address,
            Phone               = request.Phone,
            Mobile              = request.Mobile,
            Email               = request.Email,
            FaxNo               = request.FaxNo,
            PinCode             = request.PinCode,
            AreaCode            = request.AreaCode,
            CustomerType        = request.CustomerType,
            CountryCode         = request.CountryCode,
            StateCode           = request.StateCode,
            CityCode            = request.CityCode,
            CategoryCode        = request.CategoryCode,
            EmployeeCode        = request.EmployeeCode,
            PanNo               = request.PanNo,
            CstNo               = request.CstNo,
            VatNo               = request.VatNo,
            ServiceTaxNo        = request.ServiceTaxNo,
            EccNo               = request.EccNo,
            LbtNo               = request.LbtNo,
            ExciseRange         = request.ExciseRange,
            ExciseDivision      = request.ExciseDivision,
            ExciseCollectorate  = request.ExciseCollectorate,
            TallyName           = request.TallyName,
            CreditDays          = request.CreditDays,
            TdsPercentage       = request.TdsPercentage,
            IsActive            = request.IsActive,
            IsLbtApplicable     = request.IsLbtApplicable
        };

        await _repository.UpdateAsync(updateRequest);

        await _activityLog.WriteLogAsync(
            companyId: request.CompanyId,
            source:    "CustomerMaster",
            @event:    "UPDATE",
            docName:   "Customer Master",
            docNo:     request.Abbreviation ?? request.PartyName ?? string.Empty,
            docCode:   request.Id,
            userName:  _ctx.Username,
            userCode:  _ctx.UserCode,
            cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
