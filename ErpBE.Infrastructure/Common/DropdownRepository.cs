using Dapper;
using ErpBE.Application.Interfaces;
using ErpBE.Application.Common.Models;
using System.Data;

namespace ErpBE.Infrastructure.Common
{
    public class DropdownRepository : IDropdownRepository
    {
        private readonly IDbConnection _db;

        public DropdownRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<List<DropdownItem>> GetDropdownDataAsync(DropdownRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Table", request.Table);
            parameters.Add("@IdColumn", request.IdColumn);
            parameters.Add("@DisplayColumn", request.DisplayColumn);
            parameters.Add("@WhereClause", request.Where);
            parameters.Add("@OrderBy", request.OrderBy);

            var result = await _db.QueryAsync<DropdownItem>(
                "usp_GetDropdownData",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }
    }

}
