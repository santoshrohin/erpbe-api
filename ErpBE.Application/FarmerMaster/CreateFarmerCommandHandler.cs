using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.FarmerMaster
{
    public class CreateFarmerCommandHandler : IRequestHandler<CreateFarmerCommand, int>
    {
        private readonly IFarmerRepository _repository;

        public CreateFarmerCommandHandler(IFarmerRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateFarmerCommand request, CancellationToken cancellationToken)
        {
            var dto = new FarmerDto
            {
                FarmerName = request.FarmerName,
                FarmerCode = request.FarmerCode,
                FarmerAddress = request.FarmerAddress,
                BranchId = request.BranchId,
                LineId = request.LineId
            };

            return await _repository.CreateAsync(dto);
        }
    }

}
