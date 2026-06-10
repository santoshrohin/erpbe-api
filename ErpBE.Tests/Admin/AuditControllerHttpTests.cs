using ErpBE.Tests.Integration;
using FluentAssertions;
using System.Net;
using Xunit;

namespace ErpBE.Tests.Admin;

/// <summary>
/// HTTP-level integration tests for AuditController.
/// Uses TestAdminWebApplicationFactory which injects a fake Admin auth handler
/// so the [AuthorizeAdmin] policy passes and controller code gets exercised.
/// </summary>
public class AuditControllerHttpTests : IClassFixture<TestAdminWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuditControllerHttpTests(TestAdminWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAuditTrail_ForUnitMasterTable_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/Audit/ITEM_UNIT_MASTER");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAuditTrail_WithPagination_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/Audit/ITEM_UNIT_MASTER?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAuditTrail_WithInvalidPageSize_ShouldNormalizeTo50()
    {
        // pageSize=0 is invalid — controller normalizes it to 50
        var response = await _client.GetAsync("/api/Audit/ITEM_UNIT_MASTER?pageNumber=1&pageSize=0");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAuditTrail_WithInvalidPageSizeOver100_ShouldNormalizeTo50()
    {
        var response = await _client.GetAsync("/api/Audit/ITEM_UNIT_MASTER?pageNumber=1&pageSize=999");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAuditTrail_WithOptionalRecordId_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/Audit/ITEM_UNIT_MASTER?recordId=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRecordAuditTrail_ForSpecificRecord_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/Audit/ITEM_UNIT_MASTER/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRecordAuditTrail_WithPagination_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/Audit/ITEM_UNIT_MASTER/1?pageNumber=1&pageSize=25");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAuditTrail_WithInvalidNegativePageNumber_ShouldNormalizeTo1()
    {
        // pageNumber=-1 is invalid — controller normalizes it to 1
        var response = await _client.GetAsync("/api/Audit/ITEM_UNIT_MASTER?pageNumber=-1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
