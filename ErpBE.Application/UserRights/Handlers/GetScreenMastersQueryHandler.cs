using ErpBE.Application.DTOs;
using ErpBE.Application.Interfaces;
using ErpBE.Application.UserRights.Queries;
using MediatR;

namespace ErpBE.Application.UserRights.Handlers;

public class GetScreenMastersQueryHandler : IRequestHandler<GetScreenMastersQuery, IEnumerable<ScreenMasterDto>>
{
    private readonly IUserRightRepository _repository;

    public GetScreenMastersQueryHandler(IUserRightRepository repository)
        => _repository = repository;

    public Task<IEnumerable<ScreenMasterDto>> Handle(GetScreenMastersQuery request, CancellationToken cancellationToken)
        => _repository.GetScreensAsync(cancellationToken);
}
