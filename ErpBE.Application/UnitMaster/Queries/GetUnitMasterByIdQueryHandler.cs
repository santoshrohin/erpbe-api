using MediatR;
using ErpBE.Domain.Interfaces;
using ErpBE.Domain.DTOs;

namespace ErpBE.Application.UnitMaster.Queries
{
    public class GetUnitMasterByIdQueryHandler : IRequestHandler<GetUnitMasterByIdQuery, UnitMasterDto?>
    {
        private readonly IUnitMasterService _unitMasterService;

        public GetUnitMasterByIdQueryHandler(IUnitMasterService unitMasterService)
        {
            _unitMasterService = unitMasterService;
        }

        public async Task<UnitMasterDto?> Handle(GetUnitMasterByIdQuery request, CancellationToken cancellationToken)
        {
            return await _unitMasterService.GetUnitMasterByIdAsync(request.Id);
        }
    }
}
