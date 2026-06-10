using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.Auth.Queries.Login
{
    public class LoginHandler : IRequestHandler<LoginRequest, LoginResponse>
    {
        private readonly ILoginRepository    _loginRepository;
        private readonly IJwtTokenGenerator  _jwt;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LoginHandler(
            ILoginRepository         loginRepository,
            IJwtTokenGenerator       jwt,
            IRefreshTokenRepository  refreshTokenRepository)
        {
            _loginRepository        = loginRepository;
            _jwt                    = jwt;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
        {
            // Repository handles: legacy cipher detection, bcrypt verify, lazy rehash.
            // Handler stays clean — no password logic here.
            var user = await _loginRepository.AuthenticateAsync(
                request.Username,
                request.Password,     // plain text — repository decides how to verify
                request.CompanyId,
                request.FinancialYearCode,
                cancellationToken);

            if (user is null)
                throw new UnauthorizedAccessException("Invalid username or password.");

            var accessToken  = _jwt.GenerateAccessToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();

            var tokenHash = HashToken(refreshToken);
            await _refreshTokenRepository.CreateAsync(
                user.UserCode,
                tokenHash,
                DateTime.UtcNow.AddDays(7),
                cancellationToken: cancellationToken);

            return new LoginResponse
            {
                AccessToken       = accessToken,
                RefreshToken      = refreshToken,   // controller sets this as httpOnly cookie
                UserCode          = user.UserCode,
                Username          = user.Username,
                DisplayName       = user.DisplayName,
                CompanyId         = user.CompanyId,
                FinancialYearCode = user.FinancialYearCode,
                CompanyName       = user.CompanyName,
                Email             = user.UserEmail,
                IsAdmin           = user.IsAdmin,
                OpeningDate       = user.OpeningDate,
                ClosingDate       = user.ClosingDate,
                Permissions       = user.Permissions
            };
        }

        // SHA-256 hash of the raw token value before storing in DB.
        // Raw token lives in the httpOnly cookie; only the hash is persisted.
        private static string HashToken(string token)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(token);
            return Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant();
        }
    }
}
