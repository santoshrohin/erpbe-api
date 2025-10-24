using ErpBE.Application.DTOs;
using ErpBE.Application.TaxInvoice.Commands;
using ErpBE.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpBE.Application.TaxInvoice.Handlers
{
    /// <summary>
    /// Handler for creating Tax Invoice - Implements ALL business logic from legacy TaxInvoice.aspx.cs
    /// </summary>
    public class CreateTaxInvoiceCommandHandler : IRequestHandler<CreateTaxInvoiceCommand, TaxInvoiceMasterDto>
    {
        private readonly ITaxInvoiceRepository _repository;
        private readonly ILogger<CreateTaxInvoiceCommandHandler> _logger;

        public CreateTaxInvoiceCommandHandler(
            ITaxInvoiceRepository repository,
            ILogger<CreateTaxInvoiceCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<TaxInvoiceMasterDto> Handle(CreateTaxInvoiceCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating Tax Invoice for Company: {CompanyCode}, Customer: {CustomerCode}", 
                request.CompanyCode, request.CustomerCode);

            try
            {
                // 1. Generate Invoice Number
                var invoiceNumber = await _repository.GenerateInvoiceNumberAsync(request.CompanyCode);
                _logger.LogInformation("Generated Invoice Number: {InvoiceNumber}", invoiceNumber);

                // 2. Map Command to DTO
                var invoiceDto = MapCommandToDto(request, invoiceNumber);

                // 3. Calculate all amounts (NET, TAXABLE, GST, GROSS)
                CalculateAllAmounts(invoiceDto);

                // 4. Create invoice in database (SP will handle stock updates)
                var createdInvoice = await _repository.CreateTaxInvoiceAsync(invoiceDto);

                _logger.LogInformation("Tax Invoice created successfully. Invoice Code: {InvoiceCode}, Invoice Number: {InvoiceNumber}", 
                    createdInvoice.InvoiceCode, createdInvoice.InvoiceNumber);

                return createdInvoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Tax Invoice for Company: {CompanyCode}, Customer: {CustomerCode}", 
                    request.CompanyCode, request.CustomerCode);
                throw;
            }
        }

        private TaxInvoiceMasterDto MapCommandToDto(CreateTaxInvoiceCommand command, int invoiceNumber)
        {
            var dto = new TaxInvoiceMasterDto
            {
                // Basic Details
                CompanyCode = command.CompanyCode,
                InvoiceNumber = invoiceNumber,
                InvoiceDate = command.InvoiceDate,
                InvoiceType = command.InvoiceType ?? 0,
                Type = command.Type ?? "TAXINV",
                PaymentMethodCode = command.PaymentMethodCode,
                CustomerCode = command.CustomerCode,
                CustomerPoCode = command.CustomerPoCode,
                DateFrom = command.DateFrom,
                DateTo = command.DateTo,
                IsSupplementary = command.IsSupplementary,
                Process = command.Process,

                // Amount Fields
                DiscountPercentage = command.DiscountPercentage,
                PackingAmount = command.PackingAmount,
                PackingDescription = command.PackingDescription,
                TaxCode = command.TaxCode,
                TcsPercentage = command.TcsPercentage,

                // Transport & Logistics
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

                // Additional Details
                CreditDays = command.CreditDays,
                AsnNumber = command.AsnNumber,
                NatureOfProduct = command.NatureOfProduct,
                PreparedBy = command.PreparedBy,
                ReworkFlag = command.ReworkFlag,
                TariffNumber = command.TariffNumber,
                TariffName = command.TariffName,

                // Additional Charges
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

                // Export Fields
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

                // Transport Details
                TransportOwner = command.TransportOwner,
                TransportAddress = command.TransportAddress,
                TNumber = command.TNumber,

                // Tray
                TrayCode = command.TrayCode,
                TrayQuantity = command.TrayQuantity,

                // Address & HSN
                Address = command.Address,
                StateCode = command.StateCode,
                AddressSelected = command.AddressSelected,
                HsnCode = command.HsnCode,
                ElectronicReferenceNumber = command.ElectronicReferenceNumber,

                // Terms & Conditions
                TermsAndConditions = command.TermsAndConditions,
                AuthorizedName = command.AuthorizedName,

                // Service Tax (Legacy)
                ServicePercentage = command.ServicePercentage,
                ServiceEducationCessPercentage = command.ServiceEducationCessPercentage,
                ServiceHigherEducationCessPercentage = command.ServiceHigherEducationCessPercentage,

                // System Fields
                IsDeleted = false,
                IsModifyLocked = false,

                // Line Items
                InvoiceDetails = command.InvoiceDetails.Select(d => new TaxInvoiceDetailDto
                {
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

        /// <summary>
        /// Calculate all amounts as per legacy application logic
        /// Formula from TaxInvoice.aspx.cs GetTotal() and CalcExise() methods
        /// </summary>
        private void CalculateAllAmounts(TaxInvoiceMasterDto invoice)
        {
            // Step 1: Calculate line item amounts and totals
            double netAmount = 0;
            double amortizationAmount = 0;
            double cgstTotal = 0;
            double sgstTotal = 0;
            double igstTotal = 0;

            foreach (var detail in invoice.InvoiceDetails)
            {
                // Line Amount = Quantity * Rate
                detail.Amount = detail.InvoiceQuantity * (detail.Rate ?? 0);
                netAmount += detail.Amount ?? 0;

                // Amortization Amount = Quantity * AmortRate (if applicable)
                if (detail.AmortRate.HasValue)
                {
                    detail.AmortAmount = detail.InvoiceQuantity * (double)detail.AmortRate.Value;
                    amortizationAmount += detail.AmortAmount ?? 0;
                }

                // Calculate GST on line item
                if (detail.CgstPercentage.HasValue && detail.CgstPercentage > 0)
                {
                    var cgst = (detail.Amount ?? 0) * (detail.CgstPercentage.Value / 100);
                    cgstTotal += cgst;
                }

                if (detail.SgstPercentage.HasValue && detail.SgstPercentage > 0)
                {
                    var sgst = (detail.Amount ?? 0) * (detail.SgstPercentage.Value / 100);
                    sgstTotal += sgst;
                }

                if (detail.IgstPercentage.HasValue && detail.IgstPercentage > 0)
                {
                    var igst = (detail.Amount ?? 0) * (detail.IgstPercentage.Value / 100);
                    igstTotal += igst;
                }
            }

            // Step 2: Set Net Amount
            invoice.NetAmount = netAmount;

            // Step 3: Set Amortization Amount (stored in StorageLocation field as per legacy)
            if (amortizationAmount > 0)
            {
                invoice.GrossAmortizationAmount = amortizationAmount;
            }

            // Step 4: Calculate Discount Amount
            double discountAmount = 0;
            if (invoice.DiscountPercentage.HasValue && invoice.DiscountPercentage > 0)
            {
                discountAmount = netAmount * (invoice.DiscountPercentage.Value / 100);
                invoice.DiscountAmount = discountAmount;
            }

            // Step 5: Calculate Packing Amount (if provided as percentage, calculate; otherwise use direct value)
            double packingAmount = invoice.PackingAmount ?? 0;

            // Step 6: Calculate Accessible Amount
            // Accessible = Net - Discount + Packing
            invoice.AccessibleAmount = netAmount - discountAmount + packingAmount;

            // Step 7: Calculate Taxable Amount (same as Accessible in most cases)
            invoice.TaxableAmount = invoice.AccessibleAmount;

            // Step 8: Set GST Amounts
            invoice.BasicExciseAmount = cgstTotal; // CGST
            invoice.EducationCessAmount = sgstTotal; // SGST
            invoice.HigherEducationCessAmount = igstTotal; // IGST

            // Step 9: Calculate Service Tax (Legacy - if applicable)
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

            // Step 10: Calculate Sales Tax Amount (from TaxCode if applicable)
            double salesTaxAmount = invoice.TaxAmount ?? 0;

            // Step 11: Add Additional Charges
            double additionalCharges = (invoice.FreightCharges ?? 0) +
                                       (invoice.TransportAmount ?? 0) +
                                       (invoice.CourierAmount ?? 0) +
                                       (invoice.InsuranceAmount ?? 0) +
                                       (invoice.OtherAmount ?? 0) +
                                       (invoice.OctriAmount ?? 0) +
                                       (invoice.AdvanceDuty ?? 0);

            // Step 12: Calculate TCS Amount
            double tcsAmount = 0;
            if (invoice.TcsPercentage.HasValue && invoice.TcsPercentage > 0)
            {
                tcsAmount = (invoice.TaxableAmount ?? 0) * (invoice.TcsPercentage.Value / 100);
                invoice.TcsAmount = tcsAmount;
            }

            // Step 13: Calculate Gross Amount
            // Gross = Taxable + GST + ServiceTax + SalesTax + AdditionalCharges + TCS
            double grossAmount = (invoice.TaxableAmount ?? 0) +
                                cgstTotal + sgstTotal + igstTotal +
                                serviceTaxAmount +
                                salesTaxAmount +
                                additionalCharges +
                                tcsAmount;

            // Step 14: Calculate Rounding
            double roundedGross = Math.Round(grossAmount);
            invoice.RoundingAmount = roundedGross - grossAmount;
            invoice.GrossAmount = roundedGross;

            _logger.LogInformation("Calculated amounts - Net: {NetAmount}, Taxable: {TaxableAmount}, CGST: {CGST}, SGST: {SGST}, IGST: {IGST}, Gross: {GrossAmount}",
                invoice.NetAmount, invoice.TaxableAmount, cgstTotal, sgstTotal, igstTotal, invoice.GrossAmount);
        }
    }
}

