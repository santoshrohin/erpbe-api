using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Application.IssueMaster.Queries;
using MediatR;

namespace ErpBE.Application.IssueMaster.Handlers;

public class GetAllIssueMasterQueryHandler
    : IRequestHandler<GetAllIssueMasterQuery, (IEnumerable<IssueMasterDto> Data, int TotalCount)>
{
    private readonly IIssueMasterRepository _repository;

    public GetAllIssueMasterQueryHandler(IIssueMasterRepository repository)
    {
        _repository = repository;
    }

    public Task<(IEnumerable<IssueMasterDto> Data, int TotalCount)> Handle(
        GetAllIssueMasterQuery request, CancellationToken cancellationToken)
        => _repository.GetAllAsync(request.Parameters);
}
