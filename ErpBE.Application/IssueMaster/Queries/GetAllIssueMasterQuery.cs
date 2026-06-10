using ErpBE.Application.DTOs;
using MediatR;

namespace ErpBE.Application.IssueMaster.Queries;

public class GetAllIssueMasterQuery : IRequest<(IEnumerable<IssueMasterDto> Data, int TotalCount)>
{
    public IssueMasterQueryParameters Parameters { get; set; } = new();
}
