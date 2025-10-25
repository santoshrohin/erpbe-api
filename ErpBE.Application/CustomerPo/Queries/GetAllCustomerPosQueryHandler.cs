using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerPo.Queries;

/// <summary>
/// Handler for getting all Customer POs with pagination
/// </summary>
public class GetAllCustomerPosQueryHandler : IRequestHandler<GetAllCustomerPosQuery, (IEnumerable<CustomerPoMasterDto> Data, int TotalCount)>
{
    private readonly ICustomerPoRepository _repository;

    public GetAllCustomerPosQueryHandler(ICustomerPoRepository repository)
    {
        _repository = repository;
    }

    public async Task<(IEnumerable<CustomerPoMasterDto> Data, int TotalCount)> Handle(GetAllCustomerPosQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(request.Parameters);
    }
}

