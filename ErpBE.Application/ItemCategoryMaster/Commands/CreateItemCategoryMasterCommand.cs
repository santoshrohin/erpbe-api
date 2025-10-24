using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Commands
{
    public class CreateItemCategoryMasterCommand : IRequest<int>
    {
        public CreateItemCategoryMasterRequest Request { get; set; } = null!;
    }
}



