using ErpBE.Domain.DTOs;
using ErpBE.Domain.Interfaces;
using MediatR;

namespace ErpBE.Application.FarmerItemMaster
{
    public class CreateFarmerItemCommandHandler : IRequestHandler<CreateFarmerItemCommand, int>
    {
        private readonly IFarmerItemRepository _repository;

        public CreateFarmerItemCommandHandler(IFarmerItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateFarmerItemCommand request, CancellationToken cancellationToken)
        {
            var dto = new FarmerItemDto
            {
                ItemType = request.ItemType,
                ItemName = request.ItemName,
                FeedPercent = request.FeedPercent,
                OffsetDays = request.OffsetDays,
                StandardBagSize = request.StandardBagSize
            };

            return await _repository.CreateAsync(dto);
        }
    }

}
