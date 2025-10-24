using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Commands
{
    public class UpdateSoTypeMasterCommand : IRequest<Unit>
    {
        public UpdateSoTypeMasterRequest Request { get; set; } = null!;
    }
}

