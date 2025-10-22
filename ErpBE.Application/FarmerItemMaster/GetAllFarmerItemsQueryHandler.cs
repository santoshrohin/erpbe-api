using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.FarmerItemMaster
{
    public class GetAllFarmerItemsQueryHandler : IRequestHandler<GetAllFarmerItemsQuery, List<FarmerItemDto>>
    {
        private readonly IFarmerItemRepository _repository;

        public GetAllFarmerItemsQueryHandler(IFarmerItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<FarmerItemDto>> Handle(GetAllFarmerItemsQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllAsync();
        }
    }

}
