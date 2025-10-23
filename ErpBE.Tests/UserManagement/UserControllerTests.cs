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

namespace ErpBE.Tests.UserManagement
{
    /// <summary>
    /// Integration tests for User Management API
    /// Tests create test data, verify operations, and clean up afterwards
    /// </summary>
    public class UserControllerTests : IntegrationTestBase
    {
        public UserControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateUser_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var request = new
            {
                Username = $"TEST_USER_{uniqueId}",
                Password = "TestPass@123",
                Name = "Test User for Integration",
                Email = $"test_{uniqueId}@test.com",
                CompanyId = 1,
                FinancialYearCode = -2147483641,
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Act
                var response = await Client.PostAsync("/api/User", content);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                // Cleanup: Delete the created user
                if (result.TryGetProperty("userId", out var userIdElement))
                {
                    var userId = userIdElement.GetInt32();
                    await Client.DeleteAsync($"/api/User/{userId}");
                }
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetAllUsers_WithValidToken_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act
                var response = await Client.GetAsync("/api/User");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                // API might return either Array or Object (paginated response)
                // Just verify we got a successful response
                result.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetUserById_WithValidId_ShouldReturnUser()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // First, create a test user
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var createRequest = new
            {
                Username = $"TEST_GET_USER_{uniqueId}",
                Password = "TestPass@123",
                Name = "Test Get User",
                Email = $"testget_{uniqueId}@test.com",
                CompanyId = 1,
                FinancialYearCode = -2147483641,
                IsActive = true
            };

            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/User", createContent);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
            var userId = createResult.GetProperty("userId").GetInt32();

            try
            {
                // Act
                var response = await Client.GetAsync($"/api/User/{userId}");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                result.GetProperty("userId").GetInt32().Should().Be(userId);
                result.GetProperty("username").GetString().Should().Be($"TEST_GET_USER_{uniqueId}");
            }
            finally
            {
                // Cleanup
                await Client.DeleteAsync($"/api/User/{userId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task UpdateUser_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // First, create a test user
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var createRequest = new
            {
                Username = $"TEST_UPDATE_USER_{uniqueId}",
                Password = "TestPass@123",
                Name = "Test Update User",
                Email = $"testupdate_{uniqueId}@test.com",
                CompanyId = 1,
                FinancialYearCode = -2147483641,
                IsActive = true
            };

            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/User", createContent);
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
            var userId = createResult.GetProperty("userId").GetInt32();

            try
            {
                // Prepare update request
                var updateRequest = new
                {
                    UserId = userId,
                    Name = "Updated Test User",
                    Email = $"updated_{uniqueId}@test.com",
                    IsActive = true
                };

                var updateJson = JsonSerializer.Serialize(updateRequest);
                var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");

                // Act
                var response = await Client.PutAsync("/api/User", updateContent);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
            finally
            {
                // Cleanup
                await Client.DeleteAsync($"/api/User/{userId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task DeleteUser_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // First, create a test user
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var createRequest = new
            {
                Username = $"TEST_DELETE_USER_{uniqueId}",
                Password = "TestPass@123",
                Name = "Test Delete User",
                Email = $"testdelete_{uniqueId}@test.com",
                CompanyId = 1,
                FinancialYearCode = -2147483641,
                IsActive = true
            };

            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/User", createContent);
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
            var userId = createResult.GetProperty("userId").GetInt32();

            try
            {
                // Act
                var response = await Client.DeleteAsync($"/api/User/{userId}");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Verify user is deleted
                var getResponse = await Client.GetAsync($"/api/User/{userId}");
                getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task AssignRoles_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // First, create a test user
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var createRequest = new
            {
                Username = $"TEST_ROLE_USER_{uniqueId}",
                Password = "TestPass@123",
                Name = "Test Role User",
                Email = $"testrole_{uniqueId}@test.com",
                CompanyId = 1,
                FinancialYearCode = -2147483641,
                IsActive = true
            };

            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/User", createContent);
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
            var userId = createResult.GetProperty("userId").GetInt32();

            try
            {
                // Prepare role assignment request
                var roleRequest = new
                {
                    UserId = userId,
                    Roles = new List<string> { "SalesManager", "StoreManager" }
                };

                var roleJson = JsonSerializer.Serialize(roleRequest);
                var roleContent = new StringContent(roleJson, Encoding.UTF8, "application/json");

                // Act
                var response = await Client.PostAsync("/api/User/assign-roles", roleContent);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
            finally
            {
                // Cleanup
                await Client.DeleteAsync($"/api/User/{userId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetUserRoles_WithValidUserId_ShouldReturnRoles()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // First, create a test user and assign roles
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var createRequest = new
            {
                Username = $"TEST_GETROLE_USER_{uniqueId}",
                Password = "TestPass@123",
                Name = "Test GetRole User",
                Email = $"testgetrole_{uniqueId}@test.com",
                CompanyId = 1,
                FinancialYearCode = -2147483641,
                IsActive = true
            };

            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/User", createContent);
            var createResponseContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
            var userId = createResult.GetProperty("userId").GetInt32();

            // Assign roles
            var roleRequest = new
            {
                UserId = userId,
                Roles = new List<string> { "SalesManager" }
            };

            var roleJson = JsonSerializer.Serialize(roleRequest);
            var roleContent = new StringContent(roleJson, Encoding.UTF8, "application/json");
            await Client.PostAsync("/api/User/assign-roles", roleContent);

            try
            {
                // Act
                var response = await Client.GetAsync($"/api/User/{userId}/roles");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                result.ValueKind.Should().Be(JsonValueKind.Array);
            }
            finally
            {
                // Cleanup
                await Client.DeleteAsync($"/api/User/{userId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetUserByUsername_WithValidUsername_ShouldReturnUser()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var username = $"UTEST{uniqueId}";
            int? userId = null;

            try
            {
                // Create test user
                var createRequest = new
                {
                    Username = username,
                    Password = "Test@123",
                    Name = "Test User",
                    Email = $"test{uniqueId}@example.com",
                    CompanyId = 1,
                    FinancialYearCode = -2147483641,
                    IsActive = true
                };

                var json = JsonSerializer.Serialize(createRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var createResponse = await Client.PostAsync("/api/User", content);
                createResponse.EnsureSuccessStatusCode();

                var createContent = await createResponse.Content.ReadAsStringAsync();
                var createdUser = JsonSerializer.Deserialize<JsonElement>(createContent);
                userId = createdUser.GetProperty("userId").GetInt32();

                // Act
                var response = await Client.GetAsync($"/api/User/username/{username}");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var responseContent = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<JsonElement>(responseContent);
                user.GetProperty("username").GetString().Should().Be(username);
            }
            finally
            {
                if (userId.HasValue) await Client.DeleteAsync($"/api/User/{userId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetUsersByCompany_WithValidCompanyId_ShouldReturnUsers()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync("/api/User/company/1");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var content = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<JsonElement>(content);
                users.ValueKind.Should().Be(JsonValueKind.Array);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task ChangePassword_WithValidData_ShouldReturnOk()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            int? userId = null;

            try
            {
                var createRequest = new
                {
                    Username = $"PTEST{uniqueId}",
                    Password = "OldPass@123",
                    Name = "Password Test User",
                    Email = $"pwd{uniqueId}@example.com",
                    CompanyId = 1,
                    FinancialYearCode = -2147483641,
                    IsActive = true
                };

                var json = JsonSerializer.Serialize(createRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var createResponse = await Client.PostAsync("/api/User", content);
                createResponse.EnsureSuccessStatusCode();

                var createContent = await createResponse.Content.ReadAsStringAsync();
                var createdUser = JsonSerializer.Deserialize<JsonElement>(createContent);
                userId = createdUser.GetProperty("userId").GetInt32();

                var changeRequest = new
                {
                    UserId = userId.Value,
                    NewPassword = "NewPass@456"
                };

                var changeJson = JsonSerializer.Serialize(changeRequest);
                var changeContent = new StringContent(changeJson, Encoding.UTF8, "application/json");

                var response = await Client.PostAsync("/api/User/change-password", changeContent);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
            finally
            {
                if (userId.HasValue) await Client.DeleteAsync($"/api/User/{userId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task ActivateUser_WithInactiveUser_ShouldActivate()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            int? userId = null;

            try
            {
                var createRequest = new
                {
                    Username = $"ATEST{uniqueId}",
                    Password = "Test@123",
                    Name = "Activate Test",
                    Email = $"act{uniqueId}@example.com",
                    CompanyId = 1,
                    FinancialYearCode = -2147483641,
                    IsActive = false
                };

                var json = JsonSerializer.Serialize(createRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var createResponse = await Client.PostAsync("/api/User", content);
                createResponse.EnsureSuccessStatusCode();

                var createContent = await createResponse.Content.ReadAsStringAsync();
                var createdUser = JsonSerializer.Deserialize<JsonElement>(createContent);
                userId = createdUser.GetProperty("userId").GetInt32();

                var activateContent = new StringContent("true", Encoding.UTF8, "application/json");
                var response = await Client.PostAsync($"/api/User/{userId}/activate", activateContent);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Verify activated
                var getResponse = await Client.GetAsync($"/api/User/{userId}");
                var getContent = await getResponse.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<JsonElement>(getContent);
                user.GetProperty("isActive").GetBoolean().Should().BeTrue();
            }
            finally
            {
                if (userId.HasValue) await Client.DeleteAsync($"/api/User/{userId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task RemoveRoles_WithValidData_ShouldRemoveRoles()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            int? userId = null;

            try
            {
                var createRequest = new
                {
                    Username = $"RTEST{uniqueId}",
                    Password = "Test@123",
                    Name = "Role Remove Test",
                    Email = $"rrole{uniqueId}@example.com",
                    CompanyId = 1,
                    FinancialYearCode = -2147483641,
                    IsActive = true
                };

                var json = JsonSerializer.Serialize(createRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var createResponse = await Client.PostAsync("/api/User", content);
                createResponse.EnsureSuccessStatusCode();

                var createContent = await createResponse.Content.ReadAsStringAsync();
                var createdUser = JsonSerializer.Deserialize<JsonElement>(createContent);
                userId = createdUser.GetProperty("userId").GetInt32();

                // Assign role first
                var assignRequest = new { UserId = userId.Value, RoleNames = new[] { "Admin" } };
                var assignJson = JsonSerializer.Serialize(assignRequest);
                var assignContent = new StringContent(assignJson, Encoding.UTF8, "application/json");
                await Client.PostAsync("/api/User/assign-roles", assignContent);

                // Remove role
                var removeRequest = new { RoleNames = new[] { "Admin" } };
                var removeJson = JsonSerializer.Serialize(removeRequest);
                var removeContent = new StringContent(removeJson, Encoding.UTF8, "application/json");

                var response = await Client.PostAsync($"/api/User/{userId}/remove-roles", removeContent);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
            finally
            {
                if (userId.HasValue) await Client.DeleteAsync($"/api/User/{userId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetUsersByRole_WithAdminRole_ShouldReturnAdminUsers()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync("/api/User/by-role/Admin");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var content = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<JsonElement>(content);
                users.ValueKind.Should().Be(JsonValueKind.Array);
                users.GetArrayLength().Should().BeGreaterThan(0);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CheckUsername_WithExistingUsername_ShouldReturnTrue()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync("/api/User/check-username/TestUser");
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
        public async Task CheckUsername_WithNonExistingUsername_ShouldReturnFalse()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync($"/api/User/check-username/NonExist{Guid.NewGuid()}");
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
        public async Task CheckEmail_WithExistingEmail_ShouldReturnTrue()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var email = $"check{uniqueId}@example.com";
            int? userId = null;

            try
            {
                var createRequest = new
                {
                    Username = $"ETEST{uniqueId}",
                    Password = "Test@123",
                    Name = "Email Check Test",
                    Email = email,
                    CompanyId = 1,
                    FinancialYearCode = -2147483641,
                    IsActive = true
                };

                var json = JsonSerializer.Serialize(createRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var createResponse = await Client.PostAsync("/api/User", content);
                createResponse.EnsureSuccessStatusCode();

                var createContent = await createResponse.Content.ReadAsStringAsync();
                var createdUser = JsonSerializer.Deserialize<JsonElement>(createContent);
                userId = createdUser.GetProperty("userId").GetInt32();

                var response = await Client.GetAsync($"/api/User/check-email/{email}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
                result.GetProperty("exists").GetBoolean().Should().BeTrue();
            }
            finally
            {
                if (userId.HasValue) await Client.DeleteAsync($"/api/User/{userId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CheckEmail_WithNonExistingEmail_ShouldReturnFalse()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.GetAsync($"/api/User/check-email/nonexist{Guid.NewGuid()}@example.com");
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
        public async Task CreateUser_WithNullRequest_ShouldReturnBadRequest()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await Client.PostAsync("/api/User", new StringContent("", Encoding.UTF8, "application/json"));
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CreateUser_WithInvalidJson_ShouldReturnBadRequest()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var invalidJson = "{ invalid json }";
                var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");
                var response = await Client.PostAsync("/api/User", content);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task UpdateUser_WithInvalidJson_ShouldReturnBadRequest()
        {
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                var invalidJson = "{ invalid: test }";
                var content = new StringContent(invalidJson, Encoding.UTF8, "application/json");
                var response = await Client.PutAsync("/api/User", content);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}
