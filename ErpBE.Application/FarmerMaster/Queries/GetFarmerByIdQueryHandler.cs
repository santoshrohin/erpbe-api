using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.FarmerMaster.Queries
{
    public class GetFarmerByIdQueryHandler : IRequestHandler<GetFarmerByIdQuery, FarmerDto?>
    {
        private readonly IFarmerRepository _repository;

        public GetFarmerByIdQueryHandler(IFarmerRepository repository)
        {
            _repository = repository;
        }

        public async Task<FarmerDto?> Handle(GetFarmerByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetByIdAsync(request.Id);
        }
    }

}
