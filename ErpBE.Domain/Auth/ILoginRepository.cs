namespace ErpBE.Domain.Auth
{
    public interface ILoginRepository
    {
        /// <summary>
        /// Authenticates a user using plain-text password.
        /// Handles legacy-cipher detection, bcrypt verification, and lazy rehash.
        /// Returns null if credentials are invalid or user not found.
        /// </summary>
        Task<LoginResult?> AuthenticateAsync(
            string username,
            string plainPassword,
            int    companyId,
            int    financialYearCode,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Loads a user by UM_CODE for refresh-token re-authentication.
        /// Returns null if user is inactive or deleted.
        /// </summary>
        Task<LoginResult?> GetUserByCodeAsync(
            int userCode,
            CancellationToken cancellationToken = default);
    }
}
