using ErpBE.Application.DTOs.LabourChargeInvoice;
using ErpBE.Application.Interfaces;
using ErpBE.Application.LabourChargeInvoice.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.LabourChargeInvoice.Handlers
{
    public class UpdateLabourChargeInvoiceCommandHandler
        : IRequestHandler<UpdateLabourChargeInvoiceCommand, LabourChargeInvoiceMasterDto>
    {
        private readonly ILabourChargeInvoiceRepository _repository;
        private readonly ILogger<UpdateLabourChargeInvoiceCommandHandler> _logger;

        public UpdateLabourChargeInvoiceCommandHandler(
            ILabourChargeInvoiceRepository repository,
            ILogger<UpdateLabourChargeInvoiceCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<LabourChargeInvoiceMasterDto> Handle(
            UpdateLabourChargeInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Updating Labour Charge Invoice: {InvoiceCode} for Company: {CompanyCode}",
                request.InvoiceCode, request.CompanyCode);

            var updateRequest = new UpdateLabourChargeInvoiceRequest
            {
                InvoiceCode = request.InvoiceCode,
                CompanyCode = request.CompanyCode,
                InvoiceDate = request.InvoiceDate,
                CustomerCode = request.CustomerCode,
                CustomerPoCode = request.CustomerPoCode,
                InvoiceType = request.InvoiceType,
                NetAmount = request.NetAmount,
                DiscountPercentage = request.DiscountPercentage,
                ServiceTaxPercentage = request.ServiceTaxPercentage,
                TcsPercentage = request.TcsPercentage,
                PackingAmount = request.PackingAmount,
                GrossAmount = request.GrossAmount,
                TaxCode = request.TaxCode,
                VehicleNumber = request.VehicleNumber,
                TransportName = request.TransportName,
                IssueDate = request.IssueDate,
                RemovalDate = request.RemovalDate,
                Remarks = request.Remarks,
                LrNumber = request.LrNumber,
                LrDate = request.LrDate,
                TaxableAmount = request.TaxableAmount,
                RoundingAmount = request.RoundingAmount,
                OtherAmount = request.OtherAmount,
                FreightCharges = request.FreightCharges,
                InsuranceAmount = request.InsuranceAmount,
                TransportAmount = request.TransportAmount,
                OctriAmount = request.OctriAmount,
                CreditDays = request.CreditDays,
                HsnCode = request.HsnCode,
                CgstPercentage = request.CgstPercentage,
                SgstPercentage = request.SgstPercentage,
                IgstPercentage = request.IgstPercentage,
                AccessibleAmount = request.AccessibleAmount,
                DiscountAmount = request.DiscountAmount,
                IssueTime = request.IssueTime,
                RemovalTime = request.RemovalTime,
                Details = request.Details
            };

            var result = await _repository.UpdateAsync(updateRequest);
            if (result == null)
                throw new InvalidOperationException($"Labour Charge Invoice {request.InvoiceCode} not found or could not be updated.");

            return result;
        }
    }
}
