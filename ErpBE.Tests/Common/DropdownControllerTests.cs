using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ErpBE.Tests.Common
{
    /// <summary>
    /// Integration tests for Dropdown API
    /// Dropdown endpoints don't require authentication
    /// </summary>
    public class DropdownControllerTests : IntegrationTestBase
    {
        public DropdownControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetDropdown_WithValidRequest_ShouldReturnDropdownItems()
        {
            // Arrange
            var request = new
            {
                Table = "ITEM_UNIT_MASTER",
                IdColumn = "I_UOM_CODE",
                DisplayColumn = "I_UOM_NAME",
                Where = "ES_DELETE = 0",
                OrderBy = "I_UOM_NAME"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Dropdown", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            
        }

        [Fact]
        public async Task GetDropdown_WithInvalidTable_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new
            {
                Table = "INVALID_TABLE",
                IdColumn = "ID",
                DisplayColumn = "NAME",
                Where = "1=1",
                OrderBy = "NAME"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Dropdown", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetDropdown_WithMissingRequiredFields_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new
            {
                Table = "ITEM_UNIT_MASTER",
                IdColumn = "I_UOM_CODE"
                // Missing DisplayColumn, Where, OrderBy
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Dropdown", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetDropdown_WithEmptyTable_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new
            {
                Table = "",
                IdColumn = "I_UOM_CODE",
                DisplayColumn = "I_UOM_NAME",
                Where = "ES_DELETE = 0",
                OrderBy = "I_UOM_NAME"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Dropdown", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetDropdown_WithNullRequest_ShouldReturnBadRequest()
        {
            // Act
            var response = await Client.PostAsync("/api/Dropdown", new StringContent("", Encoding.UTF8, "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetDropdown_WithInvalidJson_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidJson = "{ invalid json }";
            var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Dropdown", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
