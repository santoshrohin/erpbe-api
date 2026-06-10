using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.ProductionToStore.Queries;

public class GetProductionToStoreByIdQuery : IRequest<ProductionToStoreMasterDto?>
{
    public int ProductionCode { get; set; }
    public int CompanyCode    { get; set; }
}
