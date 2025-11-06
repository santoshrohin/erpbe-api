using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerPo.Queries
{
    public class GetCustomerPoPrintDataQueryHandler : IRequestHandler<GetCustomerPoPrintDataQuery, CustomerPoPrintDto?>
    {
        private readonly ICustomerPoRepository _repository;

        public GetCustomerPoPrintDataQueryHandler(ICustomerPoRepository repository)
        {
            _repository = repository;
        }

        public async Task<CustomerPoPrintDto?> Handle(GetCustomerPoPrintDataQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetPrintDataAsync(
                request.PoCode, 
                request.CompanyId, 
                request.CompanyCode, 
                request.CopyType);
        }
    }
}

