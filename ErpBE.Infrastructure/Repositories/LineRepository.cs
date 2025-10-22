using Dapper;
using ErpBE.Domain.CommonDto;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using System.Data;

namespace ErpBE.Infrastructure.Repositories
{
    public class LineRepository : ILineRepository
    {
        private readonly IDbConnection _db;

        public LineRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<List<LineDto>> GetAllAsync()
        {
            var result = await _db.QueryAsync<LineDto>(
                "SP_GetAllLines",
                commandType: CommandType.StoredProcedure);

            return result.ToList();
        }

        public async Task<PagedResponse<LineDto>> GetPagedAsync(LineQueryParameters parameters)
        {
            var p = new DynamicParameters();
            p.Add("@PageNumber", parameters.PageNumber);
            p.Add("@PageSize", parameters.PageSize);
            p.Add("@SortBy", parameters.SortBy ?? "LineName");
            p.Add("@SortDirection", parameters.SortDirection ?? "ASC");
            p.Add("@SearchTerm", parameters.SearchTerm);
            p.Add("@LineName", parameters.LineName);
            p.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var data = await _db.QueryAsync<LineDto>(
                "SP_GetLinesWithFilters",
                p,
                commandType: CommandType.StoredProcedure);

            var totalCount = p.Get<int>("@TotalCount");

            return new PagedResponse<LineDto>(data.ToList(), totalCount, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<int> CreateAsync(string lineName)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@LineName", lineName);

            var result = await _db.ExecuteScalarAsync<int>(
                "SP_InsertLine",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result;
        }
    }
}
