using ErpBE.API.Common.Settings;
using ErpBE.API.Models.Auth;
using ErpBE.Application.Auth.Commands.Logout;
using ErpBE.Application.Auth.Commands.RefreshToken;
using ErpBE.Application.Auth.Queries.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace ErpBE.API.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IMediator               _mediator;
        private readonly CookieSettings          _cookie;
        private readonly ILogger<LoginController> _logger;

        public LoginController(
            IMediator                mediator,
            IOptions<CookieSettings> cookieOptions,
            ILogger<LoginController> logger)
        {
            _mediator = mediator;
            _cookie   = cookieOptions.Value;
            _logger   = logger;
        }

        /// <summary>
        /// Authenticate user. Returns access token in body, refresh token in httpOnly cookie.
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("login")]
        [ProducesResponseType(typeof(LoginClientResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _mediator.Send(request);
            SetRefreshTokenCookie(response.RefreshToken);

            _logger.LogInformation(
                "User '{Username}' logged in. Company={CompanyId}, FY={FY}",
                response.Username, response.CompanyId, response.FinancialYearCode);

            return Ok(new LoginClientResponse
            {
                AccessToken       = response.AccessToken,
                UserCode          = response.UserCode,
                Username          = response.Username,
                DisplayName       = response.DisplayName,
                CompanyId         = response.CompanyId,
                FinancialYearCode = response.FinancialYearCode,
                CompanyName       = response.CompanyName,
                Email             = response.Email,
                IsAdmin           = response.IsAdmin,
                OpeningDate       = response.OpeningDate,
                ClosingDate       = response.ClosingDate,
                Permissions       = response.Permissions
            });
        }

        /// <summary>
        /// Silent token refresh. Reads refresh token from httpOnly cookie.
        /// Returns new access token; rotates refresh token cookie.
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RefreshResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh()
        {
            var rawToken = Request.Cookies[_cookie.RefreshTokenCookieName];
            if (string.IsNullOrEmpty(rawToken))
                return Unauthorized(new { message = "No refresh token." });

            try
            {
                var result = await _mediator.Send(new RefreshTokenCommand
                {
                    RawToken   = rawToken,
                    IpAddress  = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    ExpiryDays = _cookie.ExpiryDays
                });

                SetRefreshTokenCookie(result.NewRawToken);

                return Ok(new RefreshResponse
                {
                    AccessToken       = result.NewAccessToken,
                    UserCode          = result.UserCode,
                    Username          = result.Username,
                    CompanyId         = result.CompanyId,
                    FinancialYearCode = result.FinancialYearCode,
                    Permissions       = result.Permissions
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                DeleteRefreshTokenCookie();
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Logout: revokes all refresh tokens for the current user and clears the cookie.
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            if (int.TryParse(User.FindFirst("user_code")?.Value, out var userCode))
                await _mediator.Send(new LogoutCommand { UserCode = userCode });

            DeleteRefreshTokenCookie();
            return Ok(new { message = "Logged out successfully." });
        }

        // ─── cookie helpers ─────────────────────────────────────────────────

        private void SetRefreshTokenCookie(string rawToken)
        {
            var sameSite = Enum.TryParse<SameSiteMode>(_cookie.SameSite, ignoreCase: true, out var sm)
                            ? sm : SameSiteMode.Lax;

            Response.Cookies.Append(_cookie.RefreshTokenCookieName, rawToken, new CookieOptions
            {
                HttpOnly = true,
                Secure   = _cookie.Secure,
                SameSite = sameSite,
                Expires  = DateTimeOffset.UtcNow.AddDays(_cookie.ExpiryDays),
                Path     = "/api/Login"
            });
        }

        private void DeleteRefreshTokenCookie()
        {
            Response.Cookies.Delete(
                _cookie.RefreshTokenCookieName,
                new CookieOptions { Path = "/api/Login" });
        }
    }
}
