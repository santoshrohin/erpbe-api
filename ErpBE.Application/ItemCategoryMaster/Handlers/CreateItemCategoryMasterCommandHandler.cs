using ErpBE.Application.Interfaces;
using ErpBE.Application.ItemCategoryMaster.Commands;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Handlers
{
    public class CreateItemCategoryMasterCommandHandler : IRequestHandler<CreateItemCategoryMasterCommand, int>
    {
        private readonly IItemCategoryMasterRepository _repository;
        private readonly IActivityLogService           _activityLog;
        private readonly ICompanyContext               _ctx;

        public CreateItemCategoryMasterCommandHandler(
            IItemCategoryMasterRepository repository,
            IActivityLogService           activityLog,
            ICompanyContext               ctx)
        {
            _repository  = repository;
            _activityLog = activityLog;
            _ctx         = ctx;
        }

        public async Task<int> Handle(CreateItemCategoryMasterCommand request, CancellationToken cancellationToken)
        {
            request.Request.CategoryName = request.Request.CategoryName.ToUpper().Trim();

            var isUnique = await _repository.IsCategoryNameUniqueAsync(
                request.Request.CategoryName,
                request.Request.CompanyId);

            if (!isUnique)
                throw new InvalidOperationException("Item Category already exists.");

            var result = await _repository.CreateAsync(request.Request);

            await _activityLog.WriteLogAsync(
                companyId: request.Request.CompanyId,
                source:    "ItemCategoryMaster",
                @event:    "INSERT",
                docName:   "Item Category Master",
                docNo:     request.Request.CategoryName,
                docCode:   result,
                userName:  _ctx.Username,
                userCode:  _ctx.UserCode,
                cancellationToken: cancellationToken);

            return result;
        }
    }
}
