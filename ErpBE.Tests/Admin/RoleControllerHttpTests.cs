using ErpBE.Tests.Integration;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ErpBE.Tests.Admin;

/// <summary>
/// HTTP-level integration tests for RoleController.
/// Uses TestAdminWebApplicationFactory which injects a fake Admin auth handler
/// so the [AuthorizeAdmin] policy passes and controller code gets exercised.
/// </summary>
public class RoleControllerHttpTests : IClassFixture<TestAdminWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RoleControllerHttpTests(TestAdminWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllRoles_AsAdmin_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/Role");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetActiveRoles_AsAdmin_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/Role/active");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRoleById_WithNonExistentId_ShouldReturn404()
    {
        var response = await _client.GetAsync("/api/Role/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRoleByName_WithNonExistentName_ShouldReturn404()
    {
        var response = await _client.GetAsync("/api/Role/name/NonExistentRole_XYZ");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CheckRoleExists_WithNonExistentRole_ShouldReturnFalse()
    {
        var response = await _client.GetAsync("/api/Role/check-exists/NonExistentRole_XYZ");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        result.GetProperty("exists").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task CreateRole_WithValidData_ShouldReturn201()
    {
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            roleName = $"TestRole_{uniqueSuffix}",
            description = "Created by HTTP controller test",
            isActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Role", createRequest);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var roleId = created.GetProperty("roleId").GetInt32();
        roleId.Should().BeGreaterThan(0);

        // Cleanup
        await _client.DeleteAsync($"/api/Role/{roleId}");
    }

    [Fact]
    public async Task CreateRole_WithDuplicateName_ShouldReturn409()
    {
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            roleName = $"DupRole_{uniqueSuffix}",
            description = "Duplicate test",
            isActive = true
        };

        var first = await _client.PostAsJsonAsync("/api/Role", createRequest);
        first.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await first.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var roleId = created.GetProperty("roleId").GetInt32();

        var second = await _client.PostAsJsonAsync("/api/Role", createRequest);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Cleanup
        await _client.DeleteAsync($"/api/Role/{roleId}");
    }

    [Fact]
    public async Task UpdateRole_WithValidData_ShouldReturn200()
    {
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            roleName = $"UpdRole_{uniqueSuffix}",
            description = "Before update",
            isActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Role", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var roleId = created.GetProperty("roleId").GetInt32();

        var updateRequest = new
        {
            roleId,
            roleName = $"UpdRole_{uniqueSuffix}",
            description = "After update",
            isActive = true
        };

        var updateResponse = await _client.PutAsJsonAsync("/api/Role", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cleanup
        await _client.DeleteAsync($"/api/Role/{roleId}");
    }

    [Fact]
    public async Task DeleteRole_WithValidId_ShouldReturn200()
    {
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            roleName = $"DelRole_{uniqueSuffix}",
            description = "To be deleted",
            isActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Role", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var roleId = created.GetProperty("roleId").GetInt32();

        var deleteResponse = await _client.DeleteAsync($"/api/Role/{roleId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteRole_WithNonExistentId_ShouldReturn404()
    {
        var response = await _client.DeleteAsync("/api/Role/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRoleById_AfterCreate_ShouldReturnRole()
    {
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            roleName = $"GetByIdRole_{uniqueSuffix}",
            description = "Get by ID test",
            isActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Role", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var roleId = created.GetProperty("roleId").GetInt32();

        var getResponse = await _client.GetAsync($"/api/Role/{roleId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var role = await getResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        role.GetProperty("roleId").GetInt32().Should().Be(roleId);

        // Cleanup
        await _client.DeleteAsync($"/api/Role/{roleId}");
    }
}
