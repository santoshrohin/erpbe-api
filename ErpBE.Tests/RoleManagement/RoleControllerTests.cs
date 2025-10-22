using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ErpBE.Tests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ErpBE.Tests.RoleManagement
{
    public class RoleControllerTests : TestBase
    {
        public RoleControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateRole_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                RoleName = "TestRole_" + Guid.NewGuid().ToString().Substring(0, 8),
                Description = "Test Role Description",
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Role", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateRole_WithDuplicateName_ShouldReturnConflict()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                RoleName = "Admin", // This already exists
                Description = "Test Role Description",
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Role", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CreateRole_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                RoleName = "", // Invalid: empty role name
                Description = "Test Role Description",
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Role", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetRoles_WithValidToken_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/Role");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetRoleById_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/Role/2"); // SalesManager role ID

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UpdateRole_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                RoleId = 25, // SalesManager role ID
                RoleName = "UpdatedRole_" + Guid.NewGuid().ToString().Substring(0, 8),
                Description = "Updated Role Description",
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PutAsync("/api/Role", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task DeleteRole_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // First, create a role to delete
            var createRequest = new
            {
                RoleName = "TO_DELETE_ROLE_" + Guid.NewGuid().ToString().Substring(0, 8),
                Description = "Role to be deleted",
                IsActive = true
            };
            var createJsonContent = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/Role", createJsonContent);
            createResponse.EnsureSuccessStatusCode();
            var createdRoleId = JsonSerializer.Deserialize<dynamic>(await createResponse.Content.ReadAsStringAsync())!.GetProperty("roleId").GetInt32();

            // Act
            var response = await Client.DeleteAsync($"/api/Role/{createdRoleId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetRoles_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await Client.GetAsync("/api/Role");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateRole_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new
            {
                RoleName = "TestRole",
                Description = "Test Role Description",
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/Role", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var loginRequest = new
            {
                Username = "Mohan",
                Password = "1234",
                CompanyId = 1,
                FinancialYearCode = -2147483641
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/Login", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            return result.GetProperty("token").GetString()!;
        }
    }
}
