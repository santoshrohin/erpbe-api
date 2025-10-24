using MediatR;

namespace ErpBE.Application.SoTypeMaster.Commands
{
    public class DeleteSoTypeMasterCommand : IRequest<Unit>
    {
        public int Id { get; set; }
    }
}

