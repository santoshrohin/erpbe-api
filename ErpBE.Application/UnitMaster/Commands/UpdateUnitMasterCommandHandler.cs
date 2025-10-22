using MediatR;
using ErpBE.Domain.Interfaces;
using ErpBE.Domain.DTOs;

namespace ErpBE.Application.UnitMaster.Commands
{
    public class UpdateUnitMasterCommandHandler : IRequestHandler<UpdateUnitMasterCommand, bool>
    {
        private readonly IUnitMasterService _unitMasterService;

        public UpdateUnitMasterCommandHandler(IUnitMasterService unitMasterService)
        {
            _unitMasterService = unitMasterService;
        }

        public async Task<bool> Handle(UpdateUnitMasterCommand request, CancellationToken cancellationToken)
        {
            return await _unitMasterService.UpdateUnitMasterAsync(request.Request);
        }
    }
}
