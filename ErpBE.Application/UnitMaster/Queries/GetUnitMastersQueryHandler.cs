using MediatR;
using ErpBE.Domain.Interfaces;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.CommonDto;

namespace ErpBE.Application.UnitMaster.Queries
{
    public class GetUnitMastersQueryHandler : IRequestHandler<GetUnitMastersQuery, PagedResponse<UnitMasterDto>>
    {
        private readonly IUnitMasterService _unitMasterService;

        public GetUnitMastersQueryHandler(IUnitMasterService unitMasterService)
        {
            _unitMasterService = unitMasterService;
        }

        public async Task<PagedResponse<UnitMasterDto>> Handle(GetUnitMastersQuery request, CancellationToken cancellationToken)
        {
            return await _unitMasterService.GetUnitMastersAsync(request.QueryParameters);
        }
    }
}
