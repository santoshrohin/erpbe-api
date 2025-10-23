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
    }
}
