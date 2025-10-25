using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.CustomerPo.Queries;

/// <summary>
/// Handler for getting a Customer PO by ID
/// </summary>
public class GetCustomerPoByIdQueryHandler : IRequestHandler<GetCustomerPoByIdQuery, CustomerPoMasterDto?>
{
    private readonly ICustomerPoRepository _repository;

    public GetCustomerPoByIdQueryHandler(ICustomerPoRepository repository)
    {
        _repository = repository;
    }

    public async Task<CustomerPoMasterDto?> Handle(GetCustomerPoByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.PoCode, request.CompanyId);
    }
}

