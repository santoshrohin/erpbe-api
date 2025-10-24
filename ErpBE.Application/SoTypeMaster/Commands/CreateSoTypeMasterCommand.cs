using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Commands
{
    public class CreateSoTypeMasterCommand : IRequest<int>
    {
        public CreateSoTypeMasterRequest Request { get; set; } = null!;
    }
}

