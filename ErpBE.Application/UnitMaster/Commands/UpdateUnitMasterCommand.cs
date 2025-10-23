using MediatR;
using ErpBE.Application.DTOs;

namespace ErpBE.Application.UnitMaster.Commands
{
    public class UpdateUnitMasterCommand : IRequest<bool>
    {
        public UpdateUnitMasterRequest Request { get; set; } = new();
    }
}
