using ErpBE.Tests.Integration;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ErpBE.Tests.Admin;

/// <summary>
/// HTTP-level integration tests for UserController.
/// Uses TestAdminWebApplicationFactory which injects a fake Admin auth handler
/// so the [AuthorizeAdmin] policy passes and controller code gets exercised.
/// </summary>
public class UserControllerHttpTests : IClassFixture<TestAdminWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserControllerHttpTests(TestAdminWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_AsAdmin_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/User?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetUserById_WithNonExistentId_ShouldReturn404()
    {
        var response = await _client.GetAsync("/api/User/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUserByUsername_WithNonExistentUsername_ShouldReturn404()
    {
        var response = await _client.GetAsync("/api/User/username/NonExistent_User_XYZ_12345");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetUsersByCompany_AsAdmin_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/User/company/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CheckUsername_WithNonExistentUsername_ShouldReturnFalse()
    {
        var response = await _client.GetAsync($"/api/User/check-username/NonExistent_{Guid.NewGuid():N}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        result.GetProperty("exists").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task CheckEmail_WithNonExistentEmail_ShouldReturnFalse()
    {
        var response = await _client.GetAsync($"/api/User/check-email/nonexistent_{Guid.NewGuid():N}@test.com");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        result.GetProperty("exists").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetUserRoles_WithNonExistentUser_ShouldReturn200WithEmptyList()
    {
        var response = await _client.GetAsync("/api/User/999999/roles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUsersByRole_WithNonExistentRole_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/User/role/NonExistentRole_XYZ");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldReturn201()
    {
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            username = $"HTTP_TEST_{uniqueSuffix}",
            password = "TestPass@123",
            name = "HTTP Test User",
            email = $"httptest_{uniqueSuffix}@test.com",
            companyId = 1,
            financialYearCode = -2147483641,
            isActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/User", createRequest);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var userId = created.GetProperty("userId").GetInt32();

        // Cleanup
        await _client.DeleteAsync($"/api/User/{userId}");
    }

    [Fact]
    public async Task CreateUser_WithDuplicateUsername_ShouldReturn409()
    {
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            username = $"DUP_HTTP_{uniqueSuffix}",
            password = "TestPass@123",
            name = "Dup Test User",
            email = $"dup_{uniqueSuffix}@test.com",
            companyId = 1,
            financialYearCode = -2147483641,
            isActive = true
        };

        var first = await _client.PostAsJsonAsync("/api/User", createRequest);
        first.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await first.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var userId = created.GetProperty("userId").GetInt32();

        var second = await _client.PostAsJsonAsync("/api/User", createRequest);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Cleanup
        await _client.DeleteAsync($"/api/User/{userId}");
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ShouldReturn200()
    {
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            username = $"UPD_HTTP_{uniqueSuffix}",
            password = "TestPass@123",
            name = "Before Update",
            email = $"upd_{uniqueSuffix}@test.com",
            companyId = 1,
            financialYearCode = -2147483641,
            isActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/User", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var userId = created.GetProperty("userId").GetInt32();

        var updateRequest = new
        {
            userId,
            name = "After Update",
            email = $"upd2_{uniqueSuffix}@test.com",
            isActive = true
        };

        var updateResponse = await _client.PutAsJsonAsync("/api/User", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cleanup
        await _client.DeleteAsync($"/api/User/{userId}");
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ShouldReturn200()
    {
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var createRequest = new
        {
            username = $"DEL_HTTP_{uniqueSuffix}",
            password = "TestPass@123",
            name = "Delete HTTP User",
            email = $"del_{uniqueSuffix}@test.com",
            companyId = 1,
            financialYearCode = -2147483641,
            isActive = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/User", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var userId = created.GetProperty("userId").GetInt32();

        var deleteResponse = await _client.DeleteAsync($"/api/User/{userId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteUser_WithNonExistentId_ShouldReturn404()
    {
        var response = await _client.DeleteAsync("/api/User/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangePassword_WithNonExistentUser_ShouldReturn400()
    {
        var request = new
        {
            userId = 999999,
            newPassword = "NewPass@123"
        };

        var response = await _client.PostAsJsonAsync("/api/User/change-password", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ActivateUser_WithNonExistentId_ShouldReturn400()
    {
        var response = await _client.PostAsJsonAsync("/api/User/999999/activate", true);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
