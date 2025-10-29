using Dapper;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace ErpBE.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Customer PO operations
/// </summary>
public class CustomerPoRepository : ICustomerPoRepository
{
    private readonly string _connectionString;

    public CustomerPoRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<CustomerPoMasterDto> CreateAsync(CustomerPoMasterDto po, IEnumerable<CustomerPoDetailDto> details)
    {
        int poCode = 0;
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Create Master
            var masterParameters = new DynamicParameters();
            masterParameters.Add("@CustomerCode", po.CustomerCode);
            masterParameters.Add("@PoNumber", po.PoNumber);
            masterParameters.Add("@PoType", po.PoType);
            masterParameters.Add("@PoDate", po.PoDate);
            masterParameters.Add("@CreditDays", po.CreditDays);
            masterParameters.Add("@CompanyId", po.CompanyId);
            masterParameters.Add("@WorkOrderNumber", po.WorkOrderNumber);
            masterParameters.Add("@PaymentTerms", po.PaymentTerms);
            masterParameters.Add("@IsAuthorized", po.IsAuthorized);
            masterParameters.Add("@CustomerPoDate", po.CustomerPoDate);
            masterParameters.Add("@QuotationCode", po.QuotationCode);
            masterParameters.Add("@TaxName", po.TaxName);
            masterParameters.Add("@TaxPercentage", po.TaxPercentage);
            masterParameters.Add("@TaxAmount", po.TaxAmount);
            masterParameters.Add("@ExcisePercentage", po.ExcisePercentage);
            masterParameters.Add("@ExciseEducationPercentage", po.ExciseEducationPercentage);
            masterParameters.Add("@ExciseHigherEducationPercentage", po.ExciseHigherEducationPercentage);
            masterParameters.Add("@BasicAmount", po.BasicAmount);
            masterParameters.Add("@DiscountPercentage", po.DiscountPercentage);
            masterParameters.Add("@DiscountAmount", po.DiscountAmount);
            masterParameters.Add("@DiscountReason", po.DiscountReason);
            masterParameters.Add("@DeviationAmount", po.DeviationAmount);
            masterParameters.Add("@DeviationReason", po.DeviationReason);
            masterParameters.Add("@PackingAmount", po.PackingAmount);
            masterParameters.Add("@ExciseAmount", po.ExciseAmount);
            masterParameters.Add("@RoundingAmount", po.RoundingAmount);
            masterParameters.Add("@GrandTotal", po.GrandTotal);
            masterParameters.Add("@FinalDestination", po.FinalDestination);
            masterParameters.Add("@PreCarriageBy", po.PreCarriageBy);
            masterParameters.Add("@PortOfLoading", po.PortOfLoading);
            masterParameters.Add("@PortOfDischarge", po.PortOfDischarge);
            masterParameters.Add("@PlaceOfDelivery", po.PlaceOfDelivery);
            masterParameters.Add("@BuyerName", po.BuyerName);
            masterParameters.Add("@BuyerAddress", po.BuyerAddress);
            masterParameters.Add("@CurrencyCode", po.CurrencyCode);
            masterParameters.Add("@InquiryCode", po.InquiryCode);
            masterParameters.Add("@IsVerbalOrder", po.IsVerbalOrder);
            masterParameters.Add("@ProjectCode", po.ProjectCode);
            masterParameters.Add("@ProjectName", po.ProjectName);

            poCode = await connection.ExecuteScalarAsync<int>(
                "ERP_CreateCustomerPo",
                masterParameters,
                transaction,
                commandType: CommandType.StoredProcedure
            );

            // 2. Create Details
            foreach (var detail in details)
            {
                var detailParameters = new DynamicParameters();
                detailParameters.Add("@PoCode", poCode);
                detailParameters.Add("@ItemCode", detail.ItemCode);
                detailParameters.Add("@UomCode", detail.UomCode);
                detailParameters.Add("@OrderedQuantity", detail.OrderedQuantity);
                detailParameters.Add("@Rate", detail.Rate);
                detailParameters.Add("@Amount", detail.Amount);
                detailParameters.Add("@Description", detail.Description);
                detailParameters.Add("@CustomerItemCode", detail.CustomerItemCode);
                detailParameters.Add("@CustomerItemName", detail.CustomerItemName);
                detailParameters.Add("@Status", detail.Status);
                detailParameters.Add("@DispatchedQuantity", detail.DispatchedQuantity);
                detailParameters.Add("@IsOrder", detail.IsOrder);
                detailParameters.Add("@StoreCode", detail.StoreCode);
                detailParameters.Add("@CurrencyCode", detail.CurrencyCode);
                detailParameters.Add("@WorkOrderQuantity", detail.WorkOrderQuantity);
                detailParameters.Add("@ModificationNumber", detail.ModificationNumber);
                detailParameters.Add("@ModificationDate", detail.ModificationDate);
                detailParameters.Add("@AmortizationRate", detail.AmortizationRate);
                detailParameters.Add("@DieAmortizationRate", detail.DieAmortizationRate);
                detailParameters.Add("@DiscountPercentage", detail.DiscountPercentage);
                detailParameters.Add("@DiscountAmount", detail.DiscountAmount);

                await connection.ExecuteAsync(
                    "ERP_CreateCustomerPoDetail",
                    detailParameters,
                    transaction,
                    commandType: CommandType.StoredProcedure
                );
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        // 3. Return created PO (after transaction is completed and connection closed)
        return await GetByIdAsync(poCode, po.CompanyId);
    }

    public async Task<CustomerPoMasterDto> UpdateAsync(CustomerPoMasterDto po, IEnumerable<CustomerPoDetailDto> details)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1. Check if locked
            var isLocked = await connection.ExecuteScalarAsync<bool>(
                "ERP_CheckCustomerPoLock",
                new { PoCode = po.PoCode },
                transaction,
                commandType: CommandType.StoredProcedure
            );

            if (isLocked)
            {
                throw new InvalidOperationException($"Customer PO {po.PoCode} is currently locked for editing.");
            }

            // 2. Delete existing details
            await connection.ExecuteAsync(
                "ERP_DeleteCustomerPoDetails",
                new { PoCode = po.PoCode },
                transaction,
                commandType: CommandType.StoredProcedure
            );

            // 3. Update Master
            var masterParameters = new DynamicParameters();
            masterParameters.Add("@PoCode", po.PoCode);
            masterParameters.Add("@CustomerCode", po.CustomerCode);
            masterParameters.Add("@PoNumber", po.PoNumber);
            masterParameters.Add("@PoType", po.PoType);
            masterParameters.Add("@PoDate", po.PoDate);
            masterParameters.Add("@CreditDays", po.CreditDays);
            masterParameters.Add("@CompanyId", po.CompanyId);
            masterParameters.Add("@WorkOrderNumber", po.WorkOrderNumber);
            masterParameters.Add("@PaymentTerms", po.PaymentTerms);
            masterParameters.Add("@IsAuthorized", po.IsAuthorized);
            masterParameters.Add("@CustomerPoDate", po.CustomerPoDate);
            masterParameters.Add("@QuotationCode", po.QuotationCode);
            masterParameters.Add("@TaxName", po.TaxName);
            masterParameters.Add("@TaxPercentage", po.TaxPercentage);
            masterParameters.Add("@TaxAmount", po.TaxAmount);
            masterParameters.Add("@ExcisePercentage", po.ExcisePercentage);
            masterParameters.Add("@ExciseEducationPercentage", po.ExciseEducationPercentage);
            masterParameters.Add("@ExciseHigherEducationPercentage", po.ExciseHigherEducationPercentage);
            masterParameters.Add("@BasicAmount", po.BasicAmount);
            masterParameters.Add("@DiscountPercentage", po.DiscountPercentage);
            masterParameters.Add("@DiscountAmount", po.DiscountAmount);
            masterParameters.Add("@DiscountReason", po.DiscountReason);
            masterParameters.Add("@DeviationAmount", po.DeviationAmount);
            masterParameters.Add("@DeviationReason", po.DeviationReason);
            masterParameters.Add("@PackingAmount", po.PackingAmount);
            masterParameters.Add("@ExciseAmount", po.ExciseAmount);
            masterParameters.Add("@RoundingAmount", po.RoundingAmount);
            masterParameters.Add("@GrandTotal", po.GrandTotal);
            masterParameters.Add("@FinalDestination", po.FinalDestination);
            masterParameters.Add("@PreCarriageBy", po.PreCarriageBy);
            masterParameters.Add("@PortOfLoading", po.PortOfLoading);
            masterParameters.Add("@PortOfDischarge", po.PortOfDischarge);
            masterParameters.Add("@PlaceOfDelivery", po.PlaceOfDelivery);
            masterParameters.Add("@BuyerName", po.BuyerName);
            masterParameters.Add("@BuyerAddress", po.BuyerAddress);
            masterParameters.Add("@CurrencyCode", po.CurrencyCode);
            masterParameters.Add("@InquiryCode", po.InquiryCode);
            masterParameters.Add("@IsVerbalOrder", po.IsVerbalOrder);
            masterParameters.Add("@ProjectCode", po.ProjectCode);
            masterParameters.Add("@ProjectName", po.ProjectName);

            await connection.ExecuteAsync(
                "ERP_UpdateCustomerPo",
                masterParameters,
                transaction,
                commandType: CommandType.StoredProcedure
            );

            // 4. Insert new details
            foreach (var detail in details)
            {
                var detailParameters = new DynamicParameters();
                detailParameters.Add("@PoCode", po.PoCode);
                detailParameters.Add("@ItemCode", detail.ItemCode);
                detailParameters.Add("@UomCode", detail.UomCode);
                detailParameters.Add("@OrderedQuantity", detail.OrderedQuantity);
                detailParameters.Add("@Rate", detail.Rate);
                detailParameters.Add("@Amount", detail.Amount);
                detailParameters.Add("@Description", detail.Description);
                detailParameters.Add("@CustomerItemCode", detail.CustomerItemCode);
                detailParameters.Add("@CustomerItemName", detail.CustomerItemName);
                detailParameters.Add("@Status", detail.Status);
                detailParameters.Add("@DispatchedQuantity", detail.DispatchedQuantity);
                detailParameters.Add("@IsOrder", detail.IsOrder);
                detailParameters.Add("@StoreCode", detail.StoreCode);
                detailParameters.Add("@CurrencyCode", detail.CurrencyCode);
                detailParameters.Add("@WorkOrderQuantity", detail.WorkOrderQuantity);
                detailParameters.Add("@ModificationNumber", detail.ModificationNumber);
                detailParameters.Add("@ModificationDate", detail.ModificationDate);
                detailParameters.Add("@AmortizationRate", detail.AmortizationRate);
                detailParameters.Add("@DieAmortizationRate", detail.DieAmortizationRate);
                detailParameters.Add("@DiscountPercentage", detail.DiscountPercentage);
                detailParameters.Add("@DiscountAmount", detail.DiscountAmount);

                await connection.ExecuteAsync(
                    "ERP_CreateCustomerPoDetail",
                    detailParameters,
                    transaction,
                    commandType: CommandType.StoredProcedure
                );
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        // 5. Return updated PO (after transaction is completed and connection closed)
        return await GetByIdAsync(po.PoCode, po.CompanyId);
    }

    public async Task<bool> DeleteAsync(int poCode, int companyId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var result = await connection.ExecuteAsync(
            "ERP_DeleteCustomerPo",
            new { PoCode = poCode, CompanyId = companyId },
            commandType: CommandType.StoredProcedure
        );

        return result > 0;
    }

    public async Task<CustomerPoMasterDto?> GetByIdAsync(int poCode, int companyId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var parameters = new DynamicParameters();
        parameters.Add("@PoCode", poCode);
        parameters.Add("@CompanyId", companyId);

        using var multi = await connection.QueryMultipleAsync(
            "ERP_GetCustomerPoById",
            parameters,
            commandType: CommandType.StoredProcedure
        );

        var master = await multi.ReadFirstOrDefaultAsync<CustomerPoMasterDto>();
        if (master != null)
        {
            var details = (await multi.ReadAsync<CustomerPoDetailDto>()).ToList();
            master.Details = details;
        }

        return master;
    }

    public async Task<(IEnumerable<CustomerPoMasterDto> Data, int TotalCount)> GetAllAsync(CustomerPoQueryParameters parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var spParameters = new DynamicParameters();
        spParameters.Add("@CompanyId", parameters.CompanyId);
        spParameters.Add("@PageNumber", parameters.PageNumber);
        spParameters.Add("@PageSize", parameters.PageSize);
        spParameters.Add("@IsActive", parameters.IsActive);
        spParameters.Add("@SearchTerm", parameters.SearchTerm);
        spParameters.Add("@PoNumber", parameters.PoNumber);
        spParameters.Add("@CustomerCode", parameters.CustomerCode);
        spParameters.Add("@CustomerName", parameters.CustomerName);
        spParameters.Add("@FromDate", parameters.FromDate);
        spParameters.Add("@ToDate", parameters.ToDate);
        spParameters.Add("@WorkOrderNumber", parameters.WorkOrderNumber);
        spParameters.Add("@CustomerItemCode", parameters.CustomerItemCode);
        spParameters.Add("@PoType", parameters.PoType);
        spParameters.Add("@ProjectCode", parameters.ProjectCode);
        spParameters.Add("@InvoiceGenerated", parameters.InvoiceGenerated);
        spParameters.Add("@HasAmendment", parameters.HasAmendment);
        spParameters.Add("@IsVerbalOrder", parameters.IsVerbalOrder);
        spParameters.Add("@SortBy", parameters.SortBy);
        spParameters.Add("@SortOrder", parameters.SortOrder);

        using var multi = await connection.QueryMultipleAsync(
            "ERP_GetAllCustomerPos",
            spParameters,
            commandType: CommandType.StoredProcedure
        );

        var data = (await multi.ReadAsync<CustomerPoMasterDto>()).ToList();
        var totalCount = await multi.ReadSingleAsync<int>();

        return (data, totalCount);
    }

    public async Task<bool> IsLockedAsync(int poCode)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        return await connection.ExecuteScalarAsync<bool>(
            "ERP_CheckCustomerPoLock",
            new { PoCode = poCode },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task LockAsync(int poCode)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            "ERP_LockCustomerPo",
            new { PoCode = poCode },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task UnlockAsync(int poCode)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            "ERP_UnlockCustomerPo",
            new { PoCode = poCode },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<CustomerPoPrintDto?> GetPrintDataAsync(int poCode, int companyId, int companyCode, PoCopyType copyType)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var multi = await connection.QueryMultipleAsync(
            "ERP_GetCustomerPoPrintData",
            new { PoCode = poCode, CompanyId = companyId, CompanyCode = companyCode },
            commandType: CommandType.StoredProcedure
        );

        // Read Result Set 1: Company Info
        var company = await multi.ReadFirstOrDefaultAsync<CompanyPrintInfo>();
        if (company == null)
            return null;

        // Read Result Set 2: PO Header (with extended fields)
        var poHeaderExtended = await multi.ReadFirstOrDefaultAsync<PoHeaderExtendedData>();
        if (poHeaderExtended == null)
            return null;

        // Read Result Set 3: Customer Info
        var customer = await multi.ReadFirstOrDefaultAsync<CustomerPrintInfo>();
        if (customer == null)
            return null;

        // Read Result Set 4: Line Items
        var lineItems = (await multi.ReadAsync<PoDetailPrintInfo>()).ToList();

        // Read Result Set 5: Totals
        var totals = await multi.ReadFirstOrDefaultAsync<PoTotalsPrintInfo>();
        if (totals == null)
            return null;

        // Convert total amount to words
        totals.AmountInWords = NumberToWordsConverter.ConvertToWords((double)totals.TotalAmount);

        // Build and return the print DTO
        return new CustomerPoPrintDto
        {
            Company = company,
            PoHeader = new PoHeaderPrintInfo
            {
                SaleOrderNo = poHeaderExtended.SaleOrderNo,
                SaleOrderDate = poHeaderExtended.SaleOrderDate,
                PoNo = poHeaderExtended.PoNo,
                PoDate = poHeaderExtended.PoDate
            },
            Customer = customer,
            LineItems = lineItems,
            Totals = totals,
            CopyType = copyType,
            Consignee = poHeaderExtended.Consignee,
            TransportThrough = poHeaderExtended.TransportThrough,
            DeliveryTerms = poHeaderExtended.DeliveryTerms,
            Narrations = poHeaderExtended.Narrations
        };
    }
    
    // Helper class to read PO header with additional fields from stored procedure
    private class PoHeaderExtendedData : PoHeaderPrintInfo
    {
        public string? Consignee { get; set; }
        public string? TransportThrough { get; set; }
        public string? DeliveryTerms { get; set; }
        public string? Narrations { get; set; }
    }
}

