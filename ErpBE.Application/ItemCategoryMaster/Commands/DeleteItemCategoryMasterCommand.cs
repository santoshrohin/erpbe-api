using MediatR;

namespace ErpBE.Application.ItemCategoryMaster.Commands
{
    public class DeleteItemCategoryMasterCommand : IRequest<Unit>
    {
        public int CategoryId { get; set; }
    }
}



