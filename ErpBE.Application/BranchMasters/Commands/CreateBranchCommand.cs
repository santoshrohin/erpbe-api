using MediatR;

namespace ErpBE.Application.BranchMasters.Commands
{
    public class CreateBranchCommand : IRequest<int>
    {
        public string BranchName { get; set; }
    }
}
