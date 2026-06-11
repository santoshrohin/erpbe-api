using ErpBE.Application.DTOs.LabourChargeInvoice;
using ErpBE.Application.Interfaces;
using ErpBE.Application.LabourChargeInvoice.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.LabourChargeInvoice.Handlers
{
    public class CreateLabourChargeInvoiceCommandHandler
        : IRequestHandler<CreateLabourChargeInvoiceCommand, LabourChargeInvoiceMasterDto>
    {
        private readonly ILabourChargeInvoiceRepository _repository;
        private readonly ILogger<CreateLabourChargeInvoiceCommandHandler> _logger;

        public CreateLabourChargeInvoiceCommandHandler(
            ILabourChargeInvoiceRepository repository,
            ILogger<CreateLabourChargeInvoiceCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<LabourChargeInvoiceMasterDto> Handle(
            CreateLabourChargeInvoiceCommand request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Creating Labour Charge Invoice for Company: {CompanyCode}, Customer: {CustomerCode}",
                request.CompanyCode, request.CustomerCode);

            var createRequest = new CreateLabourChargeInvoiceRequest
            {
                CompanyCode = request.CompanyCode,
                InvoiceDate = request.InvoiceDate,
                CustomerCode = request.CustomerCode,
                CustomerPoCode = request.CustomerPoCode,
                InvoiceType = request.InvoiceType,
                IsSupplementary = request.IsSupplementary,
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

            return await _repository.CreateAsync(createRequest);
        }
    }
}
