using Microsoft.AspNetCore.Authorization;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Common;

namespace ErpBE.API.Common
{
    public class AuthorizeAdminAttribute : AuthorizeAttribute
    {
        public AuthorizeAdminAttribute() : base(AuthorizationPolicies.AdminOnly) { }
    }

    public class AuthorizeSalesAttribute : AuthorizeAttribute
    {
        public AuthorizeSalesAttribute() : base(AuthorizationPolicies.SalesAccess) { }
    }

    public class AuthorizeStoreAttribute : AuthorizeAttribute
    {
        public AuthorizeStoreAttribute() : base(AuthorizationPolicies.StoreAccess) { }
    }

    public class AuthorizePurchaseAttribute : AuthorizeAttribute
    {
        public AuthorizePurchaseAttribute() : base(AuthorizationPolicies.PurchaseAccess) { }
    }

    public class AuthorizeReadOnlyAttribute : AuthorizeAttribute
    {
        public AuthorizeReadOnlyAttribute() : base(AuthorizationPolicies.ReadOnlyAccess) { }
    }

    public class AuthorizeUtilityAttribute : AuthorizeAttribute
    {
        public AuthorizeUtilityAttribute() : base(AuthorizationPolicies.UtilityAccess) { }
    }

    public class AuthorizeManagementAttribute : AuthorizeAttribute
    {
        public AuthorizeManagementAttribute() : base(AuthorizationPolicies.ManagementAccess) { }
    }
}
