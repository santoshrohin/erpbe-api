using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.Admin.Queries;

public record GetActiveLocksQuery : IRequest<IEnumerable<ActiveLockDto>>;

public class GetActiveLocksQueryHandler : IRequestHandler<GetActiveLocksQuery, IEnumerable<ActiveLockDto>>
{
    private readonly IAdminLockRepository _repo;

    public GetActiveLocksQueryHandler(IAdminLockRepository repo) => _repo = repo;

    public Task<IEnumerable<ActiveLockDto>> Handle(GetActiveLocksQuery request, CancellationToken ct)
        => _repo.GetActiveLocksAsync();
}
