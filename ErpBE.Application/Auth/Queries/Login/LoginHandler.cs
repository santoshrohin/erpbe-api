using ErpBE.Application.Common;
using ErpBE.Domain.Auth;
using MediatR;

namespace ErpBE.Application.Auth.Queries.Login
{
    public class LoginHandler : IRequestHandler<LoginRequest, LoginResponse>
    {
        private readonly ILoginRepository _loginRepository;
        private readonly IJwtTokenGenerator _jwt;

        public LoginHandler(ILoginRepository loginRepository, IJwtTokenGenerator jwt)
        {
            _loginRepository = loginRepository;
            _jwt = jwt;
        }

        public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
        {
            var encryptedPassword = LegacyEncryption.Encrypt(request.Password);

            var result = await _loginRepository.VerifyLoginAsync(
                request.Username,
                encryptedPassword,
                request.CompanyId,
                request.FinancialYearCode);

            if (result == null)
                throw new UnauthorizedAccessException("Invalid username or password.");

            var token = _jwt.GenerateToken(result.Username, result.Username, result.CompanyId, result.Roles);

            return new LoginResponse
            {
                Token = token,
                Username = result.Username,
                CompanyId = result.CompanyId,
                CompanyCode = result.CompanyCode,
                CompanyName = result.CompanyName,
                Email = result.Email,
                Roles = result.Roles,
                Permissions = result.Permissions
            };
        }
    }
}
