using ErpBE.Tests.Integration;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ErpBE.Tests.Master;

/// <summary>
/// HTTP-level integration tests for ItemCategoryMasterController.
/// Admin factory: verifies endpoints work (RequirePermission admin bypass).
/// NoPerm factory: verifies 403 when caller has no Masters module permission.
/// </summary>
public class ItemCategoryMasterControllerHttpTests :
    IClassFixture<TestAdminWebApplicationFactory>,
    IClassFixture<TestNoPermWebApplicationFactory>
{
    private readonly HttpClient _admin;
    private readonly HttpClient _noPerm;

    public ItemCategoryMasterControllerHttpTests(
        TestAdminWebApplicationFactory adminFactory,
        TestNoPermWebApplicationFactory noPermFactory)
    {
        _admin = adminFactory.CreateClient();
        _noPerm = noPermFactory.CreateClient();
    }

    // ── Admin bypass ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetItemCategories_AsAdmin_Returns200()
    {
        var response = await _admin.GetAsync("/api/ItemCategoryMaster?companyId=1&pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetItemCategoryById_AsAdmin_NonExistent_ReturnsSuccessOrClientError()
    {
        var response = await _admin.GetAsync("/api/ItemCategoryMaster/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetItemCategoryByName_AsAdmin_NonExistent_ReturnsSuccessOrClientError()
    {
        var response = await _admin.GetAsync("/api/ItemCategoryMaster/name/ZZNONEXISTENT?companyId=1");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CheckCategoryNameUnique_AsAdmin_Returns200()
    {
        var response = await _admin.GetAsync("/api/ItemCategoryMaster/check-unique?categoryName=ZZTEST&companyId=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("isUnique");
    }

    [Fact]
    public async Task CreateItemCategory_AsAdmin_InvalidBody_Returns400()
    {
        var response = await _admin.PostAsJsonAsync("/api/ItemCategoryMaster", new { });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateItemCategory_AsAdmin_InvalidBody_ReturnsClientOrServerError()
    {
        var response = await _admin.PutAsJsonAsync("/api/ItemCategoryMaster", new { });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound,
            HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task DeleteItemCategory_AsAdmin_NonExistent_ReturnsErrorStatus()
    {
        var response = await _admin.DeleteAsync("/api/ItemCategoryMaster/999999");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound,
            HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    // ── No permission → 403 ────────────────────────────────────────────────

    [Fact]
    public async Task GetItemCategories_WithNoPermission_Returns403()
    {
        var response = await _noPerm.GetAsync("/api/ItemCategoryMaster?companyId=1&pageNumber=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateItemCategory_WithNoPermission_Returns403()
    {
        var response = await _noPerm.PostAsJsonAsync("/api/ItemCategoryMaster",
            new { CategoryName = "TEST", CompanyId = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateItemCategory_WithNoPermission_Returns403()
    {
        var response = await _noPerm.PutAsJsonAsync("/api/ItemCategoryMaster",
            new { Id = 1, CategoryName = "TEST", CompanyId = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteItemCategory_WithNoPermission_Returns403()
    {
        var response = await _noPerm.DeleteAsync("/api/ItemCategoryMaster/1");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
