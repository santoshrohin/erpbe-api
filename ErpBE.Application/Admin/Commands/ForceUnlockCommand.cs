using ErpBE.Application.Interfaces;
using MediatR;

namespace ErpBE.Application.Admin.Commands;

public record ForceUnlockCommand(string Module, int RecordId, int AdminUserId) : IRequest<bool>;

public class ForceUnlockCommandHandler : IRequestHandler<ForceUnlockCommand, bool>
{
    private readonly IAdminLockRepository _repo;

    public ForceUnlockCommandHandler(IAdminLockRepository repo) => _repo = repo;

    public Task<bool> Handle(ForceUnlockCommand request, CancellationToken ct)
        => _repo.ForceUnlockAsync(request.Module, request.RecordId);
}
