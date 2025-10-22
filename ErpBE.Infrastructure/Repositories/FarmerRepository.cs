using Dapper;
using ErpBE.Domain.CommonDto;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using System.Data;

namespace ErpBE.Infrastructure.Repositories
{
    public class FarmerRepository : IFarmerRepository
    {
        private readonly IDbConnection _db;

        public FarmerRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<List<FarmerDto>> GetAllAsync()
        {
            var result = await _db.QueryAsync<FarmerDto>(
                "SP_GetAllFarmers",
                commandType: CommandType.StoredProcedure);
            return result.ToList();
        }

        public async Task<PagedResponse<FarmerDto>> GetPagedAsync(FarmerQueryParameters parameters)
        {
            var p = new DynamicParameters();
            p.Add("@PageNumber", parameters.PageNumber);
            p.Add("@PageSize", parameters.PageSize);
            p.Add("@SortBy", parameters.SortBy ?? "FarmerName");
            p.Add("@SortDirection", parameters.SortDirection ?? "ASC");
            p.Add("@SearchTerm", parameters.SearchTerm);
            p.Add("@BranchId", parameters.BranchId);
            p.Add("@LineId", parameters.LineId);
            p.Add("@FarmerCode", parameters.FarmerCode);
            p.Add("@FarmerName", parameters.FarmerName);
            p.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var data = await _db.QueryAsync<FarmerDto>(
                "SP_GetFarmersWithFilters",
                p,
                commandType: CommandType.StoredProcedure);

            var totalCount = p.Get<int>("@TotalCount");

            return new PagedResponse<FarmerDto>(data.ToList(), totalCount, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<int> CreateAsync(FarmerDto farmer)
        {
            var p = new DynamicParameters();
            p.Add("@FarmerName", farmer.FarmerName);
            p.Add("@FarmerCode", farmer.FarmerCode);
            p.Add("@FarmerAddress", farmer.FarmerAddress);
            p.Add("@BranchId", farmer.BranchId);
            p.Add("@LineId", farmer.LineId);

            return await _db.ExecuteScalarAsync<int>("SP_InsertFarmer", p, commandType: CommandType.StoredProcedure);
        }

        public async Task<FarmerDto?> GetByIdAsync(int id)
        {
            var result = await _db.QueryFirstOrDefaultAsync<FarmerDto>(
                "SP_GetFarmerById",
                new { Id = id },
                commandType: CommandType.StoredProcedure);

            return result;
        }
    }
}
