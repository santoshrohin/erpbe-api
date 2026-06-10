using MediatR;

namespace ErpBE.Application.IssueMaster.Commands;

public class DeleteIssueMasterCommand : IRequest<bool>
{
    public int IssueCode   { get; set; }
    public int CompanyCode { get; set; }
}
