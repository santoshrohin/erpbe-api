using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Queries
{
    public class GetItemCategoryMasterByIdQuery : IRequest<ItemCategoryMasterDto?>
    {
        public int CategoryId { get; set; }
    }
}



