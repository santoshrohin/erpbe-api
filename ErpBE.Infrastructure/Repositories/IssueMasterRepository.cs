using Dapper;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ErpBE.Infrastructure.Repositories;

public class IssueMasterRepository : IIssueMasterRepository
{
    private readonly string _connectionString;

    public IssueMasterRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
    }

    public async Task<(IEnumerable<IssueMasterDto> Data, int TotalCount)> GetAllAsync(
        IssueMasterQueryParameters parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync(
            "ERP_GetAllIssueMaster",
            new
            {
                CompanyCode = parameters.CompanyCode,
                SearchText  = parameters.SearchText,
                DateFrom    = parameters.DateFrom,
                DateTo      = parameters.DateTo,
                PageNumber  = parameters.PageNumber,
                PageSize    = parameters.PageSize
            },
            commandType: CommandType.StoredProcedure);

        var data       = await multi.ReadAsync<IssueMasterDto>();
        var totalCount = await multi.ReadFirstOrDefaultAsync<int>();
        return (data, totalCount);
    }

    public async Task<IssueMasterDto?> GetByIdAsync(int issueCode, int companyCode)
    {
        using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync(
            "ERP_GetIssueMasterById",
            new { IssueCode = issueCode, CompanyCode = companyCode },
            commandType: CommandType.StoredProcedure);

        var master = await multi.ReadFirstOrDefaultAsync<IssueMasterDto>();
        if (master == null) return null;

        master.Details = (await multi.ReadAsync<IssueMasterDetailDto>()).ToList();
        return master;
    }

    public async Task<IssueMasterDto> CreateAsync(CreateIssueMasterRequest request)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var tx = connection.BeginTransaction();

        try
        {
            var row = await connection.QueryFirstAsync(
                "ERP_CreateIssueMaster",
                new
                {
                    request.CompanyCode,
                    request.IssueDate,
                    request.IssueType,
                    request.MaterialReqNo,
                    request.IssuedBy,
                    request.RequestedBy,
                    request.UserMasterCode,
                    request.FromStore
                },
                transaction: tx,
                commandType: CommandType.StoredProcedure);

            int     issueCode   = (int)row.IssueCode;
            decimal issueNumber = (decimal)row.IssueNumber;

            foreach (var d in request.Details)
            {
                await connection.ExecuteAsync(
                    "ERP_CreateIssueMasterDetail",
                    new
                    {
                        IssueCode    = issueCode,
                        CompanyCode  = request.CompanyCode,
                        d.ItemCode,
                        d.UomCode,
                        d.CurrentStock,
                        d.RequestedQty,
                        d.IssuedQty,
                        d.Remark,
                        d.Rate,
                        d.Amount,
                        d.ToStore
                    },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure);
            }

            await tx.CommitAsync();

            return await GetByIdAsync(issueCode, request.CompanyCode)
                ?? throw new InvalidOperationException("Failed to retrieve created Issue Master.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<IssueMasterDto> UpdateAsync(UpdateIssueMasterRequest request)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        using var tx = connection.BeginTransaction();

        try
        {
            await connection.ExecuteAsync(
                "ERP_UpdateIssueMaster",
                new
                {
                    request.IssueCode,
                    request.CompanyCode,
                    request.IssueDate,
                    request.IssueType,
                    request.MaterialReqNo,
                    request.IssuedBy,
                    request.RequestedBy,
                    request.UserMasterCode,
                    request.FromStore
                },
                transaction: tx,
                commandType: CommandType.StoredProcedure);

            foreach (var d in request.Details)
            {
                await connection.ExecuteAsync(
                    "ERP_CreateIssueMasterDetail",
                    new
                    {
                        IssueCode    = request.IssueCode,
                        CompanyCode  = request.CompanyCode,
                        d.ItemCode,
                        d.UomCode,
                        d.CurrentStock,
                        d.RequestedQty,
                        d.IssuedQty,
                        d.Remark,
                        d.Rate,
                        d.Amount,
                        d.ToStore
                    },
                    transaction: tx,
                    commandType: CommandType.StoredProcedure);
            }

            await tx.CommitAsync();

            return await GetByIdAsync(request.IssueCode, request.CompanyCode)
                ?? throw new InvalidOperationException("Failed to retrieve updated Issue Master.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int issueCode, int companyCode)
    {
        using var connection = new SqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            "ERP_DeleteIssueMaster",
            new { IssueCode = issueCode, CompanyCode = companyCode },
            commandType: CommandType.StoredProcedure);
        return rowsAffected > 0;
    }
}
