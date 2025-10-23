using MediatR;

namespace ErpBE.Application.UnitMaster.Queries
{
    public class CheckUnitNameUniqueQuery : IRequest<bool>
    {
        public string UnitName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public int? ExcludeId { get; set; }
    }
}

