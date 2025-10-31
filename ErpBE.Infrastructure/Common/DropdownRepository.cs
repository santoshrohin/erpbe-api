using Dapper;
using ErpBE.Application.Interfaces;
using ErpBE.Application.Common.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace ErpBE.Infrastructure.Common
{
    public class DropdownRepository : IDropdownRepository
    {
        private readonly string _connectionString;

        public DropdownRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<List<DropdownItem>> GetDropdownDataAsync(DropdownRequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Table", request.Table);
            parameters.Add("@IdColumn", request.IdColumn);
            parameters.Add("@DisplayColumn", request.DisplayColumn);
            parameters.Add("@WhereClause", request.Where);
            parameters.Add("@OrderBy", request.OrderBy);

            var result = await connection.QueryAsync<DropdownItem>(
                "usp_GetDropdownData",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task<(List<DropdownItem> Items, int? TotalCount)> GetDropdownDataPagedAsync(DropdownRequest request)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Table", request.Table);
            parameters.Add("@IdColumn", request.IdColumn);
            parameters.Add("@DisplayColumn", request.DisplayColumn);
            parameters.Add("@WhereClause", request.Where);
            parameters.Add("@OrderBy", request.OrderBy);
            parameters.Add("@SearchText", request.SearchText);
            parameters.Add("@Skip", request.Skip ?? 0);
            parameters.Add("@Take", request.Take ?? 20);

            using var multi = await connection.QueryMultipleAsync(
                "usp_GetDropdownDataPaged",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var items = (await multi.ReadAsync<DropdownItem>()).ToList();
            int? totalCount = (await multi.ReadAsync<dynamic>()).FirstOrDefault()?.TotalCount;

            return (items, totalCount);
        }

        public async Task<BatchDropdownResponse> GetBatchDropdownsAsync(BatchDropdownRequest batchRequest)
        {
            var result = new BatchDropdownResponse();
            
            // Execute all dropdown requests in parallel for better performance
            // Each task creates its own connection to avoid connection pool exhaustion
            var tasks = batchRequest.Requests.Select(async req =>
            {
                try
                {
                    if ((req.Request.Skip.HasValue && req.Request.Take.HasValue) || !string.IsNullOrEmpty(req.Request.SearchText))
                    {
                        // Use paged/search-enabled SP - creates its own connection
                        var (items, totalCount) = await GetDropdownDataPagedAsync(req.Request);
                        // Optionally include TotalCount for paged dropdowns
                        if (items.Count > 0 && totalCount.HasValue)
                            items[0].TotalCount = totalCount.Value;
                        return new { Key = req.Key, Items = items };
                    }
                    else
                    {
                        // Use classic SP - creates its own connection
                        var items = await GetDropdownDataAsync(req.Request);
                        return new { Key = req.Key, Items = items };
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail entire batch
                    // Return empty items for failed dropdown
                    return new { Key = req.Key, Items = new List<DropdownItem>() };
                }
            });

            var results = await Task.WhenAll(tasks);
            
            // Populate result dictionary
            foreach (var res in results)
            {
                result.Data[res.Key] = res.Items;
            }
            
            return result;
        }
    }

}
