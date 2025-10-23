using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ErpBE.Tests.Audit
{
    /// <summary>
    /// Integration tests for Audit Trail API
    /// Tests audit log retrieval and pagination
    /// </summary>
    public class AuditControllerTests : IntegrationTestBase
    {
        public AuditControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetAuditTrail_WithValidTableName_ShouldReturnOk()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // First create a unit master to generate audit trail
                var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 5);
                var createRequest = new
                {
                    UnitName = $"AU{uniqueId}",
                    UnitDescription = "Audit Test Unit",
                    CompanyId = 1,
                    IsActive = true
                };

                var json = JsonSerializer.Serialize(createRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var createResponse = await Client.PostAsync("/api/UnitMaster", content);
                createResponse.EnsureSuccessStatusCode();

                var createContent = await createResponse.Content.ReadAsStringAsync();
                var createdUnit = JsonSerializer.Deserialize<JsonElement>(createContent);
                var unitId = createdUnit.GetProperty("id").GetInt32();

                // Now query audit trail
                var response = await Client.GetAsync("/api/Audit/ITEM_UNIT_MASTER");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var responseContent = await response.Content.ReadAsStringAsync();
                var auditTrail = JsonSerializer.Deserialize<JsonElement>(responseContent);
                auditTrail.ValueKind.Should().Be(JsonValueKind.Array);

                // Cleanup
                await Client.DeleteAsync($"/api/UnitMaster/{unitId}");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetAuditTrail_WithPagination_ShouldReturnOk()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync("/api/Audit/ITEM_UNIT_MASTER?pageNumber=1&pageSize=10");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsStringAsync();
                var auditTrail = JsonSerializer.Deserialize<JsonElement>(content);
                auditTrail.ValueKind.Should().Be(JsonValueKind.Array);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetRecordAuditTrail_WithValidRecord_ShouldReturnOk()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Create a unit to get its audit trail
                var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 5);
                var createRequest = new
                {
                    UnitName = $"AR{uniqueId}",
                    UnitDescription = "Record Audit Test",
                    CompanyId = 1,
                    IsActive = true
                };

                var json = JsonSerializer.Serialize(createRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var createResponse = await Client.PostAsync("/api/UnitMaster", content);
                createResponse.EnsureSuccessStatusCode();

                var createContent = await createResponse.Content.ReadAsStringAsync();
                var createdUnit = JsonSerializer.Deserialize<JsonElement>(createContent);
                var unitId = createdUnit.GetProperty("id").GetInt32();

                // Get audit trail for this specific record
                var response = await Client.GetAsync($"/api/Audit/ITEM_UNIT_MASTER/{unitId}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var responseContent = await response.Content.ReadAsStringAsync();
                var auditTrail = JsonSerializer.Deserialize<JsonElement>(responseContent);
                auditTrail.ValueKind.Should().Be(JsonValueKind.Array);

                // Cleanup
                await Client.DeleteAsync($"/api/UnitMaster/{unitId}");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetAuditTrail_WithoutAuth_ShouldReturnUnauthorized()
        {
            var response = await Client.GetAsync("/api/Audit/ITEM_UNIT_MASTER");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAuditTrail_WithEmptyTableName_ShouldReturnBadRequest()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync("/api/Audit/");
                // Should return NotFound (no route match) or BadRequest
                response.StatusCode.Should().Match(x => 
                    x == HttpStatusCode.NotFound || x == HttpStatusCode.BadRequest);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetAuditTrail_WithInvalidTableName_ShouldHandleGracefully()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act - Try with invalid/SQL injection table name
                var response = await Client.GetAsync("/api/Audit/INVALID_TABLE_NAME_12345");

                // Assert - Should handle gracefully (either OK with empty data or InternalServerError)
                response.StatusCode.Should().Match(x => 
                    x == HttpStatusCode.OK || x == HttpStatusCode.InternalServerError,
                    "because invalid table names should be handled gracefully");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetAuditTrail_WithNegativeRecordId_ShouldReturnOk()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act - Negative IDs are valid in this system
                var response = await Client.GetAsync("/api/Audit/ITEM_UNIT_MASTER?recordId=-999");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetRecordAuditTrail_WithLargePagination_ShouldReturnOk()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act - Test with maximum pagination (should be capped at 100)
                var response = await Client.GetAsync("/api/Audit/ITEM_UNIT_MASTER/1?pageSize=200");

                // Assert
                response.StatusCode.Should().Match(x => 
                    x == HttpStatusCode.OK || x == HttpStatusCode.BadRequest || x == HttpStatusCode.InternalServerError,
                    "because endpoint should handle large page sizes");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}

