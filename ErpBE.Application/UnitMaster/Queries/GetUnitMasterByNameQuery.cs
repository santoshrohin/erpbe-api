using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.UnitMaster.Queries
{
    public class GetUnitMasterByNameQuery : IRequest<UnitMasterDto?>
    {
        public string UnitName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
    }
}

