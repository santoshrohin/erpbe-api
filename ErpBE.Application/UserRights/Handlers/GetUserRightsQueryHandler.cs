using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Application.UserRights.Queries;
using MediatR;

namespace ErpBE.Application.UserRights.Handlers;

public class GetUserRightsQueryHandler : IRequestHandler<GetUserRightsQuery, IEnumerable<UserRightDto>>
{
    private readonly IUserRightRepository _repository;

    public GetUserRightsQueryHandler(IUserRightRepository repository)
        => _repository = repository;

    public Task<IEnumerable<UserRightDto>> Handle(GetUserRightsQuery request, CancellationToken cancellationToken)
        => _repository.GetUserRightsAsync(request.UserCode, cancellationToken);
}
