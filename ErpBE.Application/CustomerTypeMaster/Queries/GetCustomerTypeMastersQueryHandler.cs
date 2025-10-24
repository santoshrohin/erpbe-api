using ErpBE.Application.Common.Models;
using ErpBE.Application.CustomerTypeMaster.Queries;
using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerTypeMaster.Handlers
{
    public class GetCustomerTypeMastersQueryHandler : IRequestHandler<GetCustomerTypeMastersQuery, PagedResponse<CustomerTypeMasterDto>>
    {
        private readonly ICustomerTypeMasterRepository _repository;

        public GetCustomerTypeMastersQueryHandler(ICustomerTypeMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<CustomerTypeMasterDto>> Handle(GetCustomerTypeMastersQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllAsync(request.Parameters, cancellationToken);
        }
    }
}

