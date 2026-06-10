using ErpBE.Application.Interfaces;
using ErpBE.Application.ItemCategoryMaster.Commands;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Handlers
{
    public class DeleteItemCategoryMasterCommandHandler : IRequestHandler<DeleteItemCategoryMasterCommand, Unit>
    {
        private readonly IItemCategoryMasterRepository _repository;
        private readonly IActivityLogService           _activityLog;
        private readonly ICompanyContext               _ctx;

        public DeleteItemCategoryMasterCommandHandler(
            IItemCategoryMasterRepository repository,
            IActivityLogService           activityLog,
            ICompanyContext               ctx)
        {
            _repository  = repository;
            _activityLog = activityLog;
            _ctx         = ctx;
        }

        public async Task<Unit> Handle(DeleteItemCategoryMasterCommand request, CancellationToken cancellationToken)
        {
            var existingCategory = await _repository.GetByIdAsync(request.CategoryId);
            if (existingCategory == null)
                throw new KeyNotFoundException($"Item Category with ID '{request.CategoryId}' not found.");

            var isUsed = await _repository.IsCategoryUsedInItemMasterAsync(request.CategoryId);
            if (isUsed)
                throw new InvalidOperationException("Cannot delete this category. It is used in Item Master.");

            var success = await _repository.DeleteAsync(request.CategoryId);
            if (!success)
                throw new InvalidOperationException("Failed to delete Item Category.");

            await _activityLog.WriteLogAsync(
                companyId: existingCategory.CompanyId,
                source:    "ItemCategoryMaster",
                @event:    "DELETE",
                docName:   "Item Category Master",
                docNo:     string.Empty,
                docCode:   request.CategoryId,
                userName:  _ctx.Username,
                userCode:  _ctx.UserCode,
                cancellationToken: cancellationToken);

            return Unit.Value;
        }
    }
}
