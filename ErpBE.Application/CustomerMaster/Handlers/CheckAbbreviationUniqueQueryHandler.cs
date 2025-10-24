using ErpBE.Application.CustomerMaster.Queries;
using ErpBE.Application.CustomerMaster.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerMaster.Handlers
{
    public class CheckAbbreviationUniqueQueryHandler : IRequestHandler<CheckAbbreviationUniqueQuery, bool>
    {
        private readonly ICustomerMasterRepository _repository;

        public CheckAbbreviationUniqueQueryHandler(ICustomerMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(CheckAbbreviationUniqueQuery request, CancellationToken cancellationToken)
        {
            return await _repository.IsAbbreviationUniqueAsync(request.Abbreviation, request.Id, request.CompanyId);
        }
    }
}

