using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Queries
{
    public class CheckItemCategoryNameUniqueQueryHandler : IRequestHandler<CheckItemCategoryNameUniqueQuery, bool>
    {
        private readonly IItemCategoryMasterRepository _repository;

        public CheckItemCategoryNameUniqueQueryHandler(IItemCategoryMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(CheckItemCategoryNameUniqueQuery request, CancellationToken cancellationToken)
        {
            return await _repository.IsCategoryNameUniqueAsync(
                request.CategoryName,
                request.CompanyId,
                request.ExcludeCategoryId);
        }
    }
}



