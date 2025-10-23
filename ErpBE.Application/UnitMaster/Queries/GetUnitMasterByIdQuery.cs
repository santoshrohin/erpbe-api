using MediatR;
using ErpBE.Application.DTOs;

namespace ErpBE.Application.UnitMaster.Queries
{
    public class GetUnitMasterByIdQuery : IRequest<UnitMasterDto?>
    {
        public int Id { get; set; }
    }
}
