namespace ErpBE.Domain.Auth
{
    /// <summary>
    /// Result of a successful authentication attempt.
    /// Carries all context needed to generate JWT claims and set refresh token cookie.
    /// </summary>
    public class LoginResult
    {
        public int    UserCode         { get; set; }
        public string Username         { get; set; } = string.Empty;
        public string DisplayName      { get; set; } = string.Empty;
        public string UserEmail        { get; set; } = string.Empty;
        public bool   IsAdmin          { get; set; }

        // Company / FY context — ALL repositories scope queries to these
        public int    CompanyId         { get; set; }
        public int    FinancialYearCode { get; set; }   // CM_CODE value
        public string CompanyName       { get; set; } = string.Empty;
        public string CompanyEmail      { get; set; } = string.Empty;
        public string OpeningDate       { get; set; } = string.Empty;   // "dd/MM/yyyy"
        public string ClosingDate       { get; set; } = string.Empty;   // "dd/MM/yyyy"

        // Legacy USER_RIGHT bitmask permissions
        // Key   = module code  (e.g. 75)
        // Value = 7-char bitmask string (e.g. "1111000")
        public Dictionary<int, string> Permissions { get; set; } = new();
    }
}
