using MediatR;

namespace ErpBE.Application.UnitMaster.Commands
{
    public class DeleteUnitMasterCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}
