using ErpBE.Domain.Auth;
using ErpBE.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ErpBE.API.Common
{
    /// <summary>
    /// Enforces a legacy USER_RIGHT bitmask permission on a controller action.
    ///
    /// Usage:
    ///   [RequirePermission(ModuleCodes.Sales, PermissionBit.Add)]
    ///
    /// The JWT must contain a claim named "perm_{moduleCode}" (e.g. "perm_75")
    /// with a 7-character bitmask value where position {bit} == '1'.
    ///
    /// IsAdmin bypasses all permission checks (bit 6 of UR_RIGHTS is always 1 for admins
    /// in legacy, but we shortcut this via the is_admin claim for simplicity).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly int           _moduleCode;
        private readonly PermissionBit _bit;

        public RequirePermissionAttribute(int moduleCode, PermissionBit bit)
        {
            _moduleCode = moduleCode;
            _bit        = bit;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Admins bypass granular permission checks
            var isAdmin = user.FindFirst("is_admin")?.Value == "true"
                       || user.IsInRole("Admin");
            if (isAdmin) return;

            var claimName = PermissionClaimNames.For(_moduleCode);
            var bitmask   = user.FindFirst(claimName)?.Value;

            if (string.IsNullOrEmpty(bitmask) || bitmask.Length <= (int)_bit
                || bitmask[(int)_bit] != '1')
            {
                context.Result = new ObjectResult(new
                {
                    status  = 403,
                    error   = "Forbidden",
                    message = $"You do not have {_bit} permission for this module.",
                    module  = _moduleCode,
                    bit     = _bit.ToString()
                })
                { StatusCode = StatusCodes.Status403Forbidden };
            }
        }
    }
}
