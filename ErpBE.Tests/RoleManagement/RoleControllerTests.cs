using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ErpBE.Tests.RoleManagement
{
    /// <summary>
    /// Integration tests for Role Management API
    /// Tests create test data, verify operations, and clean up afterwards
    /// </summary>
    public class RoleControllerTests : IntegrationTestBase
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

            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var request = new
            {
                RoleName = $"TEST_ROLE_{uniqueId}",
                Description = "Test Role for Integration Testing",
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Act
                var response = await Client.PostAsync("/api/Role", content);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                // Cleanup
                if (result.TryGetProperty("roleId", out var roleIdElement))
                {
                    var roleId = roleIdElement.GetInt32();
                    await Client.DeleteAsync($"/api/Role/{roleId}");
                }
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
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

            try
            {
                // Act
                var response = await Client.PostAsync("/api/Role", content);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
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

            try
            {
                // Act
                var response = await Client.PostAsync("/api/Role", content);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetRoles_WithValidToken_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act
                var response = await Client.GetAsync("/api/Role");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                result.ValueKind.Should().Be(JsonValueKind.Array);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetRoleById_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // First, create a test role
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var createRequest = new
            {
                RoleName = $"TEST_GET_ROLE_{uniqueId}",
                Description = "Test Get Role",
                IsActive = true
            };

            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/Role", createContent);
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
            var roleId = createResult.GetProperty("roleId").GetInt32();

            try
            {
                // Act
                var response = await Client.GetAsync($"/api/Role/{roleId}");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                result.GetProperty("roleId").GetInt32().Should().Be(roleId);
            }
            finally
            {
                // Cleanup
                await Client.DeleteAsync($"/api/Role/{roleId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task UpdateRole_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // First, create a test role
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var createRequest = new
            {
                RoleName = $"TEST_UPDATE_ROLE_{uniqueId}",
                Description = "Test Update Role",
                IsActive = true
            };

            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/Role", createContent);
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
            var roleId = createResult.GetProperty("roleId").GetInt32();

            try
            {
                // Prepare update request
                var updateRequest = new
                {
                    RoleId = roleId,
                    RoleName = $"TEST_UPDATED_ROLE_{uniqueId}",
                    Description = "Updated Role Description",
                    IsActive = true
                };

                var json = JsonSerializer.Serialize(updateRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Act
                var response = await Client.PutAsync("/api/Role", content);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
            finally
            {
                // Cleanup
                await Client.DeleteAsync($"/api/Role/{roleId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task DeleteRole_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // First, create a role to delete
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var createRequest = new
            {
                RoleName = $"TEST_DELETE_ROLE_{uniqueId}",
                Description = "Role to be deleted",
                IsActive = true
            };

            var createJsonContent = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/Role", createJsonContent);
            createResponse.EnsureSuccessStatusCode();
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
            var createdRoleId = createResult.GetProperty("roleId").GetInt32();

            try
            {
                // Act
                var response = await Client.DeleteAsync($"/api/Role/{createdRoleId}");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Verify role is deleted
                var getResponse = await Client.GetAsync($"/api/Role/{createdRoleId}");
                getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
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

        [Fact]
        public async Task GetRoleByName_WithValidName_ShouldReturnRole()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var roleName = $"RNAME{uniqueId}";
            int? roleId = null;

            try
            {
                var createRequest = new { RoleName = roleName, Description = "Test", IsActive = true };
                var json = JsonSerializer.Serialize(createRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var createResponse = await Client.PostAsync("/api/Role", content);
                createResponse.EnsureSuccessStatusCode();

                var createContent = await createResponse.Content.ReadAsStringAsync();
                var createdRole = JsonSerializer.Deserialize<JsonElement>(createContent);
                roleId = createdRole.GetProperty("roleId").GetInt32();

                var response = await Client.GetAsync($"/api/Role/name/{roleName}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var responseContent = await response.Content.ReadAsStringAsync();
                var role = JsonSerializer.Deserialize<JsonElement>(responseContent);
                role.GetProperty("roleName").GetString().Should().Be(roleName);
            }
            finally
            {
                if (roleId.HasValue) await Client.DeleteAsync($"/api/Role/{roleId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetActiveRoles_ShouldReturnOnlyActiveRoles()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync("/api/Role/active");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsStringAsync();
                var roles = JsonSerializer.Deserialize<JsonElement>(content);
                roles.ValueKind.Should().Be(JsonValueKind.Array);

                // All roles should be active
                foreach (var role in roles.EnumerateArray())
                {
                    role.GetProperty("isActive").GetBoolean().Should().BeTrue();
                }
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CheckRoleExists_WithExistingRole_ShouldReturnTrue()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync("/api/Role/check-exists/Admin");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                result.GetProperty("exists").GetBoolean().Should().BeTrue();
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CheckRoleExists_WithNonExistingRole_ShouldReturnFalse()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync($"/api/Role/check-exists/NonExist{Guid.NewGuid()}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                result.GetProperty("exists").GetBoolean().Should().BeFalse();
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CreateRole_WithNullRequest_ShouldReturnBadRequest()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.PostAsync("/api/Role", new StringContent("", Encoding.UTF8, "application/json"));
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CreateRole_WithInvalidJson_ShouldReturnBadRequest()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var invalidJson = "{ invalid json }";
                var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");
                var response = await Client.PostAsync("/api/Role", content);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task UpdateRole_WithInvalidJson_ShouldReturnBadRequest()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var invalidJson = "{ invalid: test }";
                var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");
                var response = await Client.PutAsync("/api/Role", content);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetRoleById_WithNonExistentId_ShouldReturnNotFound()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync("/api/Role/999999");
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}
