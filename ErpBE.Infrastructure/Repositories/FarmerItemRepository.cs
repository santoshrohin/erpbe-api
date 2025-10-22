using Dapper;
using ErpBE.Domain.CommonDto;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using System.Data;

namespace ErpBE.Infrastructure.Repositories
{
    public class FarmerItemRepository : IFarmerItemRepository
    {
        private readonly IDbConnection _db;

        public FarmerItemRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<List<FarmerItemDto>> GetAllAsync()
        {
            var result = await _db.QueryAsync<FarmerItemDto>(
                "SP_GetAllFarmerItems",
                commandType: CommandType.StoredProcedure);
            return result.ToList();
        }

        public async Task<PagedResponse<FarmerItemDto>> GetPagedAsync(FarmerItemQueryParameters parameters)
        {
            var p = new DynamicParameters();
            p.Add("@PageNumber", parameters.PageNumber);
            p.Add("@PageSize", parameters.PageSize);
            p.Add("@SortBy", parameters.SortBy ?? "ItemName");
            p.Add("@SortDirection", parameters.SortDirection ?? "ASC");
            p.Add("@SearchTerm", parameters.SearchTerm);
            p.Add("@ItemType", parameters.ItemType);
            p.Add("@ItemName", parameters.ItemName);
            p.Add("@MinFeedPercent", parameters.MinFeedPercent);
            p.Add("@MaxFeedPercent", parameters.MaxFeedPercent);
            p.Add("@MinOffsetDays", parameters.MinOffsetDays);
            p.Add("@MaxOffsetDays", parameters.MaxOffsetDays);
            p.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var data = await _db.QueryAsync<FarmerItemDto>(
                "SP_GetFarmerItemsWithFilters",
                p,
                commandType: CommandType.StoredProcedure);

            var totalCount = p.Get<int>("@TotalCount");

            return new PagedResponse<FarmerItemDto>(data.ToList(), totalCount, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<int> CreateAsync(FarmerItemDto item)
        {
            var p = new DynamicParameters();
            p.Add("@ItemType", item.ItemType);
            p.Add("@ItemName", item.ItemName);
            p.Add("@FeedPercent", item.FeedPercent);
            p.Add("@OffsetDays", item.OffsetDays);
            p.Add("@StandardBagSize", item.StandardBagSize);

            return await _db.ExecuteScalarAsync<int>("SP_InsertFarmerItem", p, commandType: CommandType.StoredProcedure);
        }
    }
}
