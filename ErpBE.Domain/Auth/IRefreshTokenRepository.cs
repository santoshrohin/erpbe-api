namespace ErpBE.Domain.Auth
{
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Stores a new refresh token (hashed). Returns the generated token ID.
        /// </summary>
        Task<int> CreateAsync(
            int    userCode,
            string tokenHash,
            DateTime expiresAt,
            string? ipAddress  = null,
            string? userAgent  = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates and atomically rotates a refresh token.
        /// Returns the user code if valid, null if expired/revoked/not found.
        /// Old token is marked as replaced by newTokenHash.
        /// </summary>
        Task<int?> ValidateAndRotateAsync(
            string currentTokenHash,
            string newTokenHash,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes all active refresh tokens for a user (logout).
        /// </summary>
        Task RevokeAllForUserAsync(
            int userCode,
            CancellationToken cancellationToken = default);
    }
}
