using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.CustomerPo.Commands;

public class AmendCustomerPoCommandHandler : IRequestHandler<AmendCustomerPoCommand, CustomerPoMasterDto>
{
    private readonly ICustomerPoRepository _repository;
    private readonly IActivityLogService   _activityLog;
    private readonly ICompanyContext       _ctx;

    public AmendCustomerPoCommandHandler(
        ICustomerPoRepository repository,
        IActivityLogService   activityLog,
        ICompanyContext       ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<CustomerPoMasterDto> Handle(AmendCustomerPoCommand request, CancellationToken cancellationToken)
    {
        if (request.Details.Count == 0)
            throw new InvalidOperationException("At least one detail line is required.");

        var poMaster = new CustomerPoMasterDto
        {
            PoCode = request.PoCode,
            CustomerCode = request.CustomerCode,
            PoNumber = request.PoNumber,
            PoType = request.PoType,
            PoDate = request.PoDate,
            CreditDays = request.CreditDays,
            CompanyId = request.CompanyId,
            WorkOrderNumber = request.WorkOrderNumber,
            PaymentTerms = request.PaymentTerms,
            IsAuthorized = request.IsAuthorized,
            CustomerPoDate = request.CustomerPoDate,
            QuotationCode = request.QuotationCode,
            TaxName = request.TaxName,
            TaxPercentage = request.TaxPercentage,
            TaxAmount = request.TaxAmount,
            ExcisePercentage = request.ExcisePercentage,
            ExciseEducationPercentage = request.ExciseEducationPercentage,
            ExciseHigherEducationPercentage = request.ExciseHigherEducationPercentage,
            BasicAmount = request.BasicAmount,
            DiscountPercentage = request.DiscountPercentage,
            DiscountAmount = request.DiscountAmount,
            DiscountReason = request.DiscountReason,
            DeviationAmount = request.DeviationAmount,
            DeviationReason = request.DeviationReason,
            PackingAmount = request.PackingAmount,
            ExciseAmount = request.ExciseAmount,
            RoundingAmount = request.RoundingAmount,
            GrandTotal = request.GrandTotal,
            FinalDestination = request.FinalDestination,
            PreCarriageBy = request.PreCarriageBy,
            PortOfLoading = request.PortOfLoading,
            PortOfDischarge = request.PortOfDischarge,
            PlaceOfDelivery = request.PlaceOfDelivery,
            BuyerName = request.BuyerName,
            BuyerAddress = request.BuyerAddress,
            CurrencyCode = request.CurrencyCode,
            InquiryCode = request.InquiryCode,
            IsVerbalOrder = request.IsVerbalOrder,
            ProjectCode = request.ProjectCode,
            ProjectName = request.ProjectName
        };

        var poDetails = request.Details.Select(d => new CustomerPoDetailDto
        {
            ItemCode = d.ItemCode,
            UomCode = d.UomCode,
            OrderedQuantity = d.OrderedQuantity,
            Rate = d.Rate,
            Amount = d.Amount,
            Description = d.Description,
            CustomerItemCode = d.CustomerItemCode,
            CustomerItemName = d.CustomerItemName,
            Status = d.Status,
            DispatchedQuantity = d.DispatchedQuantity,
            IsOrder = d.IsOrder,
            StoreCode = d.StoreCode,
            CurrencyCode = d.CurrencyCode,
            WorkOrderQuantity = d.WorkOrderQuantity,
            ModificationNumber = d.ModificationNumber,
            ModificationDate = d.ModificationDate,
            AmortizationRate = d.AmortizationRate,
            DieAmortizationRate = d.DieAmortizationRate,
            DiscountPercentage = d.DiscountPercentage,
            DiscountAmount = d.DiscountAmount
        }).ToList();

        await _repository.AmendAsync(poMaster, poDetails);

        var result = await _repository.GetByIdAsync(request.PoCode, request.CompanyId)
            ?? throw new InvalidOperationException($"Customer PO {request.PoCode} not found after amend.");

        await _activityLog.WriteLogAsync(
            companyId: request.CompanyId,
            source:    "CustomerPo",
            @event:    "AMEND",
            docName:   "Customer PO",
            docNo:     result.PoNumber ?? string.Empty,
            docCode:   result.PoCode,
            userName:  _ctx.Username,
            userCode:  _ctx.UserCode,
            cancellationToken: cancellationToken);

        return result;
    }
}
