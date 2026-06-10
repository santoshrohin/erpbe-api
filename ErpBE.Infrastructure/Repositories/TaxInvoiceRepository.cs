using Dapper;
using ErpBE.Application.DTOs;
using ErpBE.Application.DTOs.TaxInvoice;
using ErpBE.Application.TaxInvoice.Queries;
using ErpBE.Application.Interfaces;
using ErpBE.Infrastructure.Services;
using ErpBE.Infrastructure.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ErpBE.Infrastructure.Repositories
{
    public class TaxInvoiceRepository : ITaxInvoiceRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<TaxInvoiceRepository> _logger;

        public TaxInvoiceRepository(IConfiguration configuration, ILogger<TaxInvoiceRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<TaxInvoiceMasterDto> CreateTaxInvoiceAsync(TaxInvoiceMasterDto invoice)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Insert Invoice Master
                var masterParams = new DynamicParameters();
                masterParams.Add("@CompanyCode", invoice.CompanyCode);
                masterParams.Add("@InvoiceNumber", invoice.InvoiceNumber);
                masterParams.Add("@InvoiceDate", invoice.InvoiceDate);
                masterParams.Add("@InvoiceType", invoice.InvoiceType);
                masterParams.Add("@Type", invoice.Type);
                masterParams.Add("@PaymentMethodCode", invoice.PaymentMethodCode);
                masterParams.Add("@CustomerCode", invoice.CustomerCode);
                masterParams.Add("@CustomerPoCode", invoice.CustomerPoCode);
                masterParams.Add("@DateFrom", invoice.DateFrom);
                masterParams.Add("@DateTo", invoice.DateTo);
                masterParams.Add("@IsSupplementary", invoice.IsSupplementary);
                masterParams.Add("@Process", invoice.Process);
                masterParams.Add("@NetAmount", invoice.NetAmount);
                masterParams.Add("@ServiceTaxPercentage", invoice.ServiceTaxPercentage);
                masterParams.Add("@ServiceTaxAmount", invoice.ServiceTaxAmount);
                masterParams.Add("@BasicExcisePercentage", invoice.BasicExcisePercentage);
                masterParams.Add("@BasicExciseAmount", invoice.BasicExciseAmount);
                masterParams.Add("@EducationCessPercentage", invoice.EducationCessPercentage);
                masterParams.Add("@EducationCessAmount", invoice.EducationCessAmount);
                masterParams.Add("@HigherEducationCessPercentage", invoice.HigherEducationCessPercentage);
                masterParams.Add("@HigherEducationCessAmount", invoice.HigherEducationCessAmount);
                masterParams.Add("@DiscountPercentage", invoice.DiscountPercentage);
                masterParams.Add("@DiscountAmount", invoice.DiscountAmount);
                masterParams.Add("@PackingAmount", invoice.PackingAmount);
                masterParams.Add("@PackingDescription", invoice.PackingDescription);
                masterParams.Add("@TaxCode", invoice.TaxCode);
                masterParams.Add("@TaxAmount", invoice.TaxAmount);
                masterParams.Add("@GrossAmount", invoice.GrossAmount);
                masterParams.Add("@GrossAmortizationAmount", invoice.GrossAmortizationAmount);
                masterParams.Add("@TcsPercentage", invoice.TcsPercentage);
                masterParams.Add("@TcsAmount", invoice.TcsAmount);
                masterParams.Add("@FreightCharges", invoice.FreightCharges);
                masterParams.Add("@VehicleNumber", invoice.VehicleNumber);
                masterParams.Add("@TransportName", invoice.TransportName);
                masterParams.Add("@IssueDate", invoice.IssueDate);
                masterParams.Add("@RemovalDate", invoice.RemovalDate);
                masterParams.Add("@StorageLocation", invoice.StorageLocation);
                masterParams.Add("@Remarks", invoice.Remarks);
                masterParams.Add("@CreditDays", invoice.CreditDays);
                masterParams.Add("@AsnNumber", invoice.AsnNumber);
                masterParams.Add("@NatureOfProduct", invoice.NatureOfProduct);
                masterParams.Add("@PreparedBy", invoice.PreparedBy);
                masterParams.Add("@ReworkFlag", invoice.ReworkFlag);
                masterParams.Add("@TallyTransferFlag", invoice.TallyTransferFlag);
                masterParams.Add("@TempTallyTransferFlag", invoice.TempTallyTransferFlag);
                masterParams.Add("@TariffNumber", invoice.TariffNumber);
                masterParams.Add("@TariffName", invoice.TariffName);
                masterParams.Add("@TallyTransferFlag1", invoice.TallyTransferFlag1);
                masterParams.Add("@TempTallyTransferFlag1", invoice.TempTallyTransferFlag1);
                masterParams.Add("@TransportAmount", invoice.TransportAmount);
                masterParams.Add("@CourierAmount", invoice.CourierAmount);
                masterParams.Add("@ServicePercentage", invoice.ServicePercentage);
                masterParams.Add("@ServiceAmount", invoice.ServiceAmount);
                masterParams.Add("@ServiceEducationCessPercentage", invoice.ServiceEducationCessPercentage);
                masterParams.Add("@ServiceEducationCessAmount", invoice.ServiceEducationCessAmount);
                masterParams.Add("@ServiceHigherEducationCessPercentage", invoice.ServiceHigherEducationCessPercentage);
                masterParams.Add("@ServiceHigherEducationCessAmount", invoice.ServiceHigherEducationCessAmount);
                masterParams.Add("@DeliveryAddress", invoice.DeliveryAddress);
                masterParams.Add("@AlternateCustomerCode", invoice.AlternateCustomerCode);
                masterParams.Add("@OtherAmount", invoice.OtherAmount);
                masterParams.Add("@LrNumber", invoice.LrNumber);
                masterParams.Add("@LrDate", invoice.LrDate);
                masterParams.Add("@BuyerName", invoice.BuyerName);
                masterParams.Add("@BuyerAddress", invoice.BuyerAddress);
                masterParams.Add("@InsuranceAmount", invoice.InsuranceAmount);
                masterParams.Add("@FinalDestination", invoice.FinalDestination);
                masterParams.Add("@PreCarriage", invoice.PreCarriage);
                masterParams.Add("@PortOfLoading", invoice.PortOfLoading);
                masterParams.Add("@PortOfDischarge", invoice.PortOfDischarge);
                masterParams.Add("@PlaceOfDelivery", invoice.PlaceOfDelivery);
                masterParams.Add("@CurrencyCode", invoice.CurrencyCode);
                masterParams.Add("@Clearance", invoice.Clearance);
                masterParams.Add("@FlightNumber", invoice.FlightNumber);
                masterParams.Add("@ManufacturingDate", invoice.ManufacturingDate);
                masterParams.Add("@ExpiryDate", invoice.ExpiryDate);
                masterParams.Add("@CurrencyRate", invoice.CurrencyRate);
                masterParams.Add("@AuthorizedSignatory", invoice.AuthorizedSignatory);
                masterParams.Add("@AreaFormNumber", invoice.AreaFormNumber);
                masterParams.Add("@FormDate", invoice.FormDate);
                masterParams.Add("@Shipment", invoice.Shipment);
                masterParams.Add("@CenvatAccountNumber", invoice.CenvatAccountNumber);
                masterParams.Add("@BondNumber", invoice.BondNumber);
                masterParams.Add("@BondDate", invoice.BondDate);
                masterParams.Add("@Ut1FileNumber", invoice.Ut1FileNumber);
                masterParams.Add("@FileNumber", invoice.FileNumber);
                masterParams.Add("@ExportFlag", invoice.ExportFlag);
                masterParams.Add("@ValidityDate", invoice.ValidityDate);
                masterParams.Add("@ExaminationBoxes", invoice.ExaminationBoxes);
                masterParams.Add("@TermsOfDelivery", invoice.TermsOfDelivery);
                masterParams.Add("@TermsOfPayment", invoice.TermsOfPayment);
                masterParams.Add("@VoyageNumber", invoice.VoyageNumber);
                masterParams.Add("@PlaceOfReceipt", invoice.PlaceOfReceipt);
                masterParams.Add("@MarksAndNumbers", invoice.MarksAndNumbers);
                masterParams.Add("@NumberOfPackages", invoice.NumberOfPackages);
                masterParams.Add("@UnNumber", invoice.UnNumber);
                masterParams.Add("@HazardClass", invoice.HazardClass);
                masterParams.Add("@HsCodeNumber", invoice.HsCodeNumber);
                masterParams.Add("@ContainerNumber", invoice.ContainerNumber);
                masterParams.Add("@SealNumber", invoice.SealNumber);
                masterParams.Add("@OtsNumber", invoice.OtsNumber);
                masterParams.Add("@CountryOfOrigin", invoice.CountryOfOrigin);
                masterParams.Add("@CountryOfDestination", invoice.CountryOfDestination);
                masterParams.Add("@TransportBy", invoice.TransportBy);
                masterParams.Add("@AreaRemarks", invoice.AreaRemarks);
                masterParams.Add("@CarrierName", invoice.CarrierName);
                masterParams.Add("@CarrierBookingNumber", invoice.CarrierBookingNumber);
                masterParams.Add("@ShipName", invoice.ShipName);
                masterParams.Add("@TechnicalName", invoice.TechnicalName);
                masterParams.Add("@OuterPackaging", invoice.OuterPackaging);
                masterParams.Add("@InnerPackaging", invoice.InnerPackaging);
                masterParams.Add("@SubsidiaryClass", invoice.SubsidiaryClass);
                masterParams.Add("@UnPackingGroup", invoice.UnPackingGroup);
                masterParams.Add("@UnPackingCode", invoice.UnPackingCode);
                masterParams.Add("@EmsNumber", invoice.EmsNumber);
                masterParams.Add("@FlashPoint", invoice.FlashPoint);
                masterParams.Add("@MarinePollutant", invoice.MarinePollutant);
                masterParams.Add("@ShipperDeclaration", invoice.ShipperDeclaration);
                masterParams.Add("@IecNumber", invoice.IecNumber);
                masterParams.Add("@IecDate", invoice.IecDate);
                masterParams.Add("@CentralExciseRegistration", invoice.CentralExciseRegistration);
                masterParams.Add("@DateOfExamination", invoice.DateOfExamination);
                masterParams.Add("@SuperintendentExciseName", invoice.SuperintendentExciseName);
                masterParams.Add("@InspectorExciseName", invoice.InspectorExciseName);
                masterParams.Add("@CustomsSealNumber", invoice.CustomsSealNumber);
                masterParams.Add("@PermitENumber", invoice.PermitENumber);
                masterParams.Add("@IsTallyTransferred", invoice.IsTallyTransferred);
                masterParams.Add("@NonCargoNumberOfPackages", invoice.NonCargoNumberOfPackages);
                masterParams.Add("@ShippingBillNumber", invoice.ShippingBillNumber);
                masterParams.Add("@AccessibleAmount", invoice.AccessibleAmount);
                masterParams.Add("@TaxableAmount", invoice.TaxableAmount);
                masterParams.Add("@RoundingAmount", invoice.RoundingAmount);
                masterParams.Add("@AdvanceDuty", invoice.AdvanceDuty);
                masterParams.Add("@OctriAmount", invoice.OctriAmount);
                masterParams.Add("@IsSuppliment", invoice.IsSuppliment);
                masterParams.Add("@IssueTime", invoice.IssueTime);
                masterParams.Add("@RemovalTime", invoice.RemovalTime);
                masterParams.Add("@LcNumber", invoice.LcNumber);
                masterParams.Add("@LcDate", invoice.LcDate);
                masterParams.Add("@TransportOwner", invoice.TransportOwner);
                masterParams.Add("@TransportAddress", invoice.TransportAddress);
                masterParams.Add("@TNumber", invoice.TNumber);
                masterParams.Add("@TrayCode", invoice.TrayCode);
                masterParams.Add("@TrayQuantity", invoice.TrayQuantity);
                masterParams.Add("@Address", invoice.Address);
                masterParams.Add("@StateCode", invoice.StateCode);
                masterParams.Add("@HsnCode", invoice.HsnCode);
                masterParams.Add("@ElectronicReferenceNumber", invoice.ElectronicReferenceNumber);
                masterParams.Add("@TermsAndConditions", invoice.TermsAndConditions);
                masterParams.Add("@AuthorizedName", invoice.AuthorizedName);
                masterParams.Add("@AddressSelected", invoice.AddressSelected);
                masterParams.Add("@ParentInvoiceCode", invoice.ParentInvoiceCode);
                masterParams.Add("@NewInvoiceCode", dbType: DbType.Int64, direction: ParameterDirection.Output);

                await connection.ExecuteAsync("ERP_CreateTaxInvoice", masterParams,
                    transaction: transaction, commandType: CommandType.StoredProcedure);

                var newInvoiceCode = masterParams.Get<long>("@NewInvoiceCode");
                invoice.InvoiceCode = newInvoiceCode;

                // 2. Insert Invoice Details
                foreach (var detail in invoice.InvoiceDetails)
                {
                    var detailParams = new DynamicParameters();
                    detailParams.Add("@InvoiceMasterCode", newInvoiceCode);
                    detailParams.Add("@ItemCode", detail.ItemCode);
                    detailParams.Add("@UomCode", detail.UomCode);
                    detailParams.Add("@CustomerPoCode", detail.CustomerPoCode);
                    detailParams.Add("@InvoiceQuantity", detail.InvoiceQuantity);
                    detailParams.Add("@Rate", detail.Rate);
                    detailParams.Add("@ConversionQuantity", detail.ConversionQuantity);
                    detailParams.Add("@AmortizationRate", detail.AmortizationRate);
                    detailParams.Add("@NumberOfPackages", detail.NumberOfPackages);
                    detailParams.Add("@PackageDescription", detail.PackageDescription);
                    detailParams.Add("@QuantityPerPack", detail.QuantityPerPack);
                    detailParams.Add("@Amount", detail.Amount);
                    detailParams.Add("@DeliveryChallanNumbers", detail.DeliveryChallanNumbers);
                    detailParams.Add("@DeliveryChallanDates", detail.DeliveryChallanDates);
                    detailParams.Add("@ExciseNumbers", detail.ExciseNumbers);
                    detailParams.Add("@ProcessCode", detail.ProcessCode);
                    detailParams.Add("@GinNumber", detail.GinNumber);
                    detailParams.Add("@GinDate", detail.GinDate);
                    detailParams.Add("@GinReceipt", detail.GinReceipt);
                    detailParams.Add("@MrCode", detail.MrCode);
                    detailParams.Add("@GinAcceptance", detail.GinAcceptance);
                    detailParams.Add("@ExciseAmount", detail.ExciseAmount);
                    detailParams.Add("@EducationCessAmount", detail.EducationCessAmount);
                    detailParams.Add("@SecondaryHigherEducationCessAmount", detail.SecondaryHigherEducationCessAmount);
                    detailParams.Add("@CgstPercentage", detail.CgstPercentage);
                    detailParams.Add("@SgstPercentage", detail.SgstPercentage);
                    detailParams.Add("@IgstPercentage", detail.IgstPercentage);
                    detailParams.Add("@SerialNumber", detail.SerialNumber);
                    detailParams.Add("@Remarks", detail.Remarks);
                    detailParams.Add("@ItemWarehouseCode", detail.ItemWarehouseCode);
                    detailParams.Add("@ActualWeight", detail.ActualWeight);
                    detailParams.Add("@Size", detail.Size);
                    detailParams.Add("@SubHeading", detail.SubHeading);
                    detailParams.Add("@BatchNumber", detail.BatchNumber);
                    detailParams.Add("@PackingQuantity", detail.PackingQuantity);
                    detailParams.Add("@GrossWeight", detail.GrossWeight);
                    detailParams.Add("@NetWeight", detail.NetWeight);
                    detailParams.Add("@SizeOfBox", detail.SizeOfBox);
                    detailParams.Add("@NumberOfBarrels", detail.NumberOfBarrels);
                    detailParams.Add("@NumberOfPackagesDescription", detail.NumberOfPackagesDescription);
                    detailParams.Add("@ContainerNumber", detail.ContainerNumber);
                    detailParams.Add("@RefundableQuantity", detail.RefundableQuantity);
                    detailParams.Add("@AmortRate", detail.AmortRate);
                    detailParams.Add("@AmortAmount", detail.AmortAmount);
                    detailParams.Add("@HsnCode", detail.HsnCode);
                    detailParams.Add("@StoreCode", detail.StoreCode);

                    await connection.ExecuteAsync("ERP_CreateTaxInvoiceDetail", detailParams, 
                        transaction: transaction, commandType: CommandType.StoredProcedure);
                }

                transaction.Commit();
                _logger.LogInformation("Tax Invoice created successfully. Invoice Code: {InvoiceCode}", newInvoiceCode);

                return await GetTaxInvoiceByIdAsync(newInvoiceCode, invoice.CompanyCode ?? 0);
            }
            catch (Exception ex)
            {
                try { transaction.Rollback(); } catch { /* transaction already completed by SQL Server */ }
                _logger.LogError(ex, "Error creating Tax Invoice");
                throw;
            }
        }

        public async Task<TaxInvoiceMasterDto> UpdateTaxInvoiceAsync(TaxInvoiceMasterDto invoice)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Update Invoice Master (same parameters as Create)
                var masterParams = new DynamicParameters();
                masterParams.Add("@InvoiceCode", invoice.InvoiceCode);
                masterParams.Add("@CompanyCode", invoice.CompanyCode);
                masterParams.Add("@InvoiceDate", invoice.InvoiceDate);
                masterParams.Add("@InvoiceType", invoice.InvoiceType);
                masterParams.Add("@Type", invoice.Type);
                masterParams.Add("@PaymentMethodCode", invoice.PaymentMethodCode);
                masterParams.Add("@CustomerCode", invoice.CustomerCode);
                masterParams.Add("@CustomerPoCode", invoice.CustomerPoCode);
                masterParams.Add("@DateFrom", invoice.DateFrom);
                masterParams.Add("@DateTo", invoice.DateTo);
                masterParams.Add("@IsSupplementary", invoice.IsSupplementary);
                masterParams.Add("@Process", invoice.Process);
                masterParams.Add("@NetAmount", invoice.NetAmount);
                masterParams.Add("@ServiceTaxPercentage", invoice.ServiceTaxPercentage);
                masterParams.Add("@ServiceTaxAmount", invoice.ServiceTaxAmount);
                masterParams.Add("@BasicExcisePercentage", invoice.BasicExcisePercentage);
                masterParams.Add("@BasicExciseAmount", invoice.BasicExciseAmount);
                masterParams.Add("@EducationCessPercentage", invoice.EducationCessPercentage);
                masterParams.Add("@EducationCessAmount", invoice.EducationCessAmount);
                masterParams.Add("@HigherEducationCessPercentage", invoice.HigherEducationCessPercentage);
                masterParams.Add("@HigherEducationCessAmount", invoice.HigherEducationCessAmount);
                masterParams.Add("@DiscountPercentage", invoice.DiscountPercentage);
                masterParams.Add("@DiscountAmount", invoice.DiscountAmount);
                masterParams.Add("@PackingAmount", invoice.PackingAmount);
                masterParams.Add("@PackingDescription", invoice.PackingDescription);
                masterParams.Add("@TaxCode", invoice.TaxCode);
                masterParams.Add("@TaxAmount", invoice.TaxAmount);
                masterParams.Add("@GrossAmount", invoice.GrossAmount);
                masterParams.Add("@GrossAmortizationAmount", invoice.GrossAmortizationAmount);
                masterParams.Add("@TcsPercentage", invoice.TcsPercentage);
                masterParams.Add("@TcsAmount", invoice.TcsAmount);
                masterParams.Add("@FreightCharges", invoice.FreightCharges);
                masterParams.Add("@VehicleNumber", invoice.VehicleNumber);
                masterParams.Add("@TransportName", invoice.TransportName);
                masterParams.Add("@IssueDate", invoice.IssueDate);
                masterParams.Add("@RemovalDate", invoice.RemovalDate);
                masterParams.Add("@StorageLocation", invoice.StorageLocation);
                masterParams.Add("@Remarks", invoice.Remarks);
                masterParams.Add("@CreditDays", invoice.CreditDays);
                masterParams.Add("@AsnNumber", invoice.AsnNumber);
                masterParams.Add("@NatureOfProduct", invoice.NatureOfProduct);
                masterParams.Add("@PreparedBy", invoice.PreparedBy);
                masterParams.Add("@ReworkFlag", invoice.ReworkFlag);
                masterParams.Add("@TariffNumber", invoice.TariffNumber);
                masterParams.Add("@TariffName", invoice.TariffName);
                masterParams.Add("@TransportAmount", invoice.TransportAmount);
                masterParams.Add("@CourierAmount", invoice.CourierAmount);
                masterParams.Add("@ServicePercentage", invoice.ServicePercentage);
                masterParams.Add("@ServiceAmount", invoice.ServiceAmount);
                masterParams.Add("@ServiceEducationCessPercentage", invoice.ServiceEducationCessPercentage);
                masterParams.Add("@ServiceEducationCessAmount", invoice.ServiceEducationCessAmount);
                masterParams.Add("@ServiceHigherEducationCessPercentage", invoice.ServiceHigherEducationCessPercentage);
                masterParams.Add("@ServiceHigherEducationCessAmount", invoice.ServiceHigherEducationCessAmount);
                masterParams.Add("@DeliveryAddress", invoice.DeliveryAddress);
                masterParams.Add("@AlternateCustomerCode", invoice.AlternateCustomerCode);
                masterParams.Add("@OtherAmount", invoice.OtherAmount);
                masterParams.Add("@LrNumber", invoice.LrNumber);
                masterParams.Add("@LrDate", invoice.LrDate);
                masterParams.Add("@BuyerName", invoice.BuyerName);
                masterParams.Add("@BuyerAddress", invoice.BuyerAddress);
                masterParams.Add("@InsuranceAmount", invoice.InsuranceAmount);
                masterParams.Add("@FinalDestination", invoice.FinalDestination);
                masterParams.Add("@PreCarriage", invoice.PreCarriage);
                masterParams.Add("@PortOfLoading", invoice.PortOfLoading);
                masterParams.Add("@PortOfDischarge", invoice.PortOfDischarge);
                masterParams.Add("@PlaceOfDelivery", invoice.PlaceOfDelivery);
                masterParams.Add("@CurrencyCode", invoice.CurrencyCode);
                masterParams.Add("@Clearance", invoice.Clearance);
                masterParams.Add("@FlightNumber", invoice.FlightNumber);
                masterParams.Add("@ManufacturingDate", invoice.ManufacturingDate);
                masterParams.Add("@ExpiryDate", invoice.ExpiryDate);
                masterParams.Add("@CurrencyRate", invoice.CurrencyRate);
                masterParams.Add("@AuthorizedSignatory", invoice.AuthorizedSignatory);
                masterParams.Add("@AreaFormNumber", invoice.AreaFormNumber);
                masterParams.Add("@FormDate", invoice.FormDate);
                masterParams.Add("@Shipment", invoice.Shipment);
                masterParams.Add("@CenvatAccountNumber", invoice.CenvatAccountNumber);
                masterParams.Add("@BondNumber", invoice.BondNumber);
                masterParams.Add("@BondDate", invoice.BondDate);
                masterParams.Add("@Ut1FileNumber", invoice.Ut1FileNumber);
                masterParams.Add("@FileNumber", invoice.FileNumber);
                masterParams.Add("@ExportFlag", invoice.ExportFlag);
                masterParams.Add("@ValidityDate", invoice.ValidityDate);
                masterParams.Add("@ExaminationBoxes", invoice.ExaminationBoxes);
                masterParams.Add("@TermsOfDelivery", invoice.TermsOfDelivery);
                masterParams.Add("@TermsOfPayment", invoice.TermsOfPayment);
                masterParams.Add("@VoyageNumber", invoice.VoyageNumber);
                masterParams.Add("@PlaceOfReceipt", invoice.PlaceOfReceipt);
                masterParams.Add("@MarksAndNumbers", invoice.MarksAndNumbers);
                masterParams.Add("@NumberOfPackages", invoice.NumberOfPackages);
                masterParams.Add("@UnNumber", invoice.UnNumber);
                masterParams.Add("@HazardClass", invoice.HazardClass);
                masterParams.Add("@HsCodeNumber", invoice.HsCodeNumber);
                masterParams.Add("@ContainerNumber", invoice.ContainerNumber);
                masterParams.Add("@SealNumber", invoice.SealNumber);
                masterParams.Add("@OtsNumber", invoice.OtsNumber);
                masterParams.Add("@CountryOfOrigin", invoice.CountryOfOrigin);
                masterParams.Add("@CountryOfDestination", invoice.CountryOfDestination);
                masterParams.Add("@TransportBy", invoice.TransportBy);
                masterParams.Add("@AreaRemarks", invoice.AreaRemarks);
                masterParams.Add("@CarrierName", invoice.CarrierName);
                masterParams.Add("@CarrierBookingNumber", invoice.CarrierBookingNumber);
                masterParams.Add("@ShipName", invoice.ShipName);
                masterParams.Add("@TechnicalName", invoice.TechnicalName);
                masterParams.Add("@OuterPackaging", invoice.OuterPackaging);
                masterParams.Add("@InnerPackaging", invoice.InnerPackaging);
                masterParams.Add("@SubsidiaryClass", invoice.SubsidiaryClass);
                masterParams.Add("@UnPackingGroup", invoice.UnPackingGroup);
                masterParams.Add("@UnPackingCode", invoice.UnPackingCode);
                masterParams.Add("@EmsNumber", invoice.EmsNumber);
                masterParams.Add("@FlashPoint", invoice.FlashPoint);
                masterParams.Add("@MarinePollutant", invoice.MarinePollutant);
                masterParams.Add("@ShipperDeclaration", invoice.ShipperDeclaration);
                masterParams.Add("@IecNumber", invoice.IecNumber);
                masterParams.Add("@IecDate", invoice.IecDate);
                masterParams.Add("@CentralExciseRegistration", invoice.CentralExciseRegistration);
                masterParams.Add("@DateOfExamination", invoice.DateOfExamination);
                masterParams.Add("@SuperintendentExciseName", invoice.SuperintendentExciseName);
                masterParams.Add("@InspectorExciseName", invoice.InspectorExciseName);
                masterParams.Add("@CustomsSealNumber", invoice.CustomsSealNumber);
                masterParams.Add("@PermitENumber", invoice.PermitENumber);
                masterParams.Add("@NonCargoNumberOfPackages", invoice.NonCargoNumberOfPackages);
                masterParams.Add("@ShippingBillNumber", invoice.ShippingBillNumber);
                masterParams.Add("@AccessibleAmount", invoice.AccessibleAmount);
                masterParams.Add("@TaxableAmount", invoice.TaxableAmount);
                masterParams.Add("@RoundingAmount", invoice.RoundingAmount);
                masterParams.Add("@AdvanceDuty", invoice.AdvanceDuty);
                masterParams.Add("@OctriAmount", invoice.OctriAmount);
                masterParams.Add("@IssueTime", invoice.IssueTime);
                masterParams.Add("@RemovalTime", invoice.RemovalTime);
                masterParams.Add("@LcNumber", invoice.LcNumber);
                masterParams.Add("@LcDate", invoice.LcDate);
                masterParams.Add("@TransportOwner", invoice.TransportOwner);
                masterParams.Add("@TransportAddress", invoice.TransportAddress);
                masterParams.Add("@TNumber", invoice.TNumber);
                masterParams.Add("@TrayCode", invoice.TrayCode);
                masterParams.Add("@TrayQuantity", invoice.TrayQuantity);
                masterParams.Add("@Address", invoice.Address);
                masterParams.Add("@StateCode", invoice.StateCode);
                masterParams.Add("@HsnCode", invoice.HsnCode);
                masterParams.Add("@ElectronicReferenceNumber", invoice.ElectronicReferenceNumber);
                masterParams.Add("@TermsAndConditions", invoice.TermsAndConditions);
                masterParams.Add("@AuthorizedName", invoice.AuthorizedName);
                masterParams.Add("@AddressSelected", invoice.AddressSelected);

                await connection.ExecuteAsync("ERP_UpdateTaxInvoice", masterParams, 
                    transaction: transaction, commandType: CommandType.StoredProcedure);

                // 2. Delete existing details
                await connection.ExecuteAsync(
                    "ERP_DeleteInvoiceDetails", 
                    new { InvoiceCode = invoice.InvoiceCode }, 
                    transaction: transaction, commandType: CommandType.StoredProcedure);

                // 3. Insert new details
                foreach (var detail in invoice.InvoiceDetails)
                {
                    var detailParams = new DynamicParameters();
                    detailParams.Add("@InvoiceMasterCode", invoice.InvoiceCode);
                    detailParams.Add("@ItemCode", detail.ItemCode);
                    detailParams.Add("@UomCode", detail.UomCode);
                    detailParams.Add("@CustomerPoCode", detail.CustomerPoCode);
                    detailParams.Add("@InvoiceQuantity", detail.InvoiceQuantity);
                    detailParams.Add("@Rate", detail.Rate);
                    detailParams.Add("@ConversionQuantity", detail.ConversionQuantity);
                    detailParams.Add("@AmortizationRate", detail.AmortizationRate);
                    detailParams.Add("@NumberOfPackages", detail.NumberOfPackages);
                    detailParams.Add("@PackageDescription", detail.PackageDescription);
                    detailParams.Add("@QuantityPerPack", detail.QuantityPerPack);
                    detailParams.Add("@Amount", detail.Amount);
                    detailParams.Add("@DeliveryChallanNumbers", detail.DeliveryChallanNumbers);
                    detailParams.Add("@DeliveryChallanDates", detail.DeliveryChallanDates);
                    detailParams.Add("@ExciseNumbers", detail.ExciseNumbers);
                    detailParams.Add("@ProcessCode", detail.ProcessCode);
                    detailParams.Add("@GinNumber", detail.GinNumber);
                    detailParams.Add("@GinDate", detail.GinDate);
                    detailParams.Add("@GinReceipt", detail.GinReceipt);
                    detailParams.Add("@MrCode", detail.MrCode);
                    detailParams.Add("@GinAcceptance", detail.GinAcceptance);
                    detailParams.Add("@ExciseAmount", detail.ExciseAmount);
                    detailParams.Add("@EducationCessAmount", detail.EducationCessAmount);
                    detailParams.Add("@SecondaryHigherEducationCessAmount", detail.SecondaryHigherEducationCessAmount);
                    detailParams.Add("@CgstPercentage", detail.CgstPercentage);
                    detailParams.Add("@SgstPercentage", detail.SgstPercentage);
                    detailParams.Add("@IgstPercentage", detail.IgstPercentage);
                    detailParams.Add("@SerialNumber", detail.SerialNumber);
                    detailParams.Add("@Remarks", detail.Remarks);
                    detailParams.Add("@ItemWarehouseCode", detail.ItemWarehouseCode);
                    detailParams.Add("@ActualWeight", detail.ActualWeight);
                    detailParams.Add("@Size", detail.Size);
                    detailParams.Add("@SubHeading", detail.SubHeading);
                    detailParams.Add("@BatchNumber", detail.BatchNumber);
                    detailParams.Add("@PackingQuantity", detail.PackingQuantity);
                    detailParams.Add("@GrossWeight", detail.GrossWeight);
                    detailParams.Add("@NetWeight", detail.NetWeight);
                    detailParams.Add("@SizeOfBox", detail.SizeOfBox);
                    detailParams.Add("@NumberOfBarrels", detail.NumberOfBarrels);
                    detailParams.Add("@NumberOfPackagesDescription", detail.NumberOfPackagesDescription);
                    detailParams.Add("@ContainerNumber", detail.ContainerNumber);
                    detailParams.Add("@RefundableQuantity", detail.RefundableQuantity);
                    detailParams.Add("@AmortRate", detail.AmortRate);
                    detailParams.Add("@AmortAmount", detail.AmortAmount);
                    detailParams.Add("@HsnCode", detail.HsnCode);
                    detailParams.Add("@StoreCode", detail.StoreCode);

                    await connection.ExecuteAsync("ERP_CreateTaxInvoiceDetail", detailParams, 
                        transaction: transaction, commandType: CommandType.StoredProcedure);
                }

                transaction.Commit();
                _logger.LogInformation("Tax Invoice updated successfully. Invoice Code: {InvoiceCode}", invoice.InvoiceCode);

                return await GetTaxInvoiceByIdAsync(invoice.InvoiceCode, invoice.CompanyCode ?? 0);
            }
            catch (Exception ex)
            {
                try { transaction.Rollback(); } catch { /* transaction already completed by SQL Server */ }
                _logger.LogError(ex, "Error updating Tax Invoice: {InvoiceCode}", invoice.InvoiceCode);
                throw;
            }
        }

        public async Task<bool> DeleteTaxInvoiceAsync(long invoiceCode, int companyCode)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@InvoiceCode", invoiceCode);
            parameters.Add("@CompanyCode", companyCode);

            var result = await connection.ExecuteAsync("ERP_DeleteTaxInvoice", parameters, 
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }

        public async Task<TaxInvoiceMasterDto?> GetTaxInvoiceByIdAsync(long invoiceCode, int companyCode)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@InvoiceCode", invoiceCode);
            parameters.Add("@CompanyCode", companyCode);

            using var multi = await connection.QueryMultipleAsync(
                "ERP_GetTaxInvoiceById", parameters, commandType: CommandType.StoredProcedure);

            var invoice = await multi.ReadFirstOrDefaultAsync<TaxInvoiceMasterDto>();
            if (invoice != null)
            {
                var details = await multi.ReadAsync<TaxInvoiceDetailDto>();
                invoice.InvoiceDetails = details.ToList();
            }

            return invoice;
        }

        public async Task<TaxInvoicePagedResponse> GetAllTaxInvoicesAsync(GetAllTaxInvoicesQuery query)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", query.CompanyId);
            parameters.Add("@CustomerId", query.CustomerId);
            parameters.Add("@CustomerPoCode", query.CustomerPoCode);
            parameters.Add("@InvoiceDateFrom", query.InvoiceDateFrom);
            parameters.Add("@InvoiceDateTo", query.InvoiceDateTo);
            parameters.Add("@InvoiceNumber", query.InvoiceNumber);
            parameters.Add("@InvoiceType", query.InvoiceType);
            parameters.Add("@IsDeleted", query.IsDeleted);
            parameters.Add("@IsSupplementary", query.IsSupplementary);
            parameters.Add("@ExportFlag", query.ExportFlag);
            parameters.Add("@SearchTerm", query.SearchTerm);
            parameters.Add("@SortBy", query.SortBy);
            parameters.Add("@SortOrder", query.SortOrder);
            parameters.Add("@PageNumber", query.PageNumber);
            parameters.Add("@PageSize", query.PageSize);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var invoices = await connection.QueryAsync<TaxInvoiceMasterDto>("ERP_GetAllTaxInvoices", parameters, 
                commandType: CommandType.StoredProcedure);

            var totalCount = parameters.Get<int>("@TotalCount");

            return new TaxInvoicePagedResponse
            {
                Data = invoices.ToList(),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<bool> IsInvoiceLockedAsync(long invoiceCode)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@InvoiceCode", invoiceCode);
            parameters.Add("@IsLocked", dbType: DbType.Boolean, direction: ParameterDirection.Output);
            await connection.ExecuteAsync("ERP_CheckInvoiceLock", parameters,
                commandType: CommandType.StoredProcedure);
            return parameters.Get<bool>("@IsLocked");
        }

        public async Task<bool> LockInvoiceAsync(long invoiceCode, int lockedByUserId)
        {
            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteScalarAsync<int>(
                "ERP_LockInvoice",
                new { InvoiceCode = invoiceCode, LockedByUserId = lockedByUserId },
                commandType: CommandType.StoredProcedure);
            return rowsAffected > 0;
        }

        public async Task<bool> UnlockInvoiceAsync(long invoiceCode)
        {
            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteScalarAsync<int>(
                "ERP_UnlockInvoice",
                new { InvoiceCode = invoiceCode },
                commandType: CommandType.StoredProcedure);
            return rowsAffected > 0;
        }

        public async Task<List<TaxInvoiceDetailDto>> GetAvailableItemsFromPoAsync(int customerPoCode, int companyCode)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@CustomerPoCode", customerPoCode);
            parameters.Add("@CompanyCode", companyCode);

            var items = await connection.QueryAsync<TaxInvoiceDetailDto>("ERP_GetAvailableItemsFromPo", parameters, 
                commandType: CommandType.StoredProcedure);

            return items.ToList();
        }

        public async Task<bool> ValidateItemStockAsync(int itemCode, int companyCode, double requiredQuantity)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@ItemCode", itemCode);
            parameters.Add("@CompanyCode", companyCode);
            parameters.Add("@AvailableQuantity", dbType: DbType.Double, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("ERP_GetItemStock", parameters, 
                commandType: CommandType.StoredProcedure);

            var availableQuantity = parameters.Get<double>("@AvailableQuantity");
            return availableQuantity >= requiredQuantity;
        }

        public async Task<int> GenerateInvoiceNumberAsync(int companyCode)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyCode", companyCode);
            parameters.Add("@NewInvoiceNumber", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("ERP_GenerateInvoiceNumber", parameters, 
                commandType: CommandType.StoredProcedure);

            return parameters.Get<int>("@NewInvoiceNumber");
        }

        public async Task<TaxInvoicePrintDto?> GetPrintDataAsync(long invoiceCode, int companyId)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@InvoiceCode", invoiceCode);
            parameters.Add("@CompanyId", companyId);

            using var multi = await connection.QueryMultipleAsync(
                "ERP_GetTaxInvoicePrintData_V2",  // Updated to V2
                parameters, 
                commandType: CommandType.StoredProcedure);

            // Read all 8 result sets in order
            // Using FirstOrDefaultAsync instead of ReadSingleOrDefaultAsync to handle cases where SP might return multiple rows
            // 1. Company Information
            var company = await multi.ReadFirstOrDefaultAsync<CompanyPrintInfo>();
            
            // 2. Invoice Header
            var invoiceHeader = await multi.ReadFirstOrDefaultAsync<InvoiceHeaderPrintInfo>();
            
            // 3. Recipient Details
            var recipient = await multi.ReadFirstOrDefaultAsync<RecipientPrintInfo>();
            
            // 4. Delivery Details
            var delivery = await multi.ReadFirstOrDefaultAsync<DeliveryPrintInfo>();
            
            // 5. Line Items
            var lineItems = (await multi.ReadAsync<InvoiceDetailPrintInfo>()).ToList();
            
            // 6. Totals and Tax Summary
            var totals = await multi.ReadFirstOrDefaultAsync<TotalsPrintInfo>();
            
            // 7. E-Invoice Information
            var eInvoice = await multi.ReadFirstOrDefaultAsync<EInvoicePrintInfo>();
            
            // 8. Terms and Conditions
            var terms = (await multi.ReadAsync<TermConditionItem>()).Select(t => t.TermCondition).ToList();

            // Return null if essential data not found
            if (company == null || invoiceHeader == null || recipient == null)
            {
                return null;
            }

            // Convert amount to words using NumberToWordsConverter
            if (totals != null)
            {
                totals.AmountInWords = NumberToWordsConverter.ConvertToWords((double)totals.GrandTotal);
            }

            return new TaxInvoicePrintDto
            {
                Company = company,
                InvoiceHeader = invoiceHeader,
                Recipient = recipient,
                Delivery = delivery ?? new DeliveryPrintInfo
                {
                    Name = recipient.Name,
                    Address = recipient.Address,
                    StateName = recipient.StateName,
                    StateCode = recipient.StateCode,
                    GstinNo = recipient.GstinNo
                },
                LineItems = lineItems,
                Totals = totals ?? new TotalsPrintInfo(),
                EInvoice = eInvoice,
                TermsAndConditions = terms
            };
        }

        public async Task<bool> ApproveAsync(long invoiceCode, int companyCode)
        {
            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteScalarAsync<int>(
                "ERP_ApproveTaxInvoice",
                new { InvoiceCode = invoiceCode, CompanyCode = companyCode },
                commandType: CommandType.StoredProcedure);
            return rowsAffected > 0;
        }

        public async Task<List<TaxInvoiceCustomerDto>> GetCustomersWithActivePosAsync(int companyCode)
        {
            using var connection = new SqlConnection(_connectionString);
            var result = await connection.QueryAsync<TaxInvoiceCustomerDto>(
                "ERP_GetTaxInvoiceCustomers",
                new { CompanyCode = companyCode },
                commandType: CommandType.StoredProcedure);
            return result.ToList();
        }

        public async Task<List<TaxInvoiceItemDto>> GetItemsByCustomerAsync(int customerCode, int companyCode)
        {
            using var connection = new SqlConnection(_connectionString);
            var result = await connection.QueryAsync<TaxInvoiceItemDto>(
                "ERP_GetTaxInvoiceItemsByCustomer",
                new { CustomerCode = customerCode, CompanyCode = companyCode },
                commandType: CommandType.StoredProcedure);
            return result.ToList();
        }

        public async Task<TaxInvoiceItemDetailsDto?> GetItemDetailsAsync(int itemCode, int companyCode)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<TaxInvoiceItemDetailsDto>(
                "ERP_GetTaxInvoiceItemDetails",
                new { ItemCode = itemCode, CompanyCode = companyCode },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<TaxInvoicePoDto>> GetPOsByItemCustomerAsync(int itemCode, int customerCode, int companyCode, int? invoiceCode)
        {
            using var connection = new SqlConnection(_connectionString);
            var result = await connection.QueryAsync<TaxInvoicePoDto>(
                "ERP_GetTaxInvoicePOsByItemCustomer",
                new { ItemCode = itemCode, CustomerCode = customerCode, CompanyCode = companyCode, InvoiceCode = invoiceCode },
                commandType: CommandType.StoredProcedure);
            return result.ToList();
        }

        public async Task<CompanyStateDto?> GetCompanyStateAsync(int companyCode)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<CompanyStateDto>(
                "ERP_GetCompanyState",
                new { CompanyCode = companyCode },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<SalesTaxDto>> GetSalesTaxMasterAsync(int companyCode)
        {
            using var connection = new SqlConnection(_connectionString);
            var result = await connection.QueryAsync<SalesTaxDto>(
                "ERP_GetSalesTaxMaster",
                new { CompanyCode = companyCode },
                commandType: CommandType.StoredProcedure);
            return result.ToList();
        }
    }
}

