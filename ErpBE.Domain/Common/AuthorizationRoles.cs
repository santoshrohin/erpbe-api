namespace ErpBE.Domain.Common
{
    public static class AuthorizationRoles
    {
        public const string Admin = "Admin";
        public const string SalesManager = "SalesManager";
        public const string StoreManager = "StoreManager";
        public const string PurchaseManager = "PurchaseManager";
        public const string ReadOnlyManager = "ReadOnlyManager";
        public const string UtilityManager = "UtilityManager";
    }

    public static class AuthorizationPolicies
    {
        public const string AdminOnly = "AdminOnly";
        public const string SalesAccess = "SalesAccess";
        public const string StoreAccess = "StoreAccess";
        public const string PurchaseAccess = "PurchaseAccess";
        public const string ReadOnlyAccess = "ReadOnlyAccess";
        public const string UtilityAccess = "UtilityAccess";
        public const string ManagementAccess = "ManagementAccess"; // Admin, SalesManager, StoreManager, PurchaseManager
    }

    public static class Modules
    {
        public const string Sales = "Sales";
        public const string Store = "Store";
        public const string Purchase = "Purchase";
        public const string Utility = "Utility";
        public const string Farmer = "Farmer";
        public const string Placement = "Placement";
        public const string Reports = "Reports";
    }
}
