using ErpBE.Application.Interfaces;
using ErpBE.Application.ItemCategoryMaster.Commands;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Handlers
{
    public class UpdateItemCategoryMasterCommandHandler : IRequestHandler<UpdateItemCategoryMasterCommand, Unit>
    {
        private readonly IItemCategoryMasterRepository _repository;
        private readonly IActivityLogService           _activityLog;
        private readonly ICompanyContext               _ctx;

        public UpdateItemCategoryMasterCommandHandler(
            IItemCategoryMasterRepository repository,
            IActivityLogService           activityLog,
            ICompanyContext               ctx)
        {
            _repository  = repository;
            _activityLog = activityLog;
            _ctx         = ctx;
        }

        public async Task<Unit> Handle(UpdateItemCategoryMasterCommand request, CancellationToken cancellationToken)
        {
            request.Request.CategoryName = request.Request.CategoryName.ToUpper().Trim();

            var existingCategory = await _repository.GetByIdAsync(request.Request.CategoryId);
            if (existingCategory == null)
                throw new KeyNotFoundException($"Item Category with ID '{request.Request.CategoryId}' not found.");

            var isUnique = await _repository.IsCategoryNameUniqueAsync(
                request.Request.CategoryName,
                existingCategory.CompanyId,
                request.Request.CategoryId);

            if (!isUnique)
                throw new InvalidOperationException("Item Category already exists.");

            var success = await _repository.UpdateAsync(request.Request);
            if (!success)
                throw new InvalidOperationException("Failed to update Item Category.");

            await _activityLog.WriteLogAsync(
                companyId: existingCategory.CompanyId,
                source:    "ItemCategoryMaster",
                @event:    "UPDATE",
                docName:   "Item Category Master",
                docNo:     request.Request.CategoryName,
                docCode:   request.Request.CategoryId,
                userName:  _ctx.Username,
                userCode:  _ctx.UserCode,
                cancellationToken: cancellationToken);

            return Unit.Value;
        }
    }
}
