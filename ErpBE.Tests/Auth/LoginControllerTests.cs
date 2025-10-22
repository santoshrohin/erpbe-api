using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ErpBE.Tests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ErpBE.Tests.Auth
{
    public class LoginControllerTests : TestBase
    {
        public LoginControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var loginRequest = new
            {
                Username = "Mohan",
                Password = "1234",
                CompanyId = 1,
                FinancialYearCode = -2147483641
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            result.TryGetProperty("token", out var token).Should().BeTrue();
            token.GetString().Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginRequest = new
            {
                Username = "invalid",
                Password = "invalid",
                CompanyId = 1,
                FinancialYearCode = -2147483641
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_WithMissingFinancialYearCode_ShouldReturnBadRequest()
        {
            // Arrange
            var loginRequest = new
            {
                Username = "Mohan",
                Password = "1234",
                CompanyId = 1,
                FinancialYearCode=(string)null
                // Missing FinancialYearCode
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithEmptyUsername_ShouldReturnBadRequest()
        {
            // Arrange
            var loginRequest = new
            {
                Username = "",
                Password = "1234",
                CompanyId = 1,
                FinancialYearCode = -2147483641
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithEmptyPassword_ShouldReturnBadRequest()
        {
            // Arrange
            var loginRequest = new
            {
                Username = "Mohan",
                Password = "",
                CompanyId = 1,
                FinancialYearCode = -2147483641
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithNullRequest_ShouldReturnBadRequest()
        {
            // Act
            var response = await Client.PostAsync("/api/Login", new StringContent("", Encoding.UTF8, "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_WithInvalidJson_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidJson = "{ invalid json }";
            var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Login", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
