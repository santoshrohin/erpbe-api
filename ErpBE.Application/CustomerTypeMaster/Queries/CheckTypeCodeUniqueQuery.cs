using MediatR;

namespace ErpBE.Application.CustomerTypeMaster.Queries
{
    public class CheckTypeCodeUniqueQuery : IRequest<bool>
    {
        public string TypeCode { get; set; } = string.Empty;
        public int? ExcludeId { get; set; }
        public int CompanyId { get; set; }
    }
}

