using ErpBE.Application.DTOs;
using ErpBE.Tests.Integration;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ErpBE.Tests.Admin;

public class UserRightsControllerHttpTests : IClassFixture<TestAdminWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserRightsControllerHttpTests(TestAdminWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ─── GET screens ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetScreens_AsAdmin_Returns200WithList()
    {
        var response = await _client.GetAsync("/api/userrights/screens");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var screens = await response.Content.ReadFromJsonAsync<List<ScreenMasterDto>>();
        screens.Should().NotBeNull();
    }

    // ─── GET user rights ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserRights_ForNonExistentUser_Returns200WithAllScreensDefaulted()
    {
        // Non-existent user: all screens returned with bitmask "0000000"
        var response = await _client.GetAsync("/api/userrights/999999");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rights = await response.Content.ReadFromJsonAsync<List<UserRightDto>>();
        rights.Should().NotBeNull();
        rights!.All(r => r.Bitmask == "0000000").Should().BeTrue();
    }

    // ─── PUT save rights ─────────────────────────────────────────────────────

    [Fact]
    public async Task SaveUserRights_WithValidPayload_Returns200()
    {
        var rights = new List<UserRightRequest>
        {
            new() { ScreenCode = 75, Bitmask = "1111000" },
            new() { ScreenCode = 77, Bitmask = "1100000" }
        };

        var response = await _client.PutAsJsonAsync("/api/userrights/999998", rights);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SaveUserRights_WithInvalidBitmask_Returns400()
    {
        var rights = new List<UserRightRequest>
        {
            new() { ScreenCode = 75, Bitmask = "INVALID" }
        };

        var response = await _client.PutAsJsonAsync("/api/userrights/999998", rights);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─── POST copy rights ────────────────────────────────────────────────────

    [Fact]
    public async Task CopyUserRights_SameUserCode_Returns400()
    {
        var response = await _client.PostAsync("/api/userrights/5/copy-from/5", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CopyUserRights_DifferentUsers_Returns200()
    {
        var response = await _client.PostAsync("/api/userrights/999998/copy-from/999997", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ─── DELETE rights ───────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteUserRights_Returns200()
    {
        var response = await _client.DeleteAsync("/api/userrights/999998");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
