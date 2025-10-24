using ErpBE.Application.CustomerMaster.Queries;
using ErpBE.Application.CustomerMaster.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerMaster.Handlers
{
    public class CheckPartyNameUniqueQueryHandler : IRequestHandler<CheckPartyNameUniqueQuery, bool>
    {
        private readonly ICustomerMasterRepository _repository;

        public CheckPartyNameUniqueQueryHandler(ICustomerMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(CheckPartyNameUniqueQuery request, CancellationToken cancellationToken)
        {
            return await _repository.IsPartyNameUniqueAsync(request.PartyName, request.Id, request.CompanyId);
        }
    }
}

