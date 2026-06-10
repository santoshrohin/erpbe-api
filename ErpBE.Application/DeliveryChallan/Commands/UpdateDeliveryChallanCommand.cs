using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.DeliveryChallan.Commands;

public class UpdateDeliveryChallanCommand : IRequest<DeliveryChallanMasterDto>
{
    public int        ChallanCode   { get; set; }
    public int        CompanyCode   { get; set; }
    public int?       CustomerCode  { get; set; }
    public string?    Type          { get; set; }
    public DateTime?  ChallanDate   { get; set; }
    public string?    InvoiceNumber { get; set; }
    public string?    Through       { get; set; }
    public string?    VehicleNumber { get; set; }
    public string?    LrNumber      { get; set; }
    public string?    OrderNumber   { get; set; }
    public DateTime?  OrderDate     { get; set; }
    public bool       MaterialType  { get; set; }
    public bool       IsReturnable  { get; set; }

    public List<UpdateDeliveryChallanDetailCommand> Details { get; set; } = new();
}

public class UpdateDeliveryChallanDetailCommand
{
    public int?     ItemCode          { get; set; }
    public double   OrderedQuantity   { get; set; }
    public string?  BatchNumber       { get; set; }
    public string?  NumberOfPacks     { get; set; }
    public int?     UomCode           { get; set; }
    public string?  Remark            { get; set; }
    public double?  ReturnedQuantity  { get; set; }
}
