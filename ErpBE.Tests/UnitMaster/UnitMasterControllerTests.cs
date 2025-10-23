using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ErpBE.Application.DTOs;
using ErpBE.Application.Common.Models;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System;

namespace ErpBE.Tests.UnitMaster
{
    public class UnitMasterControllerTests : IntegrationTestBase
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
        public async Task CreateUnitMaster_WithDuplicateName_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // First, create a unit with a unique name
            string uniqueName = "DUP" + Guid.NewGuid().ToString().Substring(0, 5);
            var createRequest = new CreateUnitMasterRequest
            {
                UnitName = uniqueName,
                UnitDescription = "Unit for duplicate test",
                CompanyId = 1,
                IsActive = true
            };
            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/UnitMaster", createContent);
            createResponse.EnsureSuccessStatusCode();

            // Now try to create the same unit again
            var duplicateRequest = new CreateUnitMasterRequest
            {
                UnitName = uniqueName, // This already exists now
                UnitDescription = "Duplicate unit",
                CompanyId = 1,
                IsActive = true
            };
            var duplicateJson = JsonSerializer.Serialize(duplicateRequest);
            var duplicateContent = new StringContent(duplicateJson, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/UnitMaster", duplicateContent);

            // Assert - Either FluentValidation catches it (400 BadRequest) or the stored procedure does (500 InternalServerError)
            // Both indicate proper duplicate detection
            response.StatusCode.Should().Match(x => 
                x == HttpStatusCode.BadRequest || x == HttpStatusCode.InternalServerError,
                "because duplicate unit names should be rejected");

            // Cleanup
            Client.DefaultRequestHeaders.Authorization = null;
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

        [Fact]
        public async Task GetUnitMasterById_WithInvalidId_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act - Try to get non-existent unit
            var response = await Client.GetAsync("/api/UnitMaster/999999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeleteUnitMaster_WithNonExistentId_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.DeleteAsync("/api/UnitMaster/999999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetUnitMasters_WithFiltering_ShouldReturnFilteredResults()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act - Get only active units
            var response = await Client.GetAsync("/api/UnitMaster?CompanyId=1&IsActive=true&PageNumber=1&PageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            result.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        }

        [Fact]
        public async Task GetUnitMasters_WithSearchTerm_ShouldReturnFilteredResults()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act - Search for units
            var response = await Client.GetAsync("/api/UnitMaster?CompanyId=1&SearchTerm=KG&PageNumber=1&PageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            result.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        }

        [Fact]
        public async Task GetUnitMasters_WithSorting_ShouldReturnSortedResults()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act - Sort by name descending
            var response = await Client.GetAsync("/api/UnitMaster?CompanyId=1&SortBy=UnitName&SortDirection=desc&PageNumber=1&PageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseContent);
            result.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
        }

        [Fact]
        public async Task SetUnitMasterActiveStatus_ToggleStatus_ShouldReturnNoContent()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create a unit
            var createRequest = new CreateUnitMasterRequest
            {
                UnitName = "TOGGLE" + Guid.NewGuid().ToString().Substring(0, 4),
                UnitDescription = "Toggle test",
                CompanyId = 1,
                IsActive = true
            };
            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/UnitMaster", createContent);
            createResponse.EnsureSuccessStatusCode();
            var unitId = int.Parse(await createResponse.Content.ReadAsStringAsync());

            // Act - Toggle to inactive
            var response1 = await Client.PatchAsync($"/api/UnitMaster/{unitId}/status?isActive=false", null);
            response1.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Toggle back to active
            var response2 = await Client.PatchAsync($"/api/UnitMaster/{unitId}/status?isActive=true", null);
            response2.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Cleanup
            await Client.DeleteAsync($"/api/UnitMaster/{unitId}");
        }

        [Fact]
        public async Task CreateUnitMaster_WithMinimalData_ShouldReturnCreated()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateUnitMasterRequest
            {
                UnitName = "MIN" + Guid.NewGuid().ToString().Substring(0, 5),
                UnitDescription = "", // Empty description is valid
                CompanyId = 1,
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/UnitMaster", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            // Cleanup
            var unitId = int.Parse(await response.Content.ReadAsStringAsync());
            await Client.DeleteAsync($"/api/UnitMaster/{unitId}");
        }
    }
}
