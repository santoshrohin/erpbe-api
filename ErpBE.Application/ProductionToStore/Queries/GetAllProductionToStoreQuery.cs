using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.ProductionToStore.Queries;

public class GetAllProductionToStoreQuery : IRequest<(IEnumerable<ProductionToStoreMasterDto> Data, int TotalCount)>
{
    public ProductionToStoreQueryParameters Parameters { get; set; } = new();
}
