using ErpBE.API.Common;
using ErpBE.Domain.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using Xunit;

namespace ErpBE.Tests.Auth;

/// <summary>
/// Unit tests for RequirePermissionAttribute.
/// Bitmask positions match legacy USER_RIGHT.UR_RIGHTS ordering (verified from UserRights_BL.cs):
///   Pos 0=Menu, 1=View, 2=Edit, 3=Add, 4=Delete, 5=Print, 6=BackDate
/// </summary>
public class RequirePermissionAttributeTests
{
    // ─── helpers ────────────────────────────────────────────────────────────

    private static AuthorizationFilterContext BuildContext(
        bool isAuthenticated,
        params Claim[] claims)
    {
        ClaimsIdentity identity = isAuthenticated
            ? new ClaimsIdentity(claims, "TestAuth")
            : new ClaimsIdentity();

        var principal   = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        var actionCtx   = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionCtx, new List<IFilterMetadata>());
    }

    private static Claim PermClaim(int moduleCode, string mask)
        => new(PermissionClaimNames.For(moduleCode), mask);

    // ─── Unauthenticated ────────────────────────────────────────────────────

    [Fact]
    public void OnAuthorization_Unauthenticated_Returns401()
    {
        var ctx    = BuildContext(isAuthenticated: false);
        var filter = new RequirePermissionAttribute(ModuleCodes.Sales, PermissionBit.View);

        filter.OnAuthorization(ctx);

        ctx.Result.Should().BeOfType<UnauthorizedResult>();
    }

    // ─── Admin bypass ────────────────────────────────────────────────────────

    [Fact]
    public void OnAuthorization_IsAdminClaimTrue_BypassesPermissionCheck()
    {
        var ctx    = BuildContext(true, new Claim("is_admin", "true"));
        var filter = new RequirePermissionAttribute(ModuleCodes.Sales, PermissionBit.Delete);

        filter.OnAuthorization(ctx);

        ctx.Result.Should().BeNull("admin bypass should leave result null");
    }

    [Fact]
    public void OnAuthorization_RoleAdmin_BypassesPermissionCheck()
    {
        var ctx    = BuildContext(true, new Claim(ClaimTypes.Role, "Admin"));
        var filter = new RequirePermissionAttribute(ModuleCodes.Sales, PermissionBit.BackDate);

        filter.OnAuthorization(ctx);

        ctx.Result.Should().BeNull("admin role bypass should leave result null");
    }

    // ─── Permission granted ──────────────────────────────────────────────────
    // Bitmask: pos 0=Menu, 1=View, 2=Edit, 3=Add, 4=Delete, 5=Print, 6=BackDate
    // Each row: the bit at the enum's position is '1' → access allowed.

    [Theory]
    [InlineData(PermissionBit.View,     "0100000")]  // bit 1 = 1
    [InlineData(PermissionBit.Edit,     "0010000")]  // bit 2 = 1
    [InlineData(PermissionBit.Add,      "0001000")]  // bit 3 = 1
    [InlineData(PermissionBit.Delete,   "0000100")]  // bit 4 = 1
    [InlineData(PermissionBit.Print,    "0000010")]  // bit 5 = 1
    [InlineData(PermissionBit.BackDate, "0000001")]  // bit 6 = 1
    public void OnAuthorization_HasRequiredBit_AllowsAccess(PermissionBit bit, string mask)
    {
        var ctx    = BuildContext(true, PermClaim(ModuleCodes.Sales, mask));
        var filter = new RequirePermissionAttribute(ModuleCodes.Sales, bit);

        filter.OnAuthorization(ctx);

        ctx.Result.Should().BeNull($"bit {bit} (pos {(int)bit}) is '1' in mask '{mask}'");
    }

    [Fact]
    public void OnAuthorization_FullAccessMask_AllowsView()
    {
        // "1111111" = all bits set including legacy Menu bit
        var ctx    = BuildContext(true, PermClaim(ModuleCodes.Sales, "1111111"));
        var filter = new RequirePermissionAttribute(ModuleCodes.Sales, PermissionBit.View);

        filter.OnAuthorization(ctx);

        ctx.Result.Should().BeNull("full-access mask grants View (bit 1)");
    }

    // ─── Permission denied ───────────────────────────────────────────────────
    // Each row: the bit at the enum's position is '0' → 403 returned.

    [Theory]
    [InlineData(PermissionBit.View,     "1011111")]  // bit 1 = 0 (Menu=1, View=0)
    [InlineData(PermissionBit.Add,      "1110111")]  // bit 3 = 0
    [InlineData(PermissionBit.Delete,   "1111011")]  // bit 4 = 0
    [InlineData(PermissionBit.BackDate, "1111110")]  // bit 6 = 0
    public void OnAuthorization_BitIsZero_Returns403(PermissionBit bit, string mask)
    {
        var ctx    = BuildContext(true, PermClaim(ModuleCodes.Sales, mask));
        var filter = new RequirePermissionAttribute(ModuleCodes.Sales, bit);

        filter.OnAuthorization(ctx);

        ctx.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void OnAuthorization_MenuOnlyMask_DeniesView()
    {
        // "1000000" = only Menu bit set; View (bit 1) = 0 → denied
        var ctx    = BuildContext(true, PermClaim(ModuleCodes.Sales, "1000000"));
        var filter = new RequirePermissionAttribute(ModuleCodes.Sales, PermissionBit.View);

        filter.OnAuthorization(ctx);

        ctx.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden,
                "bit 0 is Menu not View; View (bit 1) is still 0");
    }

    [Fact]
    public void OnAuthorization_MissingPermissionClaim_Returns403()
    {
        var ctx    = BuildContext(true);
        var filter = new RequirePermissionAttribute(ModuleCodes.Sales, PermissionBit.View);

        filter.OnAuthorization(ctx);

        ctx.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void OnAuthorization_AllZeroMask_Returns403()
    {
        var ctx    = BuildContext(true, PermClaim(ModuleCodes.Sales, "0000000"));
        var filter = new RequirePermissionAttribute(ModuleCodes.Sales, PermissionBit.View);

        filter.OnAuthorization(ctx);

        ctx.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void OnAuthorization_WrongModule_Returns403()
    {
        var ctx    = BuildContext(true, PermClaim(ModuleCodes.Masters, "1111111"));
        var filter = new RequirePermissionAttribute(ModuleCodes.Sales, PermissionBit.View);

        filter.OnAuthorization(ctx);

        ctx.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    // ─── Response body ───────────────────────────────────────────────────────

    [Fact]
    public void OnAuthorization_Denied_ResponseBodyContainsModuleCode()
    {
        var ctx    = BuildContext(true, PermClaim(ModuleCodes.Sales, "0000000"));
        var filter = new RequirePermissionAttribute(ModuleCodes.Sales, PermissionBit.View);

        filter.OnAuthorization(ctx);

        var result = ctx.Result.Should().BeOfType<ObjectResult>().Subject;
        result.Value.Should().NotBeNull();
        result.Value!.ToString().Should().Contain("75");
    }

    // ─── Menu bit (pos 0) is not enforced by API ─────────────────────────────

    [Fact]
    public void OnAuthorization_MenuBit_IsNotAnApiEnforcedPermission()
    {
        // Menu (bit 0) = 0, but View (bit 1) = 1 → should still be allowed
        // This verifies API does not require the Menu bit to be set
        var ctx    = BuildContext(true, PermClaim(ModuleCodes.Masters, "0100000")); // Menu=0, View=1
        var filter = new RequirePermissionAttribute(ModuleCodes.Masters, PermissionBit.View);

        filter.OnAuthorization(ctx);

        ctx.Result.Should().BeNull("View (bit 1) = 1 regardless of Menu (bit 0)");
    }
}
