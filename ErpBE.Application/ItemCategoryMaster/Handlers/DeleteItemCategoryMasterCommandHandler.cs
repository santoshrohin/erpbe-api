using ErpBE.Application.Interfaces;
using ErpBE.Application.ItemCategoryMaster.Commands;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Handlers
{
    public class DeleteItemCategoryMasterCommandHandler : IRequestHandler<DeleteItemCategoryMasterCommand, Unit>
    {
        private readonly IItemCategoryMasterRepository _repository;

        public DeleteItemCategoryMasterCommandHandler(IItemCategoryMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<Unit> Handle(DeleteItemCategoryMasterCommand request, CancellationToken cancellationToken)
        {
            // Check if category exists
            var existingCategory = await _repository.GetByIdAsync(request.CategoryId);
            if (existingCategory == null)
            {
                throw new KeyNotFoundException($"Item Category with ID '{request.CategoryId}' not found.");
            }

            // Check if category is used in Item Master (business rule from legacy)
            var isUsed = await _repository.IsCategoryUsedInItemMasterAsync(request.CategoryId);
            if (isUsed)
            {
                throw new InvalidOperationException("Cannot delete this category. It is used in Item Master.");
            }

            var success = await _repository.DeleteAsync(request.CategoryId);
            if (!success)
            {
                throw new InvalidOperationException("Failed to delete Item Category.");
            }

            return Unit.Value;
        }
    }
}



