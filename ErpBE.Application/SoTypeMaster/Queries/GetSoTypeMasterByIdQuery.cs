using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Queries
{
    public class GetSoTypeMasterByIdQuery : IRequest<SoTypeMasterDto>
    {
        public int Id { get; set; }
    }
}

