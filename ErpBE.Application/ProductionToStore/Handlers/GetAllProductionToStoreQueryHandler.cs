using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Application.ProductionToStore.Queries;
using MediatR;

namespace ErpBE.Application.ProductionToStore.Handlers;

public class GetAllProductionToStoreQueryHandler
    : IRequestHandler<GetAllProductionToStoreQuery, (IEnumerable<ProductionToStoreMasterDto> Data, int TotalCount)>
{
    private readonly IProductionToStoreRepository _repository;

    public GetAllProductionToStoreQueryHandler(IProductionToStoreRepository repository)
    {
        _repository = repository;
    }

    public Task<(IEnumerable<ProductionToStoreMasterDto> Data, int TotalCount)> Handle(
        GetAllProductionToStoreQuery request, CancellationToken cancellationToken)
        => _repository.GetAllAsync(request.Parameters);
}
