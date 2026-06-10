using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Application.ProductionToStore.Queries;
using MediatR;

namespace ErpBE.Application.ProductionToStore.Handlers;

public class GetProductionToStoreByIdQueryHandler : IRequestHandler<GetProductionToStoreByIdQuery, ProductionToStoreMasterDto?>
{
    private readonly IProductionToStoreRepository _repository;

    public GetProductionToStoreByIdQueryHandler(IProductionToStoreRepository repository)
    {
        _repository = repository;
    }

    public Task<ProductionToStoreMasterDto?> Handle(
        GetProductionToStoreByIdQuery request, CancellationToken cancellationToken)
        => _repository.GetByIdAsync(request.ProductionCode, request.CompanyCode);
}
