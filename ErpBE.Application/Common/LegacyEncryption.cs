using Microsoft.VisualBasic;

namespace ErpBE.Application.Common
{
    /// <summary>
    /// Replicates the legacy ERP password cipher exactly.
    /// Formula per character: (Asc(char) * 20 / 2) - 100
    /// Characters are joined with "-" separators.
    /// Example: "1234" → "420-460-500-540"
    ///
    /// SECURITY: This cipher is reversible obfuscation, NOT encryption.
    /// It exists only to verify passwords stored in the old format.
    /// On successful login, passwords are immediately rehashed with bcrypt.
    /// Do NOT use Encrypt() for new password storage.
    /// </summary>
    public static class LegacyEncryption
    {
        /// <summary>
        /// Produces the legacy cipher string for a plain-text password.
        /// Used only for comparison against UM_PASSWORD values that have not yet
        /// been migrated to bcrypt.
        /// </summary>
        public static string Encrypt(string pwd)
        {
            int pos = 0;
            var result = new System.Text.StringBuilder();

            foreach (char c in pwd)
            {
                int encoded = (Strings.Asc(c) * 20 / 2) - 100;
                if (pos == 0)
                    result.Append(encoded);
                else
                    result.Append('-').Append(encoded);
                pos++;
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns true if plainPassword matches a stored legacy-cipher hash.
        /// </summary>
        public static bool Verify(string plainPassword, string storedLegacyHash)
            => Encrypt(plainPassword) == storedLegacyHash;

        /// <summary>
        /// Returns true if the stored hash looks like a bcrypt hash.
        /// BCrypt.Net hashes begin with "$2a$" or "$2b$".
        /// </summary>
        public static bool IsBcryptHash(string storedHash)
            => storedHash.StartsWith("$2a$", StringComparison.Ordinal)
            || storedHash.StartsWith("$2b$", StringComparison.Ordinal);
    }
}
