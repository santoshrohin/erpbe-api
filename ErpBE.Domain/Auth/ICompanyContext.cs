namespace ErpBE.Domain.Auth
{
    /// <summary>
    /// Provides the authenticated user's company and financial-year context,
    /// extracted from JWT claims on every request.
    ///
    /// ALL repositories must scope queries to CompanyId and FinancialYearCode.
    /// Inject this service via constructor and pass values to every Dapper call.
    /// </summary>
    public interface ICompanyContext
    {
        int    UserCode          { get; }
        string Username          { get; }
        int    CompanyId         { get; }
        int    FinancialYearCode { get; }   // CM_CODE value
        string OpeningDate       { get; }   // "dd/MM/yyyy" — FY start
        string ClosingDate       { get; }   // "dd/MM/yyyy" — FY end
        bool   IsAdmin           { get; }

        /// <summary>
        /// Returns the 7-char bitmask for a module, or "0000000" if not assigned.
        /// </summary>
        string GetPermissions(int moduleCode);

        bool HasPermission(int moduleCode, int bitPosition);
    }
}
