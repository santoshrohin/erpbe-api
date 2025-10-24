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
            
            if (category == null)
            {
                throw new KeyNotFoundException($"Item Category with ID '{request.CategoryId}' not found.");
            }
            
            return category;
        }
    }
}



