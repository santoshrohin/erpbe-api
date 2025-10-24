using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Queries
{
    public class GetItemCategoryMastersQueryHandler : IRequestHandler<GetItemCategoryMastersQuery, PagedResponse<ItemCategoryMasterDto>>
    {
        private readonly IItemCategoryMasterRepository _repository;

        public GetItemCategoryMastersQueryHandler(IItemCategoryMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<ItemCategoryMasterDto>> Handle(GetItemCategoryMastersQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetPagedAsync(request.QueryParameters);
        }
    }
}



