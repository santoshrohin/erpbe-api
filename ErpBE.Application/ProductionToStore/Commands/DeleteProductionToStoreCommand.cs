using MediatR;

namespace ErpBE.Application.ProductionToStore.Commands;

public class DeleteProductionToStoreCommand : IRequest<bool>
{
    public int ProductionCode { get; set; }
    public int CompanyCode    { get; set; }
}
