using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Queries
{
    public class GetItemCategoryMasterByNameQueryHandler : IRequestHandler<GetItemCategoryMasterByNameQuery, ItemCategoryMasterDto?>
    {
        private readonly IItemCategoryMasterRepository _repository;

        public GetItemCategoryMasterByNameQueryHandler(IItemCategoryMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<ItemCategoryMasterDto?> Handle(GetItemCategoryMasterByNameQuery request, CancellationToken cancellationToken)
        {
            var category = await _repository.GetByNameAsync(request.CategoryName, request.CompanyId);
            
            if (category == null)
            {
                throw new KeyNotFoundException($"Item Category with name '{request.CategoryName}' not found for company {request.CompanyId}.");
            }
            
            return category;
        }
    }
}



