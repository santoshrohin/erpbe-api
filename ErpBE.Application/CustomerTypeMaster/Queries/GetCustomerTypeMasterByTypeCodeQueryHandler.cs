using ErpBE.Application.CustomerTypeMaster.Queries;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerTypeMaster.Handlers
{
    public class GetCustomerTypeMasterByTypeCodeQueryHandler : IRequestHandler<GetCustomerTypeMasterByTypeCodeQuery, CustomerTypeMasterDto?>
    {
        private readonly ICustomerTypeMasterRepository _repository;

        public GetCustomerTypeMasterByTypeCodeQueryHandler(ICustomerTypeMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<CustomerTypeMasterDto?> Handle(GetCustomerTypeMasterByTypeCodeQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetByTypeCodeAsync(request.TypeCode, request.CompanyId, cancellationToken);
        }
    }
}

