using MediatR;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.CommonDto;

namespace ErpBE.Application.UnitMaster.Queries
{
    public class GetUnitMastersQuery : IRequest<PagedResponse<UnitMasterDto>>
    {
        public UnitMasterQueryParameters QueryParameters { get; set; } = new();
    }
}
