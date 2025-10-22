using MediatR;
using ErpBE.Domain.Interfaces;
using ErpBE.Domain.DTOs;

namespace ErpBE.Application.UnitMaster.Commands
{
    public class CreateUnitMasterCommandHandler : IRequestHandler<CreateUnitMasterCommand, int>
    {
        private readonly IUnitMasterService _unitMasterService;

        public CreateUnitMasterCommandHandler(IUnitMasterService unitMasterService)
        {
            _unitMasterService = unitMasterService;
        }

        public async Task<int> Handle(CreateUnitMasterCommand request, CancellationToken cancellationToken)
        {
            return await _unitMasterService.CreateUnitMasterAsync(request.Request);
        }
    }
}
