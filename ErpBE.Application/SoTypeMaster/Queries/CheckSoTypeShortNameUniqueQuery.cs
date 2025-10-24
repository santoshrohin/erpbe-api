using MediatR;

namespace ErpBE.Application.SoTypeMaster.Queries
{
    public class CheckSoTypeShortNameUniqueQuery : IRequest<bool>
    {
        public string ShortName { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public int? ExcludeId { get; set; }
    }
}

