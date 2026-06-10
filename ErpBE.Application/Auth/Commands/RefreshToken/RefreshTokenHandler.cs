using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.Auth.Commands.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly ILoginRepository        _loginRepo;
    private readonly IJwtTokenGenerator      _jwt;

    public RefreshTokenHandler(
        IRefreshTokenRepository refreshTokenRepo,
        ILoginRepository        loginRepo,
        IJwtTokenGenerator      jwt)
    {
        _refreshTokenRepo = refreshTokenRepo;
        _loginRepo        = loginRepo;
        _jwt              = jwt;
    }

    public async Task<RefreshTokenResult> Handle(
        RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var currentHash = HashToken(request.RawToken);
        var newRawToken = _jwt.GenerateRefreshToken();
        var newHash     = HashToken(newRawToken);

        var userCode = await _refreshTokenRepo.ValidateAndRotateAsync(currentHash, newHash);
        if (userCode is null)
            throw new UnauthorizedAccessException("Refresh token expired or invalid.");

        var user = await _loginRepo.GetUserByCodeAsync(userCode.Value);
        if (user is null)
            throw new UnauthorizedAccessException("User not found or inactive.");

        await _refreshTokenRepo.CreateAsync(
            user.UserCode, newHash,
            DateTime.UtcNow.AddDays(request.ExpiryDays),
            ipAddress: request.IpAddress);

        return new RefreshTokenResult
        {
            NewAccessToken    = _jwt.GenerateAccessToken(user),
            NewRawToken       = newRawToken,
            UserCode          = user.UserCode,
            Username          = user.Username,
            CompanyId         = user.CompanyId,
            FinancialYearCode = user.FinancialYearCode,
            Permissions       = user.Permissions
        };
    }

    private static string HashToken(string token)
    {
        using var sha   = System.Security.Cryptography.SHA256.Create();
        var       bytes = System.Text.Encoding.UTF8.GetBytes(token);
        return Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant();
    }
}
