using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.CustomerPo.Commands;

public class UpdateCustomerPoCommandHandler : IRequestHandler<UpdateCustomerPoCommand, CustomerPoMasterDto>
{
    private readonly ICustomerPoRepository _repository;
    private readonly IActivityLogService   _activityLog;
    private readonly ICompanyContext       _ctx;

    public UpdateCustomerPoCommandHandler(
        ICustomerPoRepository repository,
        IActivityLogService   activityLog,
        ICompanyContext       ctx)
    {
        _repository  = repository;
        _activityLog = activityLog;
        _ctx         = ctx;
    }

    public async Task<CustomerPoMasterDto> Handle(UpdateCustomerPoCommand request, CancellationToken cancellationToken)
    {
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

        // Legacy ViewCustomerPO.aspx.cs: block MODIFY if PO is referenced by an active Work Order
        if (await _repository.IsUsedInWorkOrderAsync(request.PoCode))
            throw new InvalidOperationException("This Purchase Order cannot be edited because it is referenced by an active Work Order.");

        // Legacy CustomerPO.aspx.cs: duplicate PO number check is skipped only when IsVerbal=true
        if (!request.IsVerbalOrder)
        {
            var exists = await _repository.PoNumberExistsAsync(request.PoNumber, request.CompanyId, request.PoCode);
            if (exists)
                throw new InvalidOperationException($"PO Number '{request.PoNumber}' already exists.");
        }

        var result = await _repository.UpdateAsync(poMaster, poDetails);

        await _activityLog.WriteLogAsync(
            companyId: request.CompanyId,
            source:    "CustomerPo",
            @event:    "UPDATE",
            docName:   "Customer PO",
            docNo:     result.PoNumber ?? string.Empty,
            docCode:   result.PoCode,
            userName:  _ctx.Username,
            userCode:  _ctx.UserCode,
            cancellationToken: cancellationToken);

        return result;
    }
}

