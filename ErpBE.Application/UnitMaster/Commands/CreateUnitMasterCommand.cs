using MediatR;
using ErpBE.Application.DTOs;

namespace ErpBE.Application.UnitMaster.Commands
{
    public class CreateUnitMasterCommand : IRequest<int>
    {
        public CreateUnitMasterRequest Request { get; set; } = new();
    }
}
