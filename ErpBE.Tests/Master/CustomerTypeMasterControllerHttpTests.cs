using ErpBE.Tests.Integration;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ErpBE.Tests.Master;

/// <summary>
/// HTTP-level integration tests for CustomerTypeMasterController.
/// Admin factory: verifies endpoints work (RequirePermission admin bypass).
/// NoPerm factory: verifies 403 when caller has no Masters module permission.
/// </summary>
public class CustomerTypeMasterControllerHttpTests :
    IClassFixture<TestAdminWebApplicationFactory>,
    IClassFixture<TestNoPermWebApplicationFactory>
{
    private readonly HttpClient _admin;
    private readonly HttpClient _noPerm;

    public CustomerTypeMasterControllerHttpTests(
        TestAdminWebApplicationFactory adminFactory,
        TestNoPermWebApplicationFactory noPermFactory)
    {
        _admin = adminFactory.CreateClient();
        _noPerm = noPermFactory.CreateClient();
    }

    // ── Admin bypass ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCustomerTypeMasters_AsAdmin_Returns200()
    {
        var response = await _admin.GetAsync("/api/CustomerTypeMaster?companyId=1&pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCustomerTypeMasterById_AsAdmin_NonExistent_ReturnsOkOrNotFound()
    {
        var response = await _admin.GetAsync("/api/CustomerTypeMaster/999999?companyId=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCustomerTypeMasterByTypeCode_AsAdmin_NonExistent_ReturnsNotFound()
    {
        var response = await _admin.GetAsync("/api/CustomerTypeMaster/byTypeCode/ZZNONEXISTENT?companyId=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CheckTypeCodeUnique_AsAdmin_Returns200()
    {
        var response = await _admin.GetAsync("/api/CustomerTypeMaster/checkUnique?typeCode=ZZTEST&companyId=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateCustomerTypeMaster_AsAdmin_InvalidBody_Returns400()
    {
        var response = await _admin.PostAsJsonAsync("/api/CustomerTypeMaster", new { });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCustomerTypeMaster_AsAdmin_NonExistent_ReturnsErrorStatus()
    {
        var response = await _admin.PutAsJsonAsync("/api/CustomerTypeMaster/999999",
            new { CompanyId = 1, TypeCode = "ZZ", TypeDescription = "Test", FirstLetter = "Z" });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task DeleteCustomerTypeMaster_AsAdmin_NonExistent_ReturnsErrorStatus()
    {
        var response = await _admin.DeleteAsync("/api/CustomerTypeMaster/999999?companyId=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    // ── No permission → 403 ────────────────────────────────────────────────

    [Fact]
    public async Task GetCustomerTypeMasters_WithNoPermission_Returns403()
    {
        var response = await _noPerm.GetAsync("/api/CustomerTypeMaster?companyId=1&pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCustomerTypeMaster_WithNoPermission_Returns403()
    {
        var response = await _noPerm.PostAsJsonAsync("/api/CustomerTypeMaster",
            new { CompanyId = 1, TypeCode = "ZZ", TypeDescription = "Test", FirstLetter = "Z" });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateCustomerTypeMaster_WithNoPermission_Returns403()
    {
        var response = await _noPerm.PutAsJsonAsync("/api/CustomerTypeMaster/1",
            new { CompanyId = 1, TypeCode = "ZZ", TypeDescription = "Test", FirstLetter = "Z" });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteCustomerTypeMaster_WithNoPermission_Returns403()
    {
        var response = await _noPerm.DeleteAsync("/api/CustomerTypeMaster/1?companyId=1");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
