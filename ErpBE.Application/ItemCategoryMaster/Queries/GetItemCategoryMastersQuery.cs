using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Queries
{
    public class GetItemCategoryMastersQuery : IRequest<PagedResponse<ItemCategoryMasterDto>>
    {
        public ItemCategoryMasterQueryParameters QueryParameters { get; set; } = null!;
    }
}



