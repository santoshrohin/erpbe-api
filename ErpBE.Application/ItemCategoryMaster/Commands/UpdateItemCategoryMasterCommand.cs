using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Commands
{
    public class UpdateItemCategoryMasterCommand : IRequest<Unit>
    {
        public UpdateItemCategoryMasterRequest Request { get; set; } = null!;
    }
}



