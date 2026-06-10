namespace ErpBE.Domain.Auth
{
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// Generates a JWT access token containing all context and permission claims.
        /// </summary>
        string GenerateAccessToken(LoginResult user);

        /// <summary>
        /// Generates a cryptographically random refresh token value (plain text).
        /// Caller is responsible for hashing before storing.
        /// </summary>
        string GenerateRefreshToken();
    }
}
