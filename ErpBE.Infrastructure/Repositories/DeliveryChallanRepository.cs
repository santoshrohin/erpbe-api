using Dapper;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace ErpBE.Infrastructure.Repositories;

public class DeliveryChallanRepository : IDeliveryChallanRepository
{
    private readonly string _connectionString;

    public DeliveryChallanRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
    }

    public async Task<(IEnumerable<DeliveryChallanMasterDto> Data, int TotalCount)> GetAllAsync(
        DeliveryChallanQueryParameters parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync(
            "ERP_GetAllDeliveryChallans",
            new
            {
                CompanyCode  = parameters.CompanyCode,
                CustomerCode = parameters.CustomerCode,
                DateFrom     = parameters.DateFrom,
                DateTo       = parameters.DateTo,
                SearchText   = parameters.SearchText,
                PageNumber   = parameters.PageNumber,
                PageSize     = parameters.PageSize,
                SortBy       = "ChallanDate",
                SortDir      = "DESC"
            },
            commandType: System.Data.CommandType.StoredProcedure);

        var data       = await multi.ReadAsync<DeliveryChallanMasterDto>();
        var totalCount = await multi.ReadFirstOrDefaultAsync<int>();
        return (data, totalCount);
    }

    public async Task<DeliveryChallanMasterDto?> GetByIdAsync(int challanCode, int companyCode)
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync(
            "ERP_GetDeliveryChallanById",
            new { ChallanCode = challanCode, CompanyCode = companyCode },
            commandType: System.Data.CommandType.StoredProcedure);

        var master = await multi.ReadFirstOrDefaultAsync<DeliveryChallanMasterDto>();
        if (master == null) return null;

        master.Details = (await multi.ReadAsync<DeliveryChallanDetailDto>()).ToList();
        return master;
    }

    public async Task<DeliveryChallanMasterDto> CreateAsync(CreateDeliveryChallanRequest request)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var tx = connection.BeginTransaction();

        try
        {
            var row = await connection.QueryFirstAsync(
                "ERP_CreateDeliveryChallan",
                new
                {
                    request.CompanyCode,
                    request.CustomerCode,
                    request.Type,
                    request.ChallanDate,
                    request.InvoiceNumber,
                    request.Through,
                    request.VehicleNumber,
                    request.LrNumber,
                    request.OrderNumber,
                    request.OrderDate,
                    request.MaterialType,
                    request.IsReturnable
                },
                transaction:  tx,
                commandType:  System.Data.CommandType.StoredProcedure);

            int     challanCode   = (int)row.ChallanCode;
            decimal challanNumber = (decimal)row.ChallanNumber;

            foreach (var d in request.Details)
            {
                await connection.ExecuteAsync(
                    "ERP_CreateDeliveryChallanDetail",
                    new
                    {
                        ChallanCode     = challanCode,
                        d.ItemCode,
                        d.OrderedQuantity,
                        d.BatchNumber,
                        d.NumberOfPacks,
                        d.UomCode,
                        d.Remark
                    },
                    transaction: tx,
                    commandType: System.Data.CommandType.StoredProcedure);
            }

            await tx.CommitAsync();

            return await GetByIdAsync(challanCode, request.CompanyCode)
                ?? throw new InvalidOperationException("Failed to retrieve created Delivery Challan.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<DeliveryChallanMasterDto> UpdateAsync(UpdateDeliveryChallanRequest request)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var tx = connection.BeginTransaction();

        try
        {
            await connection.ExecuteAsync(
                "ERP_UpdateDeliveryChallan",
                new
                {
                    request.ChallanCode,
                    request.CompanyCode,
                    request.CustomerCode,
                    request.Type,
                    request.ChallanDate,
                    request.InvoiceNumber,
                    request.Through,
                    request.VehicleNumber,
                    request.LrNumber,
                    request.OrderNumber,
                    request.OrderDate,
                    request.MaterialType,
                    request.IsReturnable
                },
                transaction: tx,
                commandType: System.Data.CommandType.StoredProcedure);

            foreach (var d in request.Details)
            {
                await connection.ExecuteAsync(
                    "ERP_CreateDeliveryChallanDetail",
                    new
                    {
                        ChallanCode     = request.ChallanCode,
                        d.ItemCode,
                        d.OrderedQuantity,
                        d.BatchNumber,
                        d.NumberOfPacks,
                        d.UomCode,
                        d.Remark
                    },
                    transaction: tx,
                    commandType: System.Data.CommandType.StoredProcedure);
            }

            await tx.CommitAsync();

            return await GetByIdAsync(request.ChallanCode, request.CompanyCode)
                ?? throw new InvalidOperationException("Failed to retrieve updated Delivery Challan.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int challanCode, int companyCode)
    {
        using var connection = new SqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            "ERP_DeleteDeliveryChallan",
            new { ChallanCode = challanCode, CompanyCode = companyCode },
            commandType: System.Data.CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<bool> LockAsync(int challanCode, int companyCode, int lockedByUserId)
    {
        using var connection = new SqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            "ERP_LockDeliveryChallan",
            new { ChallanCode = challanCode, CompanyCode = companyCode, LockedByUserId = lockedByUserId },
            commandType: System.Data.CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<bool> UnlockAsync(int challanCode, int companyCode)
    {
        using var connection = new SqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            "ERP_UnlockDeliveryChallan",
            new { ChallanCode = challanCode, CompanyCode = companyCode },
            commandType: System.Data.CommandType.StoredProcedure);
        return rowsAffected > 0;
    }

    public async Task<DeliveryChallanPrintDto?> GetPrintDataAsync(int challanCode, int companyCode)
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync(
            "ERP_GetDeliveryChallanPrintData",
            new { ChallanCode = challanCode, CompanyCode = companyCode },
            commandType: System.Data.CommandType.StoredProcedure);

        var master = await multi.ReadFirstOrDefaultAsync<DeliveryChallanPrintDto>();
        if (master == null) return null;

        master.LineItems = (await multi.ReadAsync<DcDetailPrintInfo>()).ToList();
        return master;
    }
}
