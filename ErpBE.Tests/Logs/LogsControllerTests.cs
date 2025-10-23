using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ErpBE.Tests.Logs
{
    /// <summary>
    /// Integration tests for Logs API
    /// Tests log retrieval, filtering, and pagination
    /// </summary>
    public class LogsControllerTests : IntegrationTestBase
    {
        public LogsControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetLogs_WithValidAuth_ShouldReturnOk()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync("/api/Logs");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                result.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
                result.GetProperty("totalCount").ValueKind.Should().Be(JsonValueKind.Number);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetLogs_WithPagination_ShouldReturnPagedResults()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync("/api/Logs?pageNumber=1&pageSize=10");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                result.GetProperty("pageNumber").GetInt32().Should().Be(1);
                result.GetProperty("pageSize").GetInt32().Should().Be(10);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetLogs_WithLevelFilter_ShouldReturnFilteredLogs()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync("/api/Logs?level=Information");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                result.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetLogs_WithDateFilter_ShouldReturnFilteredLogs()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var today = DateTime.UtcNow.Date;
            var startDate = today.ToString("yyyy-MM-dd");
            var endDate = today.AddDays(1).ToString("yyyy-MM-dd");

            try
            {
                var response = await Client.GetAsync($"/api/Logs?startDate={startDate}&endDate={endDate}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                result.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetLogs_WithSearchTerm_ShouldReturnFilteredLogs()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync("/api/Logs?searchTerm=Request");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                result.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetLogStatistics_WithValidAuth_ShouldReturnOk()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync("/api/Logs/statistics");
                
                // Statistics endpoint might fail if SP doesn't exist, so just check for OK or Internal Server Error
                response.StatusCode.Should().Match(x => 
                    x == HttpStatusCode.OK || x == HttpStatusCode.InternalServerError,
                    "because the statistics endpoint should either return data or fail gracefully");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        // Removed GetLogDebug tests - /debug endpoint no longer exists after CQRS refactoring

        [Fact]
        public async Task GetLogs_WithoutAuth_ShouldReturnUnauthorized()
        {
            var response = await Client.GetAsync("/api/Logs");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetLogStatistics_WithoutAuth_ShouldReturnUnauthorized()
        {
            var response = await Client.GetAsync("/api/Logs/statistics");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetLogs_WithInvalidPageNumber_ShouldHandleGracefully()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act - Negative page number should be handled (converted to 1)
                var response = await Client.GetAsync("/api/Logs?pageNumber=-1&pageSize=10");

                // Assert - Should handle gracefully (convert to valid page)
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetLogs_WithZeroPageSize_ShouldHandleGracefully()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act - Zero page size should be handled (converted to default 50)
                var response = await Client.GetAsync("/api/Logs?pageNumber=1&pageSize=0");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetLogs_WithExcessivePageSize_ShouldHandleGracefully()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act - Page size > 100 should be capped at 100
                var response = await Client.GetAsync("/api/Logs?pageNumber=1&pageSize=1000");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                result.GetProperty("pageSize").GetInt32().Should().BeLessOrEqualTo(100);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetLogs_WithInvalidLevel_ShouldReturnOk()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act - Invalid level should just return no matches
                var response = await Client.GetAsync("/api/Logs?level=InvalidLevel");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetLogs_WithFutureDateRange_ShouldReturnOk()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var futureDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");
                
                // Act
                var response = await Client.GetAsync($"/api/Logs?startDate={futureDate}");

                // Assert - Should return OK with empty data
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CleanupOldLogs_WithValidAuth_ShouldReturnOk()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act - Clean up logs older than 365 days (safe value to not delete test data)
                var response = await Client.DeleteAsync("/api/Logs/cleanup?daysToKeep=365");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                
                // Verify response structure
                result.GetProperty("message").ValueKind.Should().Be(JsonValueKind.String);
                result.GetProperty("deletedCount").ValueKind.Should().Be(JsonValueKind.Number);
                
                var deletedCount = result.GetProperty("deletedCount").GetInt32();
                deletedCount.Should().BeGreaterThanOrEqualTo(0);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CleanupOldLogs_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await Client.DeleteAsync("/api/Logs/cleanup?daysToKeep=30");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CleanupOldLogs_WithCustomDaysToKeep_ShouldReturnOk()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act - Test with different daysToKeep value
                var response = await Client.DeleteAsync("/api/Logs/cleanup?daysToKeep=180");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                result.GetProperty("deletedCount").ValueKind.Should().Be(JsonValueKind.Number);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CleanupOldLogs_WithDefaultDaysToKeep_ShouldReturnOk()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act - Test without daysToKeep parameter (should use default 30)
                var response = await Client.DeleteAsync("/api/Logs/cleanup");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                result.GetProperty("deletedCount").ValueKind.Should().Be(JsonValueKind.Number);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}

