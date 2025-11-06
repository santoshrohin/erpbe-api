using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.Auth.Queries
{
    public class GetFinancialYearsQueryHandler : IRequestHandler<GetFinancialYearsQuery, List<FinancialYearDto>>
    {
        private readonly IFinancialYearRepository _financialYearRepository;

        public GetFinancialYearsQueryHandler(IFinancialYearRepository financialYearRepository)
        {
            _financialYearRepository = financialYearRepository;
        }

        public async Task<List<FinancialYearDto>> Handle(GetFinancialYearsQuery request, CancellationToken cancellationToken)
        {
            return await _financialYearRepository.GetFinancialYearsByCompanyIdAsync(request.CompanyId);
        }
    }
}

