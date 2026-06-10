using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.Auth.Commands.Logout;

public class LogoutHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepo;

    public LogoutHandler(IRefreshTokenRepository refreshTokenRepo)
    {
        _refreshTokenRepo = refreshTokenRepo;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _refreshTokenRepo.RevokeAllForUserAsync(request.UserCode);
        return Unit.Value;
    }
}
