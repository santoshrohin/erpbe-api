using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Application.IssueMaster.Queries;
using MediatR;

namespace ErpBE.Application.IssueMaster.Handlers;

public class GetIssueMasterByIdQueryHandler : IRequestHandler<GetIssueMasterByIdQuery, IssueMasterDto?>
{
    private readonly IIssueMasterRepository _repository;

    public GetIssueMasterByIdQueryHandler(IIssueMasterRepository repository)
    {
        _repository = repository;
    }

    public Task<IssueMasterDto?> Handle(GetIssueMasterByIdQuery request, CancellationToken cancellationToken)
        => _repository.GetByIdAsync(request.IssueCode, request.CompanyCode);
}
