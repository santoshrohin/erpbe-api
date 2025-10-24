using ErpBE.Application.Interfaces;
using ErpBE.Application.ItemCategoryMaster.Commands;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Handlers
{
    public class UpdateItemCategoryMasterCommandHandler : IRequestHandler<UpdateItemCategoryMasterCommand, Unit>
    {
        private readonly IItemCategoryMasterRepository _repository;

        public UpdateItemCategoryMasterCommandHandler(IItemCategoryMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(UpdateItemCategoryMasterCommand request, CancellationToken cancellationToken)
        {
            // Convert category name to uppercase as per legacy logic
            request.Request.CategoryName = request.Request.CategoryName.ToUpper().Trim();

            // Check if category exists
            var existingCategory = await _repository.GetByIdAsync(request.Request.CategoryId);
            if (existingCategory == null)
            {
                throw new KeyNotFoundException($"Item Category with ID '{request.Request.CategoryId}' not found.");
            }

            // Check uniqueness (excluding current category)
            var isUnique = await _repository.IsCategoryNameUniqueAsync(
                request.Request.CategoryName,
                existingCategory.CompanyId,
                request.Request.CategoryId);

            if (!isUnique)
            {
                throw new InvalidOperationException("Item Category already exists.");
            }

            var success = await _repository.UpdateAsync(request.Request);
            if (!success)
            {
                throw new InvalidOperationException("Failed to update Item Category.");
            }

            return Unit.Value;
        }
    }
}



