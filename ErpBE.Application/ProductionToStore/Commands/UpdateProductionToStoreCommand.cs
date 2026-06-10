using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.ProductionToStore.Commands;

public class UpdateProductionToStoreCommand : IRequest<ProductionToStoreMasterDto>
{
    public int       ProductionCode { get; set; }
    public int       CompanyCode    { get; set; }
    public DateTime? GinDate        { get; set; }
    public string?   Type           { get; set; }
    public string?   PersonName     { get; set; }
    public decimal?  MrCode         { get; set; }
    public int?      CustomerCode   { get; set; }
    public int?      BatchNo        { get; set; }

    public List<UpdateProductionToStoreDetailCommand> Details { get; set; } = new();
}

public class UpdateProductionToStoreDetailCommand
{
    public int?    ItemCode  { get; set; }
    public decimal Quantity  { get; set; }
    public string? Remark    { get; set; }
}
