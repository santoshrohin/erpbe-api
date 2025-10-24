using ErpBE.Application.CustomerMaster.Queries;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerMaster.Handlers
{
    public class GetCustomerMasterByIdQueryHandler : IRequestHandler<GetCustomerMasterByIdQuery, CustomerMasterDto?>
    {
        private readonly ICustomerMasterRepository _repository;

        public GetCustomerMasterByIdQueryHandler(ICustomerMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<CustomerMasterDto?> Handle(GetCustomerMasterByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetByIdAsync(request.Id, cancellationToken);
        }
    }
}

