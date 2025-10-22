using ErpBE.Domain.CommonDto;
using ErpBE.Domain.DTOs;
using MediatR;

namespace ErpBE.Application.FarmerMaster.Queries
{
    public class GetFarmersWithFiltersQuery : IRequest<PagedResponse<FarmerDto>>
    {
        public FarmerQueryParameters Parameters { get; }

        public GetFarmersWithFiltersQuery(FarmerQueryParameters parameters)
        {
            Parameters = parameters;
        }
    }
}
