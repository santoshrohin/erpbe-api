using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.IssueMaster.Queries;

public class GetIssueMasterByIdQuery : IRequest<IssueMasterDto?>
{
    public int IssueCode   { get; set; }
    public int CompanyCode { get; set; }
}
