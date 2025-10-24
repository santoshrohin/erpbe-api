using ErpBE.Application.Interfaces;
using ErpBE.Application.ItemCategoryMaster.Commands;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Handlers
{
    public class CreateItemCategoryMasterCommandHandler : IRequestHandler<CreateItemCategoryMasterCommand, int>
    {
        private readonly IItemCategoryMasterRepository _repository;

        public CreateItemCategoryMasterCommandHandler(IItemCategoryMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateItemCategoryMasterCommand request, CancellationToken cancellationToken)
        {
            // Convert category name to uppercase as per legacy logic
            request.Request.CategoryName = request.Request.CategoryName.ToUpper().Trim();

            // Check uniqueness
            var isUnique = await _repository.IsCategoryNameUniqueAsync(
                request.Request.CategoryName,
                request.Request.CompanyId);

            if (!isUnique)
            {
                throw new InvalidOperationException("Item Category already exists.");
            }

            return await _repository.CreateAsync(request.Request);
        }
    }
}



