using ErpBE.Application.Common.Models;
using ErpBE.Application.CustomerMaster.Queries;
using ErpBE.Application.CustomerMaster.Interfaces;
using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.CustomerMaster.Handlers
{
    public class GetCustomerMastersQueryHandler : IRequestHandler<GetCustomerMastersQuery, PagedResponse<CustomerMasterDto>>
    {
        private readonly ICustomerMasterRepository _repository;

        public GetCustomerMastersQueryHandler(ICustomerMasterRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<CustomerMasterDto>> Handle(GetCustomerMastersQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllAsync(request.Parameters);
        }
    }
}

