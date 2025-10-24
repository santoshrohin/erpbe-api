using ErpBE.Application.DTOs;
using ErpBE.Application.TaxInvoice.Commands;
using ErpBE.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.TaxInvoice.Handlers
{
    public class UpdateTaxInvoiceCommandHandler : IRequestHandler<UpdateTaxInvoiceCommand, TaxInvoiceMasterDto>
    {
        private readonly ITaxInvoiceRepository _repository;
        private readonly ILogger<UpdateTaxInvoiceCommandHandler> _logger;
        private readonly CreateTaxInvoiceCommandHandler _createHandler;

        public UpdateTaxInvoiceCommandHandler(
            ITaxInvoiceRepository repository,
            ILogger<UpdateTaxInvoiceCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
            _createHandler = new CreateTaxInvoiceCommandHandler(repository, 
                Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger<CreateTaxInvoiceCommandHandler>());
        }

        public async Task<TaxInvoiceMasterDto> Handle(UpdateTaxInvoiceCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating Tax Invoice: {InvoiceCode} for Company: {CompanyCode}", 
                request.InvoiceCode, request.CompanyCode);

            try
            {
                // 1. Check if invoice exists
                var existingInvoice = await _repository.GetTaxInvoiceByIdAsync(request.InvoiceCode, request.CompanyCode);
                if (existingInvoice == null)
                {
                    throw new KeyNotFoundException($"Tax Invoice with code {request.InvoiceCode} not found.");
                }

                // 2. Check if invoice is locked
                var isLocked = await _repository.IsInvoiceLockedAsync(request.InvoiceCode);
                if (isLocked)
                {
                    throw new InvalidOperationException("This invoice is currently being modified by another user. Please try again later.");
                }

                // 3. Lock the invoice
                await _repository.LockInvoiceAsync(request.InvoiceCode);

                try
                {
                    // 4. Map Update Command to DTO (keep existing invoice number)
                    var invoiceDto = MapUpdateCommandToDto(request, existingInvoice.InvoiceNumber ?? 0);

                    // 5. Recalculate all amounts
                    CalculateAllAmounts(invoiceDto);

                    // 6. Update invoice in database
                    var updatedInvoice = await _repository.UpdateTaxInvoiceAsync(invoiceDto);

                    _logger.LogInformation("Tax Invoice updated successfully. Invoice Code: {InvoiceCode}", updatedInvoice.InvoiceCode);

                    return updatedInvoice;
                }
                finally
                {
                    // 7. Always unlock the invoice
                    await _repository.UnlockInvoiceAsync(request.InvoiceCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Tax Invoice: {InvoiceCode}", request.InvoiceCode);
                throw;
            }
        }

        private TaxInvoiceMasterDto MapUpdateCommandToDto(UpdateTaxInvoiceCommand command, int invoiceNumber)
        {
            // Same mapping logic as Create, but with Invoice Code
            var dto = new TaxInvoiceMasterDto
            {
                InvoiceCode = command.InvoiceCode,
                CompanyCode = command.CompanyCode,
                InvoiceNumber = invoiceNumber,
                InvoiceDate = command.InvoiceDate,
                InvoiceType = command.InvoiceType,
                Type = command.Type,
                PaymentMethodCode = command.PaymentMethodCode,
                CustomerCode = command.CustomerCode,
                CustomerPoCode = command.CustomerPoCode,
                DateFrom = command.DateFrom,
                DateTo = command.DateTo,
                IsSupplementary = command.IsSupplementary,
                Process = command.Process,
                DiscountPercentage = command.DiscountPercentage,
                PackingAmount = command.PackingAmount,
                PackingDescription = command.PackingDescription,
                TaxCode = command.TaxCode,
                TcsPercentage = command.TcsPercentage,
                FreightCharges = command.FreightCharges,
                VehicleNumber = command.VehicleNumber,
                TransportName = command.TransportName,
                IssueDate = command.IssueDate,
                RemovalDate = command.RemovalDate,
                IssueTime = command.IssueTime,
                RemovalTime = command.RemovalTime,
                StorageLocation = command.StorageLocation,
                Remarks = command.Remarks,
                LrNumber = command.LrNumber,
                LrDate = command.LrDate,
                CreditDays = command.CreditDays,
                AsnNumber = command.AsnNumber,
                NatureOfProduct = command.NatureOfProduct,
                PreparedBy = command.PreparedBy,
                ReworkFlag = command.ReworkFlag,
                TariffNumber = command.TariffNumber,
                TariffName = command.TariffName,
                TransportAmount = command.TransportAmount,
                CourierAmount = command.CourierAmount,
                DeliveryAddress = command.DeliveryAddress,
                AlternateCustomerCode = command.AlternateCustomerCode,
                OtherAmount = command.OtherAmount,
                BuyerName = command.BuyerName,
                BuyerAddress = command.BuyerAddress,
                InsuranceAmount = command.InsuranceAmount,
                AdvanceDuty = command.AdvanceDuty,
                OctriAmount = command.OctriAmount,
                ExportFlag = command.ExportFlag,
                FinalDestination = command.FinalDestination,
                PreCarriage = command.PreCarriage,
                PortOfLoading = command.PortOfLoading,
                PortOfDischarge = command.PortOfDischarge,
                PlaceOfDelivery = command.PlaceOfDelivery,
                CurrencyCode = command.CurrencyCode,
                CurrencyRate = command.CurrencyRate,
                Clearance = command.Clearance,
                FlightNumber = command.FlightNumber,
                ManufacturingDate = command.ManufacturingDate,
                ExpiryDate = command.ExpiryDate,
                AuthorizedSignatory = command.AuthorizedSignatory,
                AreaFormNumber = command.AreaFormNumber,
                FormDate = command.FormDate,
                Shipment = command.Shipment,
                CenvatAccountNumber = command.CenvatAccountNumber,
                BondNumber = command.BondNumber,
                BondDate = command.BondDate,
                Ut1FileNumber = command.Ut1FileNumber,
                FileNumber = command.FileNumber,
                ValidityDate = command.ValidityDate,
                ExaminationBoxes = command.ExaminationBoxes,
                TermsOfDelivery = command.TermsOfDelivery,
                TermsOfPayment = command.TermsOfPayment,
                VoyageNumber = command.VoyageNumber,
                PlaceOfReceipt = command.PlaceOfReceipt,
                MarksAndNumbers = command.MarksAndNumbers,
                NumberOfPackages = command.NumberOfPackages,
                UnNumber = command.UnNumber,
                HazardClass = command.HazardClass,
                HsCodeNumber = command.HsCodeNumber,
                ContainerNumber = command.ContainerNumber,
                SealNumber = command.SealNumber,
                OtsNumber = command.OtsNumber,
                CountryOfOrigin = command.CountryOfOrigin,
                CountryOfDestination = command.CountryOfDestination,
                TransportBy = command.TransportBy,
                AreaRemarks = command.AreaRemarks,
                CarrierName = command.CarrierName,
                CarrierBookingNumber = command.CarrierBookingNumber,
                ShipName = command.ShipName,
                TechnicalName = command.TechnicalName,
                OuterPackaging = command.OuterPackaging,
                InnerPackaging = command.InnerPackaging,
                SubsidiaryClass = command.SubsidiaryClass,
                UnPackingGroup = command.UnPackingGroup,
                UnPackingCode = command.UnPackingCode,
                EmsNumber = command.EmsNumber,
                FlashPoint = command.FlashPoint,
                MarinePollutant = command.MarinePollutant,
                ShipperDeclaration = command.ShipperDeclaration,
                IecNumber = command.IecNumber,
                IecDate = command.IecDate,
                CentralExciseRegistration = command.CentralExciseRegistration,
                DateOfExamination = command.DateOfExamination,
                SuperintendentExciseName = command.SuperintendentExciseName,
                InspectorExciseName = command.InspectorExciseName,
                CustomsSealNumber = command.CustomsSealNumber,
                PermitENumber = command.PermitENumber,
                NonCargoNumberOfPackages = command.NonCargoNumberOfPackages,
                ShippingBillNumber = command.ShippingBillNumber,
                LcNumber = command.LcNumber,
                LcDate = command.LcDate,
                TransportOwner = command.TransportOwner,
                TransportAddress = command.TransportAddress,
                TNumber = command.TNumber,
                TrayCode = command.TrayCode,
                TrayQuantity = command.TrayQuantity,
                Address = command.Address,
                StateCode = command.StateCode,
                AddressSelected = command.AddressSelected,
                HsnCode = command.HsnCode,
                ElectronicReferenceNumber = command.ElectronicReferenceNumber,
                TermsAndConditions = command.TermsAndConditions,
                AuthorizedName = command.AuthorizedName,
                ServicePercentage = command.ServicePercentage,
                ServiceEducationCessPercentage = command.ServiceEducationCessPercentage,
                ServiceHigherEducationCessPercentage = command.ServiceHigherEducationCessPercentage,
                IsDeleted = false,
                IsModifyLocked = false,

                InvoiceDetails = command.InvoiceDetails.Select(d => new TaxInvoiceDetailDto
                {
                    InvoiceMasterCode = command.InvoiceCode,
                    ItemCode = d.ItemCode,
                    UomCode = d.UomCode,
                    InvoiceQuantity = d.InvoiceQuantity,
                    Rate = d.Rate,
                    CustomerPoCode = d.CustomerPoCode,
                    ConversionQuantity = d.ConversionQuantity,
                    AmortizationRate = d.AmortizationRate,
                    NumberOfPackages = d.NumberOfPackages,
                    PackageDescription = d.PackageDescription,
                    QuantityPerPack = d.QuantityPerPack,
                    DeliveryChallanNumbers = d.DeliveryChallanNumbers,
                    DeliveryChallanDates = d.DeliveryChallanDates,
                    ExciseNumbers = d.ExciseNumbers,
                    ProcessCode = d.ProcessCode,
                    GinNumber = d.GinNumber,
                    GinDate = d.GinDate,
                    GinReceipt = d.GinReceipt,
                    MrCode = d.MrCode,
                    GinAcceptance = d.GinAcceptance,
                    CgstPercentage = d.CgstPercentage,
                    SgstPercentage = d.SgstPercentage,
                    IgstPercentage = d.IgstPercentage,
                    SerialNumber = d.SerialNumber,
                    Remarks = d.Remarks,
                    ItemWarehouseCode = d.ItemWarehouseCode,
                    ActualWeight = d.ActualWeight,
                    Size = d.Size,
                    SubHeading = d.SubHeading,
                    BatchNumber = d.BatchNumber,
                    PackingQuantity = d.PackingQuantity,
                    GrossWeight = d.GrossWeight,
                    NetWeight = d.NetWeight,
                    SizeOfBox = d.SizeOfBox,
                    NumberOfBarrels = d.NumberOfBarrels,
                    NumberOfPackagesDescription = d.NumberOfPackagesDescription,
                    ContainerNumber = d.ContainerNumber,
                    RefundableQuantity = d.RefundableQuantity,
                    AmortRate = d.AmortRate,
                    HsnCode = d.HsnCode,
                    StoreCode = d.StoreCode,
                    IsDeleted = false
                }).ToList()
            };

            return dto;
        }

        // Same calculation logic as Create
        private void CalculateAllAmounts(TaxInvoiceMasterDto invoice)
        {
            double netAmount = 0;
            double amortizationAmount = 0;
            double cgstTotal = 0;
            double sgstTotal = 0;
            double igstTotal = 0;

            foreach (var detail in invoice.InvoiceDetails)
            {
                detail.Amount = detail.InvoiceQuantity * (detail.Rate ?? 0);
                netAmount += detail.Amount ?? 0;

                if (detail.AmortRate.HasValue)
                {
                    detail.AmortAmount = detail.InvoiceQuantity * (double)detail.AmortRate.Value;
                    amortizationAmount += detail.AmortAmount ?? 0;
                }

                if (detail.CgstPercentage.HasValue && detail.CgstPercentage > 0)
                {
                    cgstTotal += (detail.Amount ?? 0) * (detail.CgstPercentage.Value / 100);
                }

                if (detail.SgstPercentage.HasValue && detail.SgstPercentage > 0)
                {
                    sgstTotal += (detail.Amount ?? 0) * (detail.SgstPercentage.Value / 100);
                }

                if (detail.IgstPercentage.HasValue && detail.IgstPercentage > 0)
                {
                    igstTotal += (detail.Amount ?? 0) * (detail.IgstPercentage.Value / 100);
                }
            }

            invoice.NetAmount = netAmount;
            if (amortizationAmount > 0) invoice.GrossAmortizationAmount = amortizationAmount;

            double discountAmount = 0;
            if (invoice.DiscountPercentage.HasValue && invoice.DiscountPercentage > 0)
            {
                discountAmount = netAmount * (invoice.DiscountPercentage.Value / 100);
                invoice.DiscountAmount = discountAmount;
            }

            double packingAmount = invoice.PackingAmount ?? 0;
            invoice.AccessibleAmount = netAmount - discountAmount + packingAmount;
            invoice.TaxableAmount = invoice.AccessibleAmount;

            invoice.BasicExciseAmount = cgstTotal;
            invoice.EducationCessAmount = sgstTotal;
            invoice.HigherEducationCessAmount = igstTotal;

            double serviceTaxAmount = 0;
            if (invoice.ServicePercentage.HasValue && invoice.ServicePercentage > 0)
            {
                invoice.ServiceAmount = invoice.TaxableAmount * (invoice.ServicePercentage.Value / 100);
                serviceTaxAmount += invoice.ServiceAmount ?? 0;

                if (invoice.ServiceEducationCessPercentage.HasValue)
                {
                    invoice.ServiceEducationCessAmount = (invoice.ServiceAmount ?? 0) * 
                        (invoice.ServiceEducationCessPercentage.Value / 100);
                    serviceTaxAmount += invoice.ServiceEducationCessAmount ?? 0;
                }

                if (invoice.ServiceHigherEducationCessPercentage.HasValue)
                {
                    invoice.ServiceHigherEducationCessAmount = (invoice.ServiceAmount ?? 0) * 
                        (invoice.ServiceHigherEducationCessPercentage.Value / 100);
                    serviceTaxAmount += invoice.ServiceHigherEducationCessAmount ?? 0;
                }
            }

            invoice.ServiceTaxAmount = serviceTaxAmount;

            double salesTaxAmount = invoice.TaxAmount ?? 0;

            double additionalCharges = (invoice.FreightCharges ?? 0) +
                                       (invoice.TransportAmount ?? 0) +
                                       (invoice.CourierAmount ?? 0) +
                                       (invoice.InsuranceAmount ?? 0) +
                                       (invoice.OtherAmount ?? 0) +
                                       (invoice.OctriAmount ?? 0) +
                                       (invoice.AdvanceDuty ?? 0);

            double tcsAmount = 0;
            if (invoice.TcsPercentage.HasValue && invoice.TcsPercentage > 0)
            {
                tcsAmount = (invoice.TaxableAmount ?? 0) * (invoice.TcsPercentage.Value / 100);
                invoice.TcsAmount = tcsAmount;
            }

            double grossAmount = (invoice.TaxableAmount ?? 0) +
                                cgstTotal + sgstTotal + igstTotal +
                                serviceTaxAmount +
                                salesTaxAmount +
                                additionalCharges +
                                tcsAmount;

            double roundedGross = Math.Round(grossAmount);
            invoice.RoundingAmount = roundedGross - grossAmount;
            invoice.GrossAmount = roundedGross;
        }
    }
}

