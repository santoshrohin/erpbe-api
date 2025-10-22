using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ErpBE.Domain.DTOs;
using ErpBE.Domain.CommonDto;
using ErpBE.Tests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ErpBE.Tests.UnitMaster
{
    public class UnitMasterControllerTests : TestBase
    {
        public UnitMasterControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetUnitMasters_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await Client.GetAsync("/api/UnitMaster");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUnitMasters_WithValidToken_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/UnitMaster?CompanyId=1&IsActive=true");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetUnitMasters_WithPagination_ShouldReturnPagedResults()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/UnitMaster?CompanyId=1&PageNumber=1&PageSize=5");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResponse<UnitMasterDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result.Should().NotBeNull();
            result!.Data.Should().NotBeNull();
            result!.TotalCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetUnitMasterById_WithValidId_ShouldReturnUnit()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/UnitMaster/-2147483647");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UnitMasterDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result.Should().NotBeNull();
            result!.Id.Should().Be(-2147483647);
            result!.UnitName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CreateUnitMaster_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateUnitMasterRequest
            {
                UnitName = "TEST" + Guid.NewGuid().ToString().Substring(0, 6), // Max 10 chars
                UnitDescription = "Test Unit Description",
                CompanyId = 1,
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/UnitMaster", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateUnitMaster_WithDuplicateName_ShouldReturnConflict()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateUnitMasterRequest
            {
                UnitName = "KG", // This already exists
                UnitDescription = "Test Unit Description",
                CompanyId = 1,
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/UnitMaster", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task UpdateUnitMaster_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // First, create a unit to update
            var createRequest = new CreateUnitMasterRequest
            {
                UnitName = "UPDATE" + Guid.NewGuid().ToString().Substring(0, 4), // Max 10 chars
                UnitDescription = "Unit to be updated",
                CompanyId = 1,
                IsActive = true
            };
            var createJsonContent = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/UnitMaster", createJsonContent);
            createResponse.EnsureSuccessStatusCode();
            var createdUnitId = int.Parse(await createResponse.Content.ReadAsStringAsync());

            var request = new UpdateUnitMasterRequest
            {
                Id = createdUnitId,
                UnitName = "UPDATED" + Guid.NewGuid().ToString().Substring(0, 3), // Max 10 chars
                UnitDescription = "Updated Description",
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PutAsync("/api/UnitMaster", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task DeleteUnitMaster_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // First, create a unit to delete
            var createRequest = new CreateUnitMasterRequest
            {
                UnitName = "DELETE" + Guid.NewGuid().ToString().Substring(0, 4), // Max 10 chars
                UnitDescription = "Unit to be deleted",
                CompanyId = 1,
                IsActive = true
            };
            var createJsonContent = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/UnitMaster", createJsonContent);
            createResponse.EnsureSuccessStatusCode();
            var createdUnitId = int.Parse(await createResponse.Content.ReadAsStringAsync());

            // Act
            var response = await Client.DeleteAsync($"/api/UnitMaster/{createdUnitId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task SetUnitMasterActiveStatus_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // First, create a unit to update its status
            var createRequest = new CreateUnitMasterRequest
            {
                UnitName = "STATUS" + Guid.NewGuid().ToString().Substring(0, 4), // Max 10 chars
                UnitDescription = "Unit for status change",
                CompanyId = 1,
                IsActive = true
            };
            var createJsonContent = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/UnitMaster", createJsonContent);
            createResponse.EnsureSuccessStatusCode();
            var createdUnitId = int.Parse(await createResponse.Content.ReadAsStringAsync());

            // Act - Set to inactive
            var response = await Client.PatchAsync($"/api/UnitMaster/{createdUnitId}/status?isActive=false", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

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
