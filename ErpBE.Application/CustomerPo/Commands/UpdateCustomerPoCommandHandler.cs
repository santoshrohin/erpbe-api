using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerPo.Commands;

/// <summary>
/// Handler for updating an existing Customer PO
/// </summary>
public class UpdateCustomerPoCommandHandler : IRequestHandler<UpdateCustomerPoCommand, CustomerPoMasterDto>
{
    private readonly ICustomerPoRepository _repository;

    public UpdateCustomerPoCommandHandler(ICustomerPoRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerPoMasterDto> Handle(UpdateCustomerPoCommand request, CancellationToken cancellationToken)
    {
        // Map command to DTO
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

        // Map details
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

        // Update PO with details in transaction
        return await _repository.UpdateAsync(poMaster, poDetails);
    }
}

