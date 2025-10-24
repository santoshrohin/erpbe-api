using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ErpBE.Tests.SoTypeMaster
{
    public class SoTypeMasterControllerTests : IntegrationTestBase
    {
        public SoTypeMasterControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateSoTypeMaster_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = $"TEST_SO_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var request = new CreateSoTypeMasterRequest
            {
                CompanyId = 1,
                ShortName = uniqueName,
                Description = "Test SO Type",
                FirstLetter = "T"
            };

            try
            {
                // Act
                var response = await Client.PostAsJsonAsync("/api/SoTypeMaster", request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                var jsonElement = (JsonElement)result!;
                var soTypeId = jsonElement.GetProperty("id").GetInt32();
                soTypeId.Should().NotBe(0);

                // Cleanup
                await Client.DeleteAsync($"/api/SoTypeMaster/{soTypeId}");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CreateSoTypeMaster_WithDuplicateName_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = $"TEST_SO_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var request = new CreateSoTypeMasterRequest
            {
                CompanyId = 1,
                ShortName = uniqueName,
                Description = "Test SO Type",
                FirstLetter = "T"
            };

            // Create first
            var createResponse = await Client.PostAsJsonAsync("/api/SoTypeMaster", request);
            createResponse.EnsureSuccessStatusCode();
            var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
            var jsonElement = (JsonElement)createResult!;
            var soTypeId = jsonElement.GetProperty("id").GetInt32();

            // Act - Try to create duplicate
            var response = await Client.PostAsJsonAsync("/api/SoTypeMaster", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Cleanup
            await Client.DeleteAsync($"/api/SoTypeMaster/{soTypeId}");
        }

        [Theory]
        [InlineData("", "Description is required", "FirstLetter is required")]
        [InlineData("Valid Name", "", "FirstLetter is required")]
        [InlineData("Valid Name", "Valid Desc", "")]
        public async Task CreateSoTypeMaster_WithInvalidData_ShouldReturnBadRequest(
            string shortName, string description, string firstLetter)
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateSoTypeMasterRequest
            {
                CompanyId = 1,
                ShortName = shortName,
                Description = description,
                FirstLetter = firstLetter
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/SoTypeMaster", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetSoTypeMasterById_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = $"TEST_SO_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var createRequest = new CreateSoTypeMasterRequest
            {
                CompanyId = 1,
                ShortName = uniqueName,
                Description = "Test SO Type",
                FirstLetter = "T"
            };

            var createResponse = await Client.PostAsJsonAsync("/api/SoTypeMaster", createRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
            var jsonElement = (JsonElement)createResult!;
            var soTypeId = jsonElement.GetProperty("id").GetInt32();

            // Act
            var response = await Client.GetAsync($"/api/SoTypeMaster/{soTypeId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var soType = await response.Content.ReadFromJsonAsync<SoTypeMasterDto>();
            soType.Should().NotBeNull();
            soType!.ShortName.Should().Be(uniqueName);

            // Cleanup
            await Client.DeleteAsync($"/api/SoTypeMaster/{soTypeId}");
        }

        [Fact]
        public async Task GetSoTypeMasterById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/SoTypeMaster/999999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetSoTypeMasterByShortName_WithExistingName_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = $"TEST_SO_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var createRequest = new CreateSoTypeMasterRequest
            {
                CompanyId = 1,
                ShortName = uniqueName,
                Description = "Test SO Type",
                FirstLetter = "T"
            };

            var createResponse = await Client.PostAsJsonAsync("/api/SoTypeMaster", createRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
            var jsonElement = (JsonElement)createResult!;
            var soTypeId = jsonElement.GetProperty("id").GetInt32();

            // Act
            var response = await Client.GetAsync($"/api/SoTypeMaster/by-name/{uniqueName}?companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var soType = await response.Content.ReadFromJsonAsync<SoTypeMasterDto>();
            soType.Should().NotBeNull();
            soType!.ShortName.Should().Be(uniqueName);

            // Cleanup
            await Client.DeleteAsync($"/api/SoTypeMaster/{soTypeId}");
        }

        [Fact]
        public async Task UpdateSoTypeMaster_WithValidData_ShouldReturnNoContent()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = $"TEST_SO_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var createRequest = new CreateSoTypeMasterRequest
            {
                CompanyId = 1,
                ShortName = uniqueName,
                Description = "Test SO Type",
                FirstLetter = "T"
            };

            var createResponse = await Client.PostAsJsonAsync("/api/SoTypeMaster", createRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
            var jsonElement = (JsonElement)createResult!;
            var soTypeId = jsonElement.GetProperty("id").GetInt32();

            var updatedName = $"UPDATED_{uniqueName}";
            var updateRequest = new UpdateSoTypeMasterRequest
            {
                Id = soTypeId, // Use the actual ID from creation (can be negative)
                CompanyId = 1,
                ShortName = updatedName,
                Description = "Updated Description",
                FirstLetter = "U"
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/SoTypeMaster/{soTypeId}", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify update
            var getResponse = await Client.GetAsync($"/api/SoTypeMaster/{soTypeId}");
            var soType = await getResponse.Content.ReadFromJsonAsync<SoTypeMasterDto>();
            soType!.ShortName.Should().Be(updatedName);

            // Cleanup
            await Client.DeleteAsync($"/api/SoTypeMaster/{soTypeId}");
        }

        [Fact]
        public async Task DeleteSoTypeMaster_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = $"TEST_SO_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var createRequest = new CreateSoTypeMasterRequest
            {
                CompanyId = 1,
                ShortName = uniqueName,
                Description = "Test SO Type",
                FirstLetter = "T"
            };

            var createResponse = await Client.PostAsJsonAsync("/api/SoTypeMaster", createRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
            var jsonElement = (JsonElement)createResult!;
            var soTypeId = jsonElement.GetProperty("id").GetInt32();

            // Act
            var response = await Client.DeleteAsync($"/api/SoTypeMaster/{soTypeId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify soft deletion (record still exists but marked as deleted)
            // Soft delete sets ES_DELETE = 1, so the record won't be returned by normal queries
            // but it still exists in the database
        }

        [Fact]
        public async Task GetSoTypeMasters_WithPagination_ShouldReturnPagedResults()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/SoTypeMaster?PageNumber=1&PageSize=15&CompanyId=1&SortDirection=ASC");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResponse<SoTypeMasterDto>>();
            result.Should().NotBeNull();
            result!.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(15);
        }

        [Fact]
        public async Task GetSoTypeMasters_WithSearch_ShouldReturnFilteredResults()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var searchTerm = $"SEARCH_TEST_{Guid.NewGuid().ToString().Substring(0, 6)}";
            var createRequest = new CreateSoTypeMasterRequest
            {
                CompanyId = 1,
                ShortName = searchTerm,
                Description = "Search Test",
                FirstLetter = "S"
            };

            var createResponse = await Client.PostAsJsonAsync("/api/SoTypeMaster", createRequest);
            var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
            var jsonElement = (JsonElement)createResult!;
            var soTypeId = jsonElement.GetProperty("id").GetInt32();

            // Act
            var response = await Client.GetAsync($"/api/SoTypeMaster?SearchTerm={searchTerm}&CompanyId=1&SortDirection=ASC");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResponse<SoTypeMasterDto>>();
            result.Should().NotBeNull();
            result!.Data.Should().Contain(c => c.ShortName == searchTerm);

            // Cleanup
            await Client.DeleteAsync($"/api/SoTypeMaster/{soTypeId}");
        }

        [Fact]
        public async Task CheckShortNameUnique_WithUniqueName_ShouldReturnTrue()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = $"UNIQUE_{Guid.NewGuid().ToString().Substring(0, 8)}";

            // Act
            var response = await Client.GetAsync($"/api/SoTypeMaster/check-unique?shortName={uniqueName}&companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            var jsonElement = (JsonElement)result!;
            var isUnique = jsonElement.GetProperty("isUnique").GetBoolean();
            isUnique.Should().BeTrue();
        }
    }
}

