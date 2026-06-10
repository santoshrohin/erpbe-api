using Dapper;
using ErpBE.Application.DTOs;
using ErpBE.Application.DTOs.LabourChargeInvoice;
using ErpBE.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ErpBE.Infrastructure.Repositories
{
    public class LabourChargeInvoiceRepository : ILabourChargeInvoiceRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<LabourChargeInvoiceRepository> _logger;

        public LabourChargeInvoiceRepository(
            IConfiguration configuration,
            ILogger<LabourChargeInvoiceRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger;
        }

        public async Task<LabourChargeInvoicePagedResponse> GetAllAsync(LabourChargeInvoiceQueryParameters p)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", p.CompanyCode);
            parameters.Add("@CustomerId", (object?)null);
            parameters.Add("@CustomerPoCode", (object?)null);
            parameters.Add("@InvoiceDateFrom", p.DateFrom);
            parameters.Add("@InvoiceDateTo", p.DateTo);
            parameters.Add("@SearchTerm", p.SearchText);
            parameters.Add("@SortBy", "InvoiceDate");
            parameters.Add("@SortOrder", "desc");
            parameters.Add("@PageNumber", p.PageNumber);
            parameters.Add("@PageSize", p.PageSize);
            parameters.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var data = await connection.QueryAsync<LabourChargeInvoiceMasterDto>(
                "ERP_GetAllLabourChargeInvoices",
                parameters,
                commandType: CommandType.StoredProcedure);

            var totalCount = parameters.Get<int>("@TotalCount");

            return new LabourChargeInvoicePagedResponse
            {
                Data = data.ToList(),
                TotalCount = totalCount
            };
        }

        public async Task<LabourChargeInvoiceMasterDto?> GetByIdAsync(int id, int companyCode)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@InvoiceCode", id);
            parameters.Add("@CompanyCode", companyCode);

            using var multi = await connection.QueryMultipleAsync(
                "ERP_GetLabourChargeInvoiceById",
                parameters,
                commandType: CommandType.StoredProcedure);

            var master = await multi.ReadFirstOrDefaultAsync<LabourChargeInvoiceMasterDto>();
            if (master != null)
            {
                master.Details = (await multi.ReadAsync<LabourChargeInvoiceDetailDto>()).ToList();
            }

            return master;
        }

        public async Task<LabourChargeInvoiceMasterDto> CreateAsync(CreateLabourChargeInvoiceRequest req)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Get next invoice number for OutJWINM type
                var numParams = new DynamicParameters();
                numParams.Add("@CompanyCode", req.CompanyCode);
                numParams.Add("@NewInvoiceNumber", dbType: DbType.Int32, direction: ParameterDirection.Output);
                await connection.ExecuteAsync("ERP_GetNextLabourChargeInvoiceNumber", numParams,
                    transaction: transaction, commandType: CommandType.StoredProcedure);
                var invoiceNumber = numParams.Get<int?>("@NewInvoiceNumber") ?? 1;

                // 2. Call ERP_CreateTaxInvoice with @Type='OutJWINM'
                var masterParams = new DynamicParameters();
                masterParams.Add("@CompanyCode", req.CompanyCode);
                masterParams.Add("@InvoiceNumber", invoiceNumber);
                masterParams.Add("@InvoiceDate", req.InvoiceDate);
                masterParams.Add("@InvoiceType", req.InvoiceType ?? 0);
                masterParams.Add("@Type", "OutJWINM");
                masterParams.Add("@PaymentMethodCode", (object?)null);
                masterParams.Add("@CustomerCode", req.CustomerCode);
                masterParams.Add("@CustomerPoCode", req.CustomerPoCode);
                masterParams.Add("@DateFrom", (object?)null);
                masterParams.Add("@DateTo", (object?)null);
                masterParams.Add("@IsSupplementary", req.IsSupplementary.HasValue ? (object)req.IsSupplementary.Value : (object?)null);
                masterParams.Add("@Process", (object?)null);
                masterParams.Add("@NetAmount", req.NetAmount);
                masterParams.Add("@ServiceTaxPercentage", req.ServiceTaxPercentage);
                masterParams.Add("@ServiceTaxAmount", (object?)null);
                masterParams.Add("@BasicExcisePercentage", (object?)null);
                masterParams.Add("@BasicExciseAmount", (object?)null);
                masterParams.Add("@EducationCessPercentage", (object?)null);
                masterParams.Add("@EducationCessAmount", (object?)null);
                masterParams.Add("@HigherEducationCessPercentage", (object?)null);
                masterParams.Add("@HigherEducationCessAmount", (object?)null);
                masterParams.Add("@DiscountPercentage", req.DiscountPercentage);
                masterParams.Add("@DiscountAmount", (object?)null);
                masterParams.Add("@PackingAmount", req.PackingAmount);
                masterParams.Add("@PackingDescription", (object?)null);
                masterParams.Add("@TaxCode", req.TaxCode);
                masterParams.Add("@TaxAmount", (object?)null);
                masterParams.Add("@GrossAmount", req.GrossAmount);
                masterParams.Add("@GrossAmortizationAmount", (object?)null);
                masterParams.Add("@TcsPercentage", req.TcsPercentage);
                masterParams.Add("@TcsAmount", (object?)null);
                masterParams.Add("@FreightCharges", req.FreightCharges);
                masterParams.Add("@VehicleNumber", req.VehicleNumber);
                masterParams.Add("@TransportName", req.TransportName);
                masterParams.Add("@IssueDate", req.IssueDate);
                masterParams.Add("@RemovalDate", req.RemovalDate);
                masterParams.Add("@StorageLocation", (object?)null);
                masterParams.Add("@Remarks", req.Remarks);
                masterParams.Add("@CreditDays", req.CreditDays);
                masterParams.Add("@AsnNumber", (object?)null);
                masterParams.Add("@NatureOfProduct", (object?)null);
                masterParams.Add("@PreparedBy", (object?)null);
                masterParams.Add("@ReworkFlag", (object?)null);
                masterParams.Add("@TallyTransferFlag", (object?)null);
                masterParams.Add("@TempTallyTransferFlag", (object?)null);
                masterParams.Add("@TariffNumber", (object?)null);
                masterParams.Add("@TariffName", (object?)null);
                masterParams.Add("@TallyTransferFlag1", (object?)null);
                masterParams.Add("@TempTallyTransferFlag1", (object?)null);
                masterParams.Add("@TransportAmount", req.TransportAmount);
                masterParams.Add("@CourierAmount", (object?)null);
                masterParams.Add("@ServicePercentage", (object?)null);
                masterParams.Add("@ServiceAmount", (object?)null);
                masterParams.Add("@ServiceEducationCessPercentage", (object?)null);
                masterParams.Add("@ServiceEducationCessAmount", (object?)null);
                masterParams.Add("@ServiceHigherEducationCessPercentage", (object?)null);
                masterParams.Add("@ServiceHigherEducationCessAmount", (object?)null);
                masterParams.Add("@DeliveryAddress", (object?)null);
                masterParams.Add("@AlternateCustomerCode", (object?)null);
                masterParams.Add("@OtherAmount", req.OtherAmount);
                masterParams.Add("@LrNumber", req.LrNumber);
                masterParams.Add("@LrDate", req.LrDate);
                masterParams.Add("@BuyerName", (object?)null);
                masterParams.Add("@BuyerAddress", (object?)null);
                masterParams.Add("@InsuranceAmount", req.InsuranceAmount);
                masterParams.Add("@FinalDestination", (object?)null);
                masterParams.Add("@PreCarriage", (object?)null);
                masterParams.Add("@PortOfLoading", (object?)null);
                masterParams.Add("@PortOfDischarge", (object?)null);
                masterParams.Add("@PlaceOfDelivery", (object?)null);
                masterParams.Add("@CurrencyCode", (object?)null);
                masterParams.Add("@Clearance", (object?)null);
                masterParams.Add("@FlightNumber", (object?)null);
                masterParams.Add("@ManufacturingDate", (object?)null);
                masterParams.Add("@ExpiryDate", (object?)null);
                masterParams.Add("@CurrencyRate", (object?)null);
                masterParams.Add("@AuthorizedSignatory", (object?)null);
                masterParams.Add("@AreaFormNumber", (object?)null);
                masterParams.Add("@FormDate", (object?)null);
                masterParams.Add("@Shipment", (object?)null);
                masterParams.Add("@CenvatAccountNumber", (object?)null);
                masterParams.Add("@BondNumber", (object?)null);
                masterParams.Add("@BondDate", (object?)null);
                masterParams.Add("@Ut1FileNumber", (object?)null);
                masterParams.Add("@FileNumber", (object?)null);
                masterParams.Add("@ExportFlag", (object?)null);
                masterParams.Add("@ValidityDate", (object?)null);
                masterParams.Add("@ExaminationBoxes", (object?)null);
                masterParams.Add("@TermsOfDelivery", (object?)null);
                masterParams.Add("@TermsOfPayment", (object?)null);
                masterParams.Add("@VoyageNumber", (object?)null);
                masterParams.Add("@PlaceOfReceipt", (object?)null);
                masterParams.Add("@MarksAndNumbers", (object?)null);
                masterParams.Add("@NumberOfPackages", (object?)null);
                masterParams.Add("@UnNumber", (object?)null);
                masterParams.Add("@HazardClass", (object?)null);
                masterParams.Add("@HsCodeNumber", (object?)null);
                masterParams.Add("@ContainerNumber", (object?)null);
                masterParams.Add("@SealNumber", (object?)null);
                masterParams.Add("@OtsNumber", (object?)null);
                masterParams.Add("@CountryOfOrigin", (object?)null);
                masterParams.Add("@CountryOfDestination", (object?)null);
                masterParams.Add("@TransportBy", (object?)null);
                masterParams.Add("@AreaRemarks", (object?)null);
                masterParams.Add("@CarrierName", (object?)null);
                masterParams.Add("@CarrierBookingNumber", (object?)null);
                masterParams.Add("@ShipName", (object?)null);
                masterParams.Add("@TechnicalName", (object?)null);
                masterParams.Add("@OuterPackaging", (object?)null);
                masterParams.Add("@InnerPackaging", (object?)null);
                masterParams.Add("@SubsidiaryClass", (object?)null);
                masterParams.Add("@UnPackingGroup", (object?)null);
                masterParams.Add("@UnPackingCode", (object?)null);
                masterParams.Add("@EmsNumber", (object?)null);
                masterParams.Add("@FlashPoint", (object?)null);
                masterParams.Add("@MarinePollutant", (object?)null);
                masterParams.Add("@ShipperDeclaration", (object?)null);
                masterParams.Add("@IecNumber", (object?)null);
                masterParams.Add("@IecDate", (object?)null);
                masterParams.Add("@CentralExciseRegistration", (object?)null);
                masterParams.Add("@DateOfExamination", (object?)null);
                masterParams.Add("@SuperintendentExciseName", (object?)null);
                masterParams.Add("@InspectorExciseName", (object?)null);
                masterParams.Add("@CustomsSealNumber", (object?)null);
                masterParams.Add("@PermitENumber", (object?)null);
                masterParams.Add("@IsTallyTransferred", (object?)null);
                masterParams.Add("@NonCargoNumberOfPackages", (object?)null);
                masterParams.Add("@ShippingBillNumber", (object?)null);
                masterParams.Add("@AccessibleAmount", (object?)null);
                masterParams.Add("@TaxableAmount", req.TaxableAmount);
                masterParams.Add("@RoundingAmount", req.RoundingAmount);
                masterParams.Add("@AdvanceDuty", (object?)null);
                masterParams.Add("@OctriAmount", req.OctriAmount);
                masterParams.Add("@IsSuppliment", (object?)null);
                masterParams.Add("@IssueTime", (object?)null);
                masterParams.Add("@RemovalTime", (object?)null);
                masterParams.Add("@LcNumber", (object?)null);
                masterParams.Add("@LcDate", (object?)null);
                masterParams.Add("@TransportOwner", (object?)null);
                masterParams.Add("@TransportAddress", (object?)null);
                masterParams.Add("@TNumber", (object?)null);
                masterParams.Add("@TrayCode", (object?)null);
                masterParams.Add("@TrayQuantity", (object?)null);
                masterParams.Add("@Address", (object?)null);
                masterParams.Add("@StateCode", (object?)null);
                masterParams.Add("@HsnCode", req.HsnCode);
                masterParams.Add("@ElectronicReferenceNumber", (object?)null);
                masterParams.Add("@TermsAndConditions", (object?)null);
                masterParams.Add("@AuthorizedName", (object?)null);
                masterParams.Add("@AddressSelected", (object?)null);
                masterParams.Add("@NewInvoiceCode", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await connection.ExecuteAsync(
                    "ERP_CreateTaxInvoice",
                    masterParams,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);

                var newInvoiceCode = masterParams.Get<int>("@NewInvoiceCode");

                // 3. Insert each detail
                foreach (var detail in req.Details)
                {
                    await InsertDetailAsync(connection, transaction, newInvoiceCode, detail);
                }

                transaction.Commit();
                _logger.LogInformation("Labour Charge Invoice created. Code: {InvoiceCode}", newInvoiceCode);

                return (await GetByIdAsync(newInvoiceCode, req.CompanyCode))!;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error creating Labour Charge Invoice");
                throw;
            }
        }

        public async Task<LabourChargeInvoiceMasterDto?> UpdateAsync(UpdateLabourChargeInvoiceRequest req)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Update master
                var masterParams = new DynamicParameters();
                masterParams.Add("@InvoiceCode", req.InvoiceCode);
                masterParams.Add("@CompanyCode", req.CompanyCode);
                masterParams.Add("@InvoiceDate", req.InvoiceDate);
                masterParams.Add("@InvoiceType", req.InvoiceType);
                masterParams.Add("@CustomerCode", req.CustomerCode);
                masterParams.Add("@CustomerPoCode", req.CustomerPoCode);
                masterParams.Add("@NetAmount", req.NetAmount);
                masterParams.Add("@DiscountPercentage", req.DiscountPercentage);
                masterParams.Add("@DiscountAmount", (object?)null);
                masterParams.Add("@ServiceTaxPercentage", req.ServiceTaxPercentage);
                masterParams.Add("@ServiceTaxAmount", (object?)null);
                masterParams.Add("@TcsPercentage", req.TcsPercentage);
                masterParams.Add("@TcsAmount", (object?)null);
                masterParams.Add("@PackingAmount", req.PackingAmount);
                masterParams.Add("@GrossAmount", req.GrossAmount);
                masterParams.Add("@TaxCode", req.TaxCode);
                masterParams.Add("@VehicleNumber", req.VehicleNumber);
                masterParams.Add("@TransportName", req.TransportName);
                masterParams.Add("@IssueDate", req.IssueDate);
                masterParams.Add("@RemovalDate", req.RemovalDate);
                masterParams.Add("@Remarks", req.Remarks);
                masterParams.Add("@LrNumber", req.LrNumber);
                masterParams.Add("@LrDate", req.LrDate);
                masterParams.Add("@TaxableAmount", req.TaxableAmount);
                masterParams.Add("@RoundingAmount", req.RoundingAmount);
                masterParams.Add("@OtherAmount", req.OtherAmount);
                masterParams.Add("@FreightCharges", req.FreightCharges);
                masterParams.Add("@InsuranceAmount", req.InsuranceAmount);
                masterParams.Add("@TransportAmount", req.TransportAmount);
                masterParams.Add("@OctriAmount", req.OctriAmount);
                masterParams.Add("@CreditDays", req.CreditDays);
                masterParams.Add("@HsnCode", req.HsnCode);

                await connection.ExecuteAsync(
                    "ERP_UpdateLabourChargeInvoice",
                    masterParams,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);

                // 2. Delete existing details
                await connection.ExecuteAsync(
                    "ERP_DeleteInvoiceDetails",
                    new { InvoiceCode = req.InvoiceCode },
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);

                // 3. Re-insert details
                foreach (var detail in req.Details)
                {
                    await InsertDetailAsync(connection, transaction, req.InvoiceCode, detail);
                }

                transaction.Commit();
                _logger.LogInformation("Labour Charge Invoice updated. Code: {InvoiceCode}", req.InvoiceCode);

                return await GetByIdAsync(req.InvoiceCode, req.CompanyCode);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Error updating Labour Charge Invoice: {InvoiceCode}", req.InvoiceCode);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id, int companyCode)
        {
            using var connection = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("@InvoiceCode", id);
            parameters.Add("@CompanyCode", companyCode);

            var result = await connection.ExecuteAsync(
                "ERP_DeleteLabourChargeInvoice",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result > 0;
        }

        public async Task<bool> LockAsync(int id, int companyCode, int lockedByUserId)
        {
            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteScalarAsync<int>(
                "ERP_LockLabourChargeInvoice",
                new { InvoiceCode = id, CompanyCode = companyCode, LockedByUserId = lockedByUserId },
                commandType: CommandType.StoredProcedure);
            return rowsAffected > 0;
        }

        public async Task<bool> UnlockAsync(int id, int companyCode)
        {
            using var connection = new SqlConnection(_connectionString);
            var rowsAffected = await connection.ExecuteScalarAsync<int>(
                "ERP_UnlockLabourChargeInvoice",
                new { InvoiceCode = id, CompanyCode = companyCode },
                commandType: CommandType.StoredProcedure);
            return rowsAffected > 0;
        }

        public async Task<LabourChargeInvoicePrintDto?> GetPrintDataAsync(int invoiceCode, int companyCode)
        {
            using var connection = new SqlConnection(_connectionString);
            using var multi = await connection.QueryMultipleAsync(
                "ERP_GetLabourChargeInvoicePrintData",
                new { InvoiceCode = invoiceCode, CompanyCode = companyCode },
                commandType: CommandType.StoredProcedure);

            var master = await multi.ReadFirstOrDefaultAsync<LabourChargeInvoicePrintDto>();
            if (master == null) return null;

            master.LineItems = (await multi.ReadAsync<LciDetailPrintInfo>()).ToList();
            return master;
        }

        // ── Private helper ────────────────────────────────────────────────────
        private static async Task InsertDetailAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            int invoiceMasterCode,
            CreateLabourChargeInvoiceDetailRequest detail)
        {
            var detailParams = new DynamicParameters();
            detailParams.Add("@InvoiceMasterCode", invoiceMasterCode);
            detailParams.Add("@ItemCode", detail.ItemCode);
            detailParams.Add("@UomCode", detail.UomCode);
            detailParams.Add("@CustomerPoCode", detail.CustomerPoCode);
            detailParams.Add("@InvoiceQuantity", (double)detail.InvoiceQuantity);
            detailParams.Add("@Rate", detail.Rate);
            detailParams.Add("@ConversionQuantity", (object?)null);
            detailParams.Add("@AmortizationRate", (object?)null);
            detailParams.Add("@NumberOfPackages", (object?)null);
            detailParams.Add("@PackageDescription", (object?)null);
            detailParams.Add("@QuantityPerPack", (object?)null);
            detailParams.Add("@Amount", detail.Amount);
            detailParams.Add("@DeliveryChallanNumbers", (object?)null);
            detailParams.Add("@DeliveryChallanDates", (object?)null);
            detailParams.Add("@ExciseNumbers", (object?)null);
            detailParams.Add("@ProcessCode", (object?)null);
            detailParams.Add("@GinNumber", (object?)null);
            detailParams.Add("@GinDate", (object?)null);
            detailParams.Add("@GinReceipt", (object?)null);
            detailParams.Add("@MrCode", (object?)null);
            detailParams.Add("@GinAcceptance", (object?)null);
            detailParams.Add("@ExciseAmount", (object?)null);
            detailParams.Add("@EducationCessAmount", (object?)null);
            detailParams.Add("@SecondaryHigherEducationCessAmount", (object?)null);
            detailParams.Add("@CgstPercentage", detail.CgstPercentage);
            detailParams.Add("@SgstPercentage", detail.SgstPercentage);
            detailParams.Add("@IgstPercentage", detail.IgstPercentage);
            detailParams.Add("@SerialNumber", (object?)null);
            detailParams.Add("@Remarks", detail.Remarks);
            detailParams.Add("@ItemWarehouseCode", (object?)null);
            detailParams.Add("@ActualWeight", (object?)null);
            detailParams.Add("@Size", (object?)null);
            detailParams.Add("@SubHeading", detail.SubHeading);
            detailParams.Add("@BatchNumber", detail.BatchNumber);
            detailParams.Add("@PackingQuantity", (object?)null);
            detailParams.Add("@GrossWeight", (object?)null);
            detailParams.Add("@NetWeight", (object?)null);
            detailParams.Add("@SizeOfBox", (object?)null);
            detailParams.Add("@NumberOfBarrels", (object?)null);
            detailParams.Add("@NumberOfPackagesDescription", (object?)null);
            detailParams.Add("@ContainerNumber", (object?)null);
            detailParams.Add("@RefundableQuantity", (object?)null);
            detailParams.Add("@AmortRate", detail.AmortRate);
            detailParams.Add("@AmortAmount", detail.AmortAmount);
            detailParams.Add("@HsnCode", detail.HsnCode);
            detailParams.Add("@StoreCode", (object?)null);

            await connection.ExecuteAsync(
                "ERP_CreateTaxInvoiceDetail",
                detailParams,
                transaction: transaction,
                commandType: CommandType.StoredProcedure);
        }
    }
}
