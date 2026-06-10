using ErpBE.Tests.Integration;
using FluentAssertions;
using System.Net;
using Xunit;

namespace ErpBE.Tests.Admin;

/// <summary>
/// HTTP-level integration tests for LogsController.
/// Uses TestAdminWebApplicationFactory which injects a fake Admin auth handler
/// so the [AuthorizeAdmin] policy passes and controller code gets exercised.
/// </summary>
[Collection(nameof(IntegrationFixture))]
public class LogsControllerHttpTests : IClassFixture<TestAdminWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LogsControllerHttpTests(TestAdminWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetLogs_WithDefaultParameters_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/Logs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetLogs_WithPagination_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/Logs?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLogs_FilterByLevel_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/Logs?level=Information");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLogs_FilterBySearchTerm_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/Logs?searchTerm=login");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLogs_FilterByDateRange_ShouldReturn200()
    {
        var startDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var response = await _client.GetAsync($"/api/Logs?startDate={startDate}&endDate={endDate}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLogStatistics_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/Logs/statistics");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CleanupOldLogs_WithDefaultDaysToKeep_ShouldReturn200()
    {
        // Use a very high daysToKeep value so no real logs are deleted during testing
        var response = await _client.DeleteAsync("/api/Logs/cleanup?daysToKeep=9999");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
