using ErpBE.Application.CustomerTypeMaster.Queries;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerTypeMaster.Handlers
{
    public class GetCustomerTypeMasterByIdQueryHandler : IRequestHandler<GetCustomerTypeMasterByIdQuery, CustomerTypeMasterDto?>
    {
        private readonly ICustomerTypeMasterRepository _repository;

        public GetCustomerTypeMasterByIdQueryHandler(ICustomerTypeMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<CustomerTypeMasterDto?> Handle(GetCustomerTypeMasterByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetByIdAsync(request.Id, cancellationToken);
        }
    }
}

