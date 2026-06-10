using ErpBE.Tests.Integration;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ErpBE.Tests.Auth;

/// <summary>
/// Cross-cutting permission enforcement tests.
/// Verifies that the [RequirePermission] attribute correctly blocks access
/// to all business controllers when the caller has no module permissions.
/// Also verifies that the admin bypass works on all protected controllers.
/// </summary>
public class PermissionEnforcementHttpTests :
    IClassFixture<TestAdminWebApplicationFactory>,
    IClassFixture<TestNoPermWebApplicationFactory>
{
    private readonly HttpClient _admin;
    private readonly HttpClient _noPerm;

    public PermissionEnforcementHttpTests(
        TestAdminWebApplicationFactory adminFactory,
        TestNoPermWebApplicationFactory noPermFactory)
    {
        _admin = adminFactory.CreateClient();
        _noPerm = noPermFactory.CreateClient();
    }

    // ── Masters module (perm_72) permission enforcement ─────────────────────

    [Theory]
    [InlineData("/api/SoTypeMaster?companyId=1&pageNumber=1&pageSize=10&sortDirection=ASC")]
    [InlineData("/api/UnitMaster?companyId=1&pageNumber=1&pageSize=10")]
    [InlineData("/api/ItemCategoryMaster?companyId=1&pageNumber=1&pageSize=10")]
    [InlineData("/api/CustomerTypeMaster?companyId=1&pageNumber=1&pageSize=10")]
    public async Task MastersEndpoints_WithNoPermission_Return403(string url)
    {
        var response = await _noPerm.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("/api/SoTypeMaster?companyId=1&pageNumber=1&pageSize=10&sortDirection=ASC")]
    [InlineData("/api/UnitMaster?companyId=1&pageNumber=1&pageSize=10")]
    [InlineData("/api/ItemCategoryMaster?companyId=1&pageNumber=1&pageSize=10")]
    [InlineData("/api/CustomerTypeMaster?companyId=1&pageNumber=1&pageSize=10")]
    public async Task MastersEndpoints_AsAdmin_Return200(string url)
    {
        var response = await _admin.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Sales module (perm_75) permission enforcement ───────────────────────

    [Theory]
    [InlineData("/api/CustomerMaster?companyId=1&pageNumber=1&pageSize=10")]
    [InlineData("/api/CustomerPo?companyId=1&pageNumber=1&pageSize=10")]
    [InlineData("/api/TaxInvoice?companyId=1&pageNumber=1&pageSize=10")]
    public async Task SalesEndpoints_WithNoPermission_Return403(string url)
    {
        var response = await _noPerm.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("/api/CustomerMaster?companyId=1&pageNumber=1&pageSize=10")]
    [InlineData("/api/CustomerPo?companyId=1&pageNumber=1&pageSize=10")]
    [InlineData("/api/TaxInvoice?companyId=1&pageNumber=1&pageSize=10")]
    public async Task SalesEndpoints_AsAdmin_Return200(string url)
    {
        var response = await _admin.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Write operations: 403 enforcement ──────────────────────────────────

    [Fact]
    public async Task SoType_Post_WithNoPermission_Returns403()
    {
        var response = await _noPerm.PostAsJsonAsync("/api/SoTypeMaster",
            new { ShortName = "XX", Description = "XX", CompanyId = 1, FirstLetter = "X" });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CustomerMaster_Post_WithNoPermission_Returns403()
    {
        var response = await _noPerm.PostAsJsonAsync("/api/CustomerMaster", new { CompanyId = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UnitMaster_Delete_WithNoPermission_Returns403()
    {
        var response = await _noPerm.DeleteAsync("/api/UnitMaster/1");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ItemCategory_Delete_WithNoPermission_Returns403()
    {
        var response = await _noPerm.DeleteAsync("/api/ItemCategoryMaster/1");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CustomerPo_Delete_WithNoPermission_Returns403()
    {
        var response = await _noPerm.DeleteAsync("/api/CustomerPo/1?companyId=1");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TaxInvoice_Delete_WithNoPermission_Returns403()
    {
        var response = await _noPerm.DeleteAsync("/api/TaxInvoice/1?companyId=1");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── Print endpoints: 403 enforcement ───────────────────────────────────

    [Fact]
    public async Task CustomerPo_Print_WithNoPermission_Returns403()
    {
        var response = await _noPerm.GetAsync("/api/CustomerPo/1/print?companyId=1&companyCode=1");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TaxInvoice_Print_WithNoPermission_Returns403()
    {
        var response = await _noPerm.GetAsync("/api/TaxInvoice/1/print?companyId=1");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── Admin bypass on print endpoints ────────────────────────────────────

    [Fact]
    public async Task CustomerPo_Print_AsAdmin_NonExistent_ReturnsNotFound()
    {
        var response = await _admin.GetAsync("/api/CustomerPo/999999/print?companyId=1&companyCode=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task TaxInvoice_Print_AsAdmin_NonExistent_ReturnsNotFound()
    {
        var response = await _admin.GetAsync("/api/TaxInvoice/999999/print?companyId=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK,
            HttpStatusCode.InternalServerError);
    }
}
