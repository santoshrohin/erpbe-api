using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Queries
{
    public class GetSoTypeMastersQuery : IRequest<PagedResponse<SoTypeMasterDto>>
    {
        public SoTypeMasterQueryParameters QueryParameters { get; set; } = null!;
    }
}

