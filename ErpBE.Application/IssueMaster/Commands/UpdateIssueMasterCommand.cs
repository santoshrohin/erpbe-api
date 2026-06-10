using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.IssueMaster.Commands;

public class UpdateIssueMasterCommand : IRequest<IssueMasterDto>
{
    public int       IssueCode      { get; set; }
    public int       CompanyCode    { get; set; }
    public DateTime? IssueDate      { get; set; }
    public string?   IssueType      { get; set; }
    public decimal?  MaterialReqNo  { get; set; }
    public string?   IssuedBy       { get; set; }
    public string?   RequestedBy    { get; set; }
    public int?      UserMasterCode { get; set; }
    public int       FromStore      { get; set; } = -2147483647;

    public List<UpdateIssueMasterDetailCommand> Details { get; set; } = new();
}

public class UpdateIssueMasterDetailCommand
{
    public int?     ItemCode     { get; set; }
    public int?     UomCode      { get; set; }
    public decimal  CurrentStock { get; set; }
    public decimal  RequestedQty { get; set; }
    public decimal  IssuedQty    { get; set; }
    public string?  Remark       { get; set; }
    public decimal? Rate         { get; set; }
    public decimal? Amount       { get; set; }
    public int?     ToStore      { get; set; }
}
