using MediatR;

namespace ErpBE.Application.CustomerMaster.Queries
{
    public class CheckAbbreviationUniqueQuery : IRequest<bool>
    {
        public string Abbreviation { get; set; } = string.Empty;
        public int? Id { get; set; }
        public int CompanyId { get; set; }
    }
}

