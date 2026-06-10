namespace ErpBE.Domain.Common
{
    /// <summary>
    /// Legacy ERP module codes from USER_RIGHT.UR_SM_CODE.
    /// These values are fixed — they exist in the database and in legacy code.
    /// Do NOT change these values.
    /// </summary>
    public static class ModuleCodes
    {
        public const int Masters     = 72;
        public const int Purchase    = 73;
        public const int Production  = 74;
        public const int Sales       = 75;
        public const int Utility     = 76;
        public const int Admin       = 77;
        public const int RnD         = 99;
        public const int Excise      = 106;
    }

    /// <summary>
    /// Bit positions within the 7-character USER_RIGHT.UR_RIGHTS bitmask.
    /// Position 0 is the leftmost character of the string.
    ///
    /// These values match the ACTUAL legacy ordering written by UserRights_BL.Save()
    /// in the legacy WebForms application (verified from source):
    ///   chkMenuDg(0) | chkViewDg(1) | chkUpdateDg(2) | chkAddDg(3)
    ///   | chkDeleteDg(4) | chkPrintDg(5) | chkBackDateDg(6)
    ///
    /// Example: "0111100" → Menu=0, View=1, Edit=1, Add=1, Delete=1, Print=0, BackDate=0
    /// Example: "1111111" → full access (all bits set)
    /// Example: "0000000" → no access
    ///
    /// NOTE: The API does not enforce Menu (bit 0). It is preserved for read-only
    /// menu-visibility use by the frontend.
    /// </summary>
    public enum PermissionBit
    {
        Menu     = 0, // Legacy menu-visibility bit; not enforced by API endpoints
        View     = 1,
        Edit     = 2,
        Add      = 3,
        Delete   = 4,
        Print    = 5,
        BackDate = 6, // "Back Date" entry permission in legacy system
    }

    /// <summary>
    /// JWT claim name prefix for permission bitmask claims.
    /// Full claim name = ClaimPrefix + moduleCode  (e.g. "perm_75")
    /// </summary>
    public static class PermissionClaimNames
    {
        public const string Prefix = "perm_";
        public static string For(int moduleCode) => $"{Prefix}{moduleCode}";
    }
}
