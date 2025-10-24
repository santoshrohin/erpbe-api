using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Queries
{
    public class GetItemCategoryMasterByNameQuery : IRequest<ItemCategoryMasterDto?>
    {
        public string CategoryName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
    }
}



