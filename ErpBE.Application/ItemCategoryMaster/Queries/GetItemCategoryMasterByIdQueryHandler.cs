using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Queries
{
    public class GetItemCategoryMasterByIdQueryHandler : IRequestHandler<GetItemCategoryMasterByIdQuery, ItemCategoryMasterDto?>
    {
        private readonly IItemCategoryMasterRepository _repository;

        public GetItemCategoryMasterByIdQueryHandler(IItemCategoryMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<ItemCategoryMasterDto?> Handle(GetItemCategoryMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _repository.GetByIdAsync(request.CategoryId);
            
            // Return null if not found instead of throwing exception
            // This allows the caller to handle the case gracefully
            return category;
        }
    }
}



