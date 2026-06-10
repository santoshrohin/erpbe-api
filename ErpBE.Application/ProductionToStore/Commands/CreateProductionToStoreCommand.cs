using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.ProductionToStore.Commands;

public class CreateProductionToStoreCommand : IRequest<ProductionToStoreMasterDto>
{
    public int       CompanyCode  { get; set; }
    public DateTime? GinDate      { get; set; }
    public string?   Type         { get; set; }
    public string?   PersonName   { get; set; }
    public decimal?  MrCode       { get; set; }
    public int?      CustomerCode { get; set; }
    public int?      BatchNo      { get; set; }

    public List<CreateProductionToStoreDetailCommand> Details { get; set; } = new();
}

public class CreateProductionToStoreDetailCommand
{
    public int?    ItemCode  { get; set; }
    public decimal Quantity  { get; set; }
    public string? Remark    { get; set; }
}
