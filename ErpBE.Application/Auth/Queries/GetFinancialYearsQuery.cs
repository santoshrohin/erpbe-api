using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.Auth.Queries
{
    public class GetFinancialYearsQuery : IRequest<List<FinancialYearDto>>
    {
        public int CompanyId { get; set; }
    }
}

