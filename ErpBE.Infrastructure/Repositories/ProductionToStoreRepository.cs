using Dapper;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ErpBE.Infrastructure.Repositories;

public class ProductionToStoreRepository : IProductionToStoreRepository
{
    private readonly string _connectionString;

    public ProductionToStoreRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
    }

    public async Task<(IEnumerable<ProductionToStoreMasterDto> Data, int TotalCount)> GetAllAsync(
        ProductionToStoreQueryParameters parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync(
            "ERP_GetAllProductionToStore",
            new
            {
                CompanyCode = parameters.CompanyCode,
                SearchText  = parameters.SearchText,
                DateFrom    = parameters.DateFrom,
                DateTo      = parameters.DateTo,
                PageNumber  = parameters.PageNumber,
                PageSize    = parameters.PageSize
            },
            commandType: System.Data.CommandType.StoredProcedure);

        var data       = await multi.ReadAsync<ProductionToStoreMasterDto>();
        var totalCount = await multi.ReadFirstOrDefaultAsync<int>();
        return (data, totalCount);
    }

    public async Task<ProductionToStoreMasterDto?> GetByIdAsync(int productionCode, int companyCode)
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync(
            "ERP_GetProductionToStoreById",
            new { ProductionCode = productionCode, CompanyCode = companyCode },
            commandType: System.Data.CommandType.StoredProcedure);

        var master = await multi.ReadFirstOrDefaultAsync<ProductionToStoreMasterDto>();
        if (master == null) return null;

        master.Details = (await multi.ReadAsync<ProductionToStoreDetailDto>()).ToList();
        return master;
    }

    public async Task<ProductionToStoreMasterDto> CreateAsync(CreateProductionToStoreRequest request)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var tx = connection.BeginTransaction();

        try
        {
            var row = await connection.QueryFirstAsync(
                "ERP_CreateProductionToStore",
                new
                {
                    request.CompanyCode,
                    request.GinDate,
                    request.Type,
                    request.PersonName,
                    request.MrCode,
                    request.CustomerCode,
                    request.BatchNo
                },
                transaction: tx,
                commandType: System.Data.CommandType.StoredProcedure);

            int     productionCode = (int)row.ProductionCode;
            decimal ginNumber      = (decimal)row.GinNumber;

            foreach (var d in request.Details)
            {
                await connection.ExecuteAsync(
                    "ERP_CreateProductionToStoreDetail",
                    new
                    {
                        ProductionCode = productionCode,
                        d.ItemCode,
                        d.Quantity,
                        d.Remark
                    },
                    transaction: tx,
                    commandType: System.Data.CommandType.StoredProcedure);
            }

            await tx.CommitAsync();

            return await GetByIdAsync(productionCode, request.CompanyCode)
                ?? throw new InvalidOperationException("Failed to retrieve created Production To Store record.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<ProductionToStoreMasterDto> UpdateAsync(UpdateProductionToStoreRequest request)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var tx = connection.BeginTransaction();

        try
        {
            await connection.ExecuteAsync(
                "ERP_UpdateProductionToStore",
                new
                {
                    request.ProductionCode,
                    request.CompanyCode,
                    request.GinDate,
                    request.Type,
                    request.PersonName,
                    request.MrCode,
                    request.CustomerCode,
                    request.BatchNo
                },
                transaction: tx,
                commandType: System.Data.CommandType.StoredProcedure);

            foreach (var d in request.Details)
            {
                await connection.ExecuteAsync(
                    "ERP_CreateProductionToStoreDetail",
                    new
                    {
                        ProductionCode = request.ProductionCode,
                        d.ItemCode,
                        d.Quantity,
                        d.Remark
                    },
                    transaction: tx,
                    commandType: System.Data.CommandType.StoredProcedure);
            }

            await tx.CommitAsync();

            return await GetByIdAsync(request.ProductionCode, request.CompanyCode)
                ?? throw new InvalidOperationException("Failed to retrieve updated Production To Store record.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int productionCode, int companyCode)
    {
        using var connection = new SqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            "ERP_DeleteProductionToStore",
            new { ProductionCode = productionCode, CompanyCode = companyCode },
            commandType: System.Data.CommandType.StoredProcedure);
        return rowsAffected > 0;
    }
}
