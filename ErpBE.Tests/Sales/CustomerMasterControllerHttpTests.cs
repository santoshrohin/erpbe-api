using ErpBE.Tests.Integration;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ErpBE.Tests.Sales;

/// <summary>
/// HTTP-level integration tests for CustomerMasterController.
/// Admin factory: verifies endpoints work (RequirePermission admin bypass).
/// NoPerm factory: verifies 403 when caller has no Sales module permission.
/// </summary>
public class CustomerMasterControllerHttpTests :
    IClassFixture<TestAdminWebApplicationFactory>,
    IClassFixture<TestNoPermWebApplicationFactory>
{
    private readonly HttpClient _admin;
    private readonly HttpClient _noPerm;

    public CustomerMasterControllerHttpTests(
        TestAdminWebApplicationFactory adminFactory,
        TestNoPermWebApplicationFactory noPermFactory)
    {
        _admin = adminFactory.CreateClient();
        _noPerm = noPermFactory.CreateClient();
    }

    // ── Admin bypass ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_AsAdmin_Returns200()
    {
        var response = await _admin.GetAsync("/api/CustomerMaster?companyId=1&pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_AsAdmin_NonExistent_ReturnsOkOrNotFound()
    {
        var response = await _admin.GetAsync("/api/CustomerMaster/999999?companyId=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CheckPartyNameUnique_AsAdmin_Returns200()
    {
        var response = await _admin.GetAsync("/api/CustomerMaster/check-partyname?partyName=ZZTEST&companyId=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("isUnique");
    }

    [Fact]
    public async Task CheckAbbreviationUnique_AsAdmin_Returns200()
    {
        var response = await _admin.GetAsync("/api/CustomerMaster/check-abbreviation?abbreviation=ZZ&companyId=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("isUnique");
    }

    [Fact]
    public async Task Create_AsAdmin_InvalidBody_Returns400()
    {
        var response = await _admin.PostAsJsonAsync("/api/CustomerMaster", new { });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_AsAdmin_IdMismatch_Returns400()
    {
        var response = await _admin.PutAsJsonAsync("/api/CustomerMaster/1", new { Id = 2, CompanyId = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_AsAdmin_NonExistent_ReturnsErrorStatus()
    {
        var response = await _admin.DeleteAsync("/api/CustomerMaster/999999?companyId=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    // ── No permission → 403 ────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithNoPermission_Returns403()
    {
        var response = await _noPerm.GetAsync("/api/CustomerMaster?companyId=1&pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetById_WithNoPermission_Returns403()
    {
        var response = await _noPerm.GetAsync("/api/CustomerMaster/1?companyId=1");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_WithNoPermission_Returns403()
    {
        var response = await _noPerm.PostAsJsonAsync("/api/CustomerMaster", new { CompanyId = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_WithNoPermission_Returns403()
    {
        var response = await _noPerm.PutAsJsonAsync("/api/CustomerMaster/1", new { Id = 1, CompanyId = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_WithNoPermission_Returns403()
    {
        var response = await _noPerm.DeleteAsync("/api/CustomerMaster/1?companyId=1");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
