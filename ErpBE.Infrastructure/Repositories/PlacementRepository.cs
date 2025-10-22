using Dapper;
using ErpBE.Domain.CommonDto;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using System.Data;

namespace ErpBE.Infrastructure.Repositories
{
    public class PlacementRepository : IPlacementRepository
    {
        private readonly IDbConnection _db;

        public PlacementRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<int> CreatePlacementAsync(PlacementDto placement, List<FarmerItemCalculatedDTO> details)
        {
            var detailTable = new DataTable();
            detailTable.Columns.Add("ItemId", typeof(int)); // <- You might need this
            detailTable.Columns.Add("ScheduledDate", typeof(DateTime));
            detailTable.Columns.Add("FeedQtyPercentage", typeof(decimal));
            detailTable.Columns.Add("FeedQty", typeof(int));
            detailTable.Columns.Add("OffsetDays", typeof(int));
            detailTable.Columns.Add("BagsRequired", typeof(decimal));

            foreach (var d in details)
            {
                detailTable.Rows.Add(
                    d.ItemId, // fallback if ItemId is null
                    d.ScheduledDate,
                    d.FeedPercent,
                    d.FeedQty,
                    d.OffsetDays,
                    d.BagsRequired
                );
            }

            var p = new DynamicParameters();
            p.Add("@EntryDate", DateTime.UtcNow);
            p.Add("@FarmerId", placement.FarmerId);
            p.Add("@FarmerName", placement.FarmerName);
            p.Add("@FarmerCode", placement.FarmerCode);
            p.Add("@FarmerAddress", placement.FarmerAddress);
            p.Add("@BranchId", placement.BranchId);
            p.Add("@LineId", placement.LineId);
            p.Add("@PlacementQty", placement.PlacementQty);
            p.Add("@PlacementDate", placement.PlacementDate);
            p.Add("@Status", true);
            p.Add("@PlacementDetails", detailTable.AsTableValuedParameter("PlacementDetailType"));

            return await _db.ExecuteScalarAsync<int>("SP_InsertFullPlacement", p, commandType: CommandType.StoredProcedure);
        }

        public async Task<PagedResponse<PlacementWithDetailsDto>> GetPagedAsync(PlacementQueryParameters parameters)
        {
            var p = new DynamicParameters();
            p.Add("@PageNumber", parameters.PageNumber);
            p.Add("@PageSize", parameters.PageSize);
            p.Add("@SortBy", parameters.SortBy ?? "PlacementDate");
            p.Add("@SortDirection", parameters.SortDirection ?? "DESC");
            p.Add("@SearchTerm", parameters.SearchTerm);
            p.Add("@FarmerId", parameters.FarmerId);
            p.Add("@BranchId", parameters.BranchId);
            p.Add("@LineId", parameters.LineId);
            p.Add("@FarmerName", parameters.FarmerName);
            p.Add("@FarmerCode", parameters.FarmerCode);
            p.Add("@PlacementDateFrom", parameters.PlacementDateFrom);
            p.Add("@PlacementDateTo", parameters.PlacementDateTo);
            p.Add("@MinPlacementQty", parameters.MinPlacementQty);
            p.Add("@MaxPlacementQty", parameters.MaxPlacementQty);
            p.Add("@Status", parameters.Status);
            p.Add("@TotalCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var data = await _db.QueryAsync<PlacementWithDetailsDto>(
                "SP_GetPlacementsWithFilters",
                p,
                commandType: CommandType.StoredProcedure);

            var totalCount = p.Get<int>("@TotalCount");

            return new PagedResponse<PlacementWithDetailsDto>(data.ToList(), totalCount, parameters.PageNumber, parameters.PageSize);
        }

        public async Task<IEnumerable<FarmerItemCalculatedDTO>> GetPlacementDetailsAsync(int? placementQty, DateTime? placementDate)
        {
            var parameters = new
            {
                PlacementQty = placementQty,
                PlacementDate = placementDate
            };

            // Execute the stored procedure to get the calculated placement details
            var result = await _db.QueryAsync<FarmerItemCalculatedDTO>(
                "dbo.GetPlacementDetails", // The name of the stored procedure
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<IEnumerable<PlacementWithDetailsDto>> GetAllPlacementsWithDetailsAsync()
        {
            var result = await _db.QueryAsync<PlacementWithDetailsDto>(
                "dbo.GetAllPlacementsWithDetails",
                commandType: CommandType.StoredProcedure
            );
            return result;
        }
    }
}
