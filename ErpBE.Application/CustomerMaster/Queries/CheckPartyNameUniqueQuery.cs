using MediatR;

namespace ErpBE.Application.CustomerMaster.Queries
{
    public class CheckPartyNameUniqueQuery : IRequest<bool>
    {
        public string PartyName { get; set; } = string.Empty;
        public int? Id { get; set; }
        public int CompanyId { get; set; }
    }
}

