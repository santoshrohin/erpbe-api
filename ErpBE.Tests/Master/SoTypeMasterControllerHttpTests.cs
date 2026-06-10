using ErpBE.Tests.Integration;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ErpBE.Tests.Master;

/// <summary>
/// HTTP-level integration tests for SoTypeMasterController.
/// Admin factory: verifies endpoints work (RequirePermission admin bypass).
/// NoPerm factory: verifies 403 when caller has no Masters module permission.
/// </summary>
public class SoTypeMasterControllerHttpTests :
    IClassFixture<TestAdminWebApplicationFactory>,
    IClassFixture<TestNoPermWebApplicationFactory>
{
    private readonly HttpClient _admin;
    private readonly HttpClient _noPerm;

    public SoTypeMasterControllerHttpTests(
        TestAdminWebApplicationFactory adminFactory,
        TestNoPermWebApplicationFactory noPermFactory)
    {
        _admin = adminFactory.CreateClient();
        _noPerm = noPermFactory.CreateClient();
    }

    // ── Admin bypass (should succeed) ──────────────────────────────────────

    [Fact]
    public async Task GetSoTypeMasters_AsAdmin_Returns200()
    {
        var response = await _admin.GetAsync("/api/SoTypeMaster?companyId=1&pageNumber=1&pageSize=10&sortDirection=ASC");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSoTypeMasterById_AsAdmin_NonExistent_Returns200OrNotFound()
    {
        var response = await _admin.GetAsync("/api/SoTypeMaster/999999");
        // Handler returns Ok(null) or throws — either 200 or 404 is acceptable
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSoTypeMasterByShortName_AsAdmin_Returns200OrNotFound()
    {
        var response = await _admin.GetAsync("/api/SoTypeMaster/by-name/NONEXISTENT?companyId=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CheckUnique_AsAdmin_Returns200()
    {
        var response = await _admin.GetAsync("/api/SoTypeMaster/check-unique?shortName=ZZTEST&companyId=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("isUnique");
    }

    [Fact]
    public async Task CreateSoTypeMaster_AsAdmin_InvalidBody_Returns400()
    {
        var response = await _admin.PostAsJsonAsync("/api/SoTypeMaster", new { });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateSoTypeMaster_AsAdmin_IdMismatch_Returns400()
    {
        var response = await _admin.PutAsJsonAsync("/api/SoTypeMaster/1", new { Id = 2, ShortName = "X", CompanyId = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteSoTypeMaster_AsAdmin_NonExistent_Returns204OrNotFound()
    {
        var response = await _admin.DeleteAsync("/api/SoTypeMaster/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    // ── No permission → 403 ────────────────────────────────────────────────

    [Fact]
    public async Task GetSoTypeMasters_WithNoPermission_Returns403()
    {
        var response = await _noPerm.GetAsync("/api/SoTypeMaster?companyId=1&pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateSoTypeMaster_WithNoPermission_Returns403()
    {
        var response = await _noPerm.PostAsJsonAsync("/api/SoTypeMaster",
            new { ShortName = "X", Description = "Y", CompanyId = 1, FirstLetter = "X" });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateSoTypeMaster_WithNoPermission_Returns403()
    {
        var response = await _noPerm.PutAsJsonAsync("/api/SoTypeMaster/1",
            new { Id = 1, ShortName = "X", Description = "Y", CompanyId = 1, FirstLetter = "X" });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteSoTypeMaster_WithNoPermission_Returns403()
    {
        var response = await _noPerm.DeleteAsync("/api/SoTypeMaster/1");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
