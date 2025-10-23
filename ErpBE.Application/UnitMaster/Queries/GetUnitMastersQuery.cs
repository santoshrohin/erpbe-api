using MediatR;
using ErpBE.Application.DTOs;
using ErpBE.Application.Common.Models;

namespace ErpBE.Application.UnitMaster.Queries
{
    public class GetUnitMastersQuery : IRequest<PagedResponse<UnitMasterDto>>
    {
        public UnitMasterQueryParameters QueryParameters { get; set; } = new();
    }
}
