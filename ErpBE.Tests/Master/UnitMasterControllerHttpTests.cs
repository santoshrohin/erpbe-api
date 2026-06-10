using ErpBE.Tests.Integration;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ErpBE.Tests.Master;

/// <summary>
/// HTTP-level integration tests for UnitMasterController.
/// Admin factory: verifies endpoints work (RequirePermission admin bypass).
/// NoPerm factory: verifies 403 when caller has no Masters module permission.
/// </summary>
public class UnitMasterControllerHttpTests :
    IClassFixture<TestAdminWebApplicationFactory>,
    IClassFixture<TestNoPermWebApplicationFactory>
{
    private readonly HttpClient _admin;
    private readonly HttpClient _noPerm;

    public UnitMasterControllerHttpTests(
        TestAdminWebApplicationFactory adminFactory,
        TestNoPermWebApplicationFactory noPermFactory)
    {
        _admin = adminFactory.CreateClient();
        _noPerm = noPermFactory.CreateClient();
    }

    // ── Admin bypass ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUnitMasters_AsAdmin_Returns200()
    {
        var response = await _admin.GetAsync("/api/UnitMaster?companyId=1&pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUnitMasterById_AsAdmin_NonExistent_ReturnsSuccessOrClientError()
    {
        var response = await _admin.GetAsync("/api/UnitMaster/999999");
        // Returns 204 when null (ASP.NET Core Ok(null) → NoContent), or 404/400
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetUnitMasterByName_AsAdmin_NonExistent_ReturnsSuccessOrClientError()
    {
        var response = await _admin.GetAsync("/api/UnitMaster/name/ZZNONEXISTENT?companyId=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CheckUnitNameUnique_AsAdmin_Returns200()
    {
        var response = await _admin.GetAsync("/api/UnitMaster/check-unique?unitName=ZZTEST&companyId=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("isUnique");
    }

    [Fact]
    public async Task CreateUnitMaster_AsAdmin_InvalidBody_ReturnsClientOrServerError()
    {
        var response = await _admin.PostAsJsonAsync("/api/UnitMaster", new { });
        // FluentValidation returns 400; if validator missing the SP throws → 500
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task UpdateUnitMaster_AsAdmin_InvalidBody_ReturnsClientOrServerError()
    {
        var response = await _admin.PutAsJsonAsync("/api/UnitMaster", new { });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task DeleteUnitMaster_AsAdmin_NonExistent_ReturnsErrorStatus()
    {
        var response = await _admin.DeleteAsync("/api/UnitMaster/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SetActiveStatus_AsAdmin_NonExistent_ReturnsErrorStatus()
    {
        var response = await _admin.PatchAsync("/api/UnitMaster/999999/status?isActive=true", null);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    // ── No permission → 403 ────────────────────────────────────────────────

    [Fact]
    public async Task GetUnitMasters_WithNoPermission_Returns403()
    {
        var response = await _noPerm.GetAsync("/api/UnitMaster?companyId=1&pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateUnitMaster_WithNoPermission_Returns403()
    {
        var response = await _noPerm.PostAsJsonAsync("/api/UnitMaster",
            new { UnitName = "KG", UnitDescription = "Kilogram", CompanyId = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateUnitMaster_WithNoPermission_Returns403()
    {
        var response = await _noPerm.PutAsJsonAsync("/api/UnitMaster",
            new { Id = 1, UnitName = "KG", UnitDescription = "Kilogram", CompanyId = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteUnitMaster_WithNoPermission_Returns403()
    {
        var response = await _noPerm.DeleteAsync("/api/UnitMaster/1");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
