using Dapper;
using ErpBE.Domain.CommonDto;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using System.Data;

namespace ErpBE.Infrastructure.Repositories
{
    public class BranchRepository : IBranchRepository
    {
        private readonly IDbConnection _db;

        public BranchRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<List<BranchDto>> GetAllAsync()
        {
            var result = await _db.QueryAsync<BranchDto>(
                "SP_GetAllBranches",
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<PagedResponse<BranchDto>> GetPagedAsync(BranchQueryParameters parameters)
        {
            var p = new DynamicParameters();
            p.Add("@PageNumber", parameters.PageNumber);
            p.Add("@PageSize", parameters.PageSize);
            p.Add("@SortBy", parameters.SortBy ?? "BranchName");
            p.Add("@SortDirection", parameters.SortDirection ?? "ASC");
            p.Add("@SearchTerm", parameters.SearchTerm);
            p.Add("@BranchName", parameters.BranchName);
            p.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var data = await _db.QueryAsync<BranchDto>(
                "SP_GetBranchesWithFilters",
                p,
                commandType: CommandType.StoredProcedure);

            var totalCount = p.Get<int>("@TotalCount");

            return new PagedResponse<BranchDto>(data.ToList(), totalCount, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<int> CreateAsync(string branchName)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@BranchName", branchName);

            var result = await _db.ExecuteScalarAsync<int>(
                "SP_InsertBranch",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result;
        }
    }
}
