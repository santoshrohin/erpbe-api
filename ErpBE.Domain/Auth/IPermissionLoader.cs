namespace ErpBE.Domain.Auth
{
    public interface IPermissionLoader
    {
        /// <summary>
        /// Loads all USER_RIGHT bitmask entries for a user from the database.
        /// Called once at login time — results are embedded in JWT claims.
        /// Key = module code, Value = 7-char bitmask string.
        /// </summary>
        Task<Dictionary<int, string>> LoadAsync(
            int userCode,
            CancellationToken cancellationToken = default);
    }
}
