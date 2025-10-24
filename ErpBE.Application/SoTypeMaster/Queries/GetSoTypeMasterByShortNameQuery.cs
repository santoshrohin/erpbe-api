using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.SoTypeMaster.Queries
{
    public class GetSoTypeMasterByShortNameQuery : IRequest<SoTypeMasterDto>
    {
        public string ShortName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
    }
}

