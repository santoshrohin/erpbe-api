using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ErpBE.Tests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ErpBE.Tests.UserManagement
{
    public class UserControllerTests : TestBase
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

            var emailUserLength = 10;
            var domain = "@ex.com"; // 7 characters
            var dynamicUser = "user" + Guid.NewGuid().ToString("N").Substring(0, emailUserLength - 4); // e.g., "userabcde"

            var request = new
            {
                Username = "Aiyuser_" + Guid.NewGuid().ToString().Substring(0, 8),
                Password = "testpass123",
                Name = "Test User",
                Email = dynamicUser + domain, // Total length = 10 + 7 = 17
                CompanyId = 1,
                FinancialYearCode = -2147483641,
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/User", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateUser_WithDuplicateUsername_ShouldReturnConflict()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Username = "mohan", // This already exists
                Password = "testpass123",
                Name = "Test User",
                Email = "test@example.com",
                CompanyId = 1,
                FinancialYearCode = -2147483641,
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/User", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task CreateUser_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new
            {
                Username = "", // Invalid: empty username
                Password = "testpass123",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/User", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetUsers_WithValidToken_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/User");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetUserById_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/User/-2147483561"); // Mohan's actual user ID

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task UpdateUser_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // First, get the user by username to get the correct ID
            var getUserResponse = await Client.GetAsync("/api/User/username/Test");
            getUserResponse.EnsureSuccessStatusCode();
            var userJson = await getUserResponse.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(userJson);
            var userId = user.GetProperty("userId").GetInt32();

            var request = new
            {
                UserId = userId, // Use the actual user ID from the database
                Name = "Updated Mohan",
                Email = "updated@example.com",
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PutAsync("/api/User", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task DeleteUser_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.DeleteAsync("/api/User/-2147483560"); // Mohan's actual user ID

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

    [Fact]
    public async Task AssignRoles_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            UserId = -2147483561, // Mohan's actual user ID
            Roles = new[] { "SalesManager", "StoreManager" } // Role names as strings
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/User/assign-roles", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

        [Fact]
        public async Task GetUserRoles_WithValidUserId_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/User/-2147483550/roles"); // Mohan's actual user ID

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var loginRequest = new
            {
                Username = "mohan",
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
