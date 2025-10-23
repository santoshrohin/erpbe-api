using MediatR;


namespace ErpBE.Application.UnitMaster.Commands
{
    public class DeleteUnitMasterCommandHandler : IRequestHandler<DeleteUnitMasterCommand, bool>
    {
        private readonly IUnitMasterService _unitMasterService;

        public DeleteUnitMasterCommandHandler(IUnitMasterService unitMasterService)
        {
            _unitMasterService = unitMasterService;
        }

        public async Task<bool> Handle(DeleteUnitMasterCommand request, CancellationToken cancellationToken)
        {
            return await _unitMasterService.DeleteUnitMasterAsync(request.Id);
        }
    }
}
