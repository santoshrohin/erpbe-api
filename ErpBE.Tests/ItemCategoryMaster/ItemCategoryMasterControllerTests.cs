using System.Net;
using System.Text;
using System.Text.Json;
using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ErpBE.Tests.ItemCategoryMaster
{
    public class ItemCategoryMasterControllerTests : IntegrationTestBase
    {
        public ItemCategoryMasterControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetItemCategories_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Act
            var response = await Client.GetAsync("/api/ItemCategoryMaster");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetItemCategories_WithValidToken_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act
                var response = await Client.GetAsync("/api/ItemCategoryMaster?CompanyId=1&IsActive=true");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CreateItemCategory_WithoutAuth_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new CreateItemCategoryMasterRequest
            {
                CategoryName = "TEST",
                CompanyId = 1,
                IsAutoShortClose = false,
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await Client.PostAsync("/api/ItemCategoryMaster", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateItemCategory_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = "CAT" + Guid.NewGuid().ToString().Substring(0, 4);
            var request = new CreateItemCategoryMasterRequest
            {
                CategoryName = uniqueName,
                CompanyId = 1,
                IsAutoShortClose = false,
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Act
                var response = await Client.PostAsync("/api/ItemCategoryMaster", requestContent);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                
                var content = await response.Content.ReadAsStringAsync();
                var categoryId = int.Parse(content);
                categoryId.Should().NotBe(0); // Accept any non-zero ID (positive or negative)

                // Cleanup
                await Client.DeleteAsync($"/api/ItemCategoryMaster/{categoryId}");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CreateItemCategory_WithDuplicateName_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = "DUP" + Guid.NewGuid().ToString().Substring(0, 4);
            var request = new CreateItemCategoryMasterRequest
            {
                CategoryName = uniqueName,
                CompanyId = 1,
                IsAutoShortClose = false,
                IsActive = true
            };

            var json = JsonSerializer.Serialize(request);
            var content1 = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Act - Create first category
                var response1 = await Client.PostAsync("/api/ItemCategoryMaster", content1);
                response1.EnsureSuccessStatusCode();
                var categoryId = int.Parse(await response1.Content.ReadAsStringAsync());

                // Act - Try to create duplicate
                var content2 = new StringContent(json, Encoding.UTF8, "application/json");
                var response2 = await Client.PostAsync("/api/ItemCategoryMaster", content2);

                // Assert
                response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Cleanup
                await Client.DeleteAsync($"/api/ItemCategoryMaster/{categoryId}");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetItemCategoryById_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = "GET" + Guid.NewGuid().ToString().Substring(0, 4);
            var createRequest = new CreateItemCategoryMasterRequest
            {
                CategoryName = uniqueName,
                CompanyId = 1,
                IsAutoShortClose = false,
                IsActive = true
            };

            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/ItemCategoryMaster", createContent);
            var categoryId = int.Parse(await createResponse.Content.ReadAsStringAsync());

            try
            {
                // Act
                var response = await Client.GetAsync($"/api/ItemCategoryMaster/{categoryId}");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var content = await response.Content.ReadAsStringAsync();
                var category = JsonSerializer.Deserialize<ItemCategoryMasterDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                category.Should().NotBeNull();
                category!.CategoryName.Should().Be(uniqueName.ToUpper()); // Handler converts to uppercase
            }
            finally
            {
                // Cleanup
                await Client.DeleteAsync($"/api/ItemCategoryMaster/{categoryId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetItemCategoryById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act
                var response = await Client.GetAsync("/api/ItemCategoryMaster/999999");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task UpdateItemCategory_WithValidData_ShouldReturnNoContent()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = "UPD" + Guid.NewGuid().ToString().Substring(0, 4);
            var createRequest = new CreateItemCategoryMasterRequest
            {
                CategoryName = uniqueName,
                CompanyId = 1,
                IsAutoShortClose = false,
                IsActive = true
            };

            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/ItemCategoryMaster", createContent);
            var categoryId = int.Parse(await createResponse.Content.ReadAsStringAsync());

            try
            {
                // Act
                var updatedName = "UPDATED" + Guid.NewGuid().ToString().Substring(0, 3);
                var updateRequest = new UpdateItemCategoryMasterRequest
                {
                    CategoryId = categoryId,
                    CategoryName = updatedName,
                    IsAutoShortClose = true,
                    IsActive = true
                };

                var updateJson = JsonSerializer.Serialize(updateRequest);
                var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");
                var updateResponse = await Client.PutAsync("/api/ItemCategoryMaster", updateContent);

                // Assert
                updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

                // Verify update
                var getResponse = await Client.GetAsync($"/api/ItemCategoryMaster/{categoryId}");
                var content = await getResponse.Content.ReadAsStringAsync();
                var category = JsonSerializer.Deserialize<ItemCategoryMasterDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                category!.CategoryName.Should().Be(updatedName.ToUpper()); // Handler converts to uppercase
                category.IsAutoShortClose.Should().BeTrue();
            }
            finally
            {
                // Cleanup
                await Client.DeleteAsync($"/api/ItemCategoryMaster/{categoryId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task DeleteItemCategory_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = "DEL" + Guid.NewGuid().ToString().Substring(0, 4);
            var createRequest = new CreateItemCategoryMasterRequest
            {
                CategoryName = uniqueName,
                CompanyId = 1,
                IsAutoShortClose = false,
                IsActive = true
            };

            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/ItemCategoryMaster", createContent);
            var categoryId = int.Parse(await createResponse.Content.ReadAsStringAsync());

            try
            {
                // Act
                var deleteResponse = await Client.DeleteAsync($"/api/ItemCategoryMaster/{categoryId}");

                // Assert
                deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

                // Verify deletion (soft delete - should still exist but marked as deleted)
                var getResponse = await Client.GetAsync($"/api/ItemCategoryMaster/{categoryId}");
                getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetItemCategoryByName_WithExistingCategory_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = "NAME" + Guid.NewGuid().ToString().Substring(0, 4);
            var createRequest = new CreateItemCategoryMasterRequest
            {
                CategoryName = uniqueName,
                CompanyId = 1,
                IsAutoShortClose = false,
                IsActive = true
            };

            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/ItemCategoryMaster", createContent);
            var categoryId = int.Parse(await createResponse.Content.ReadAsStringAsync());

            try
            {
                // Act
                var response = await Client.GetAsync($"/api/ItemCategoryMaster/name/{uniqueName}?companyId=1");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var content = await response.Content.ReadAsStringAsync();
                var category = JsonSerializer.Deserialize<ItemCategoryMasterDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                category.Should().NotBeNull();
                category!.CategoryName.Should().Be(uniqueName.ToUpper()); // Handler converts to uppercase
            }
            finally
            {
                // Cleanup
                await Client.DeleteAsync($"/api/ItemCategoryMaster/{categoryId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetItemCategoryByName_WithNonExistingCategory_ShouldReturnNotFound()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act
                var response = await Client.GetAsync($"/api/ItemCategoryMaster/name/NONEXISTENT_{Guid.NewGuid()}?companyId=1");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CheckCategoryNameUnique_WithUniqueName_ShouldReturnTrue()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = "UNIQUE" + Guid.NewGuid().ToString().Substring(0, 8);

            try
            {
                // Act
                var response = await Client.GetAsync($"/api/ItemCategoryMaster/check-unique?categoryName={uniqueName}&companyId=1");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                result.GetProperty("isUnique").GetBoolean().Should().BeTrue();
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CheckCategoryNameUnique_WithExistingName_ShouldReturnFalse()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var existingName = "CHK" + Guid.NewGuid().ToString().Substring(0, 4);
            var createRequest = new CreateItemCategoryMasterRequest
            {
                CategoryName = existingName,
                CompanyId = 1,
                IsAutoShortClose = false,
                IsActive = true
            };

            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/ItemCategoryMaster", createContent);
            var categoryId = int.Parse(await createResponse.Content.ReadAsStringAsync());

            try
            {
                // Act
                var response = await Client.GetAsync($"/api/ItemCategoryMaster/check-unique?categoryName={existingName}&companyId=1");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                result.GetProperty("isUnique").GetBoolean().Should().BeFalse();
            }
            finally
            {
                // Cleanup
                await Client.DeleteAsync($"/api/ItemCategoryMaster/{categoryId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CheckCategoryNameUnique_WithExcludeId_ShouldReturnTrue()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var categoryName = "EXC" + Guid.NewGuid().ToString().Substring(0, 4);
            var createRequest = new CreateItemCategoryMasterRequest
            {
                CategoryName = categoryName,
                CompanyId = 1,
                IsAutoShortClose = false,
                IsActive = true
            };

            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/ItemCategoryMaster", createContent);
            var categoryId = int.Parse(await createResponse.Content.ReadAsStringAsync());

            try
            {
                // Act - Check uniqueness excluding the current category's ID
                var response = await Client.GetAsync($"/api/ItemCategoryMaster/check-unique?categoryName={categoryName}&companyId=1&excludeId={categoryId}");

                // Assert - Should return true because we're excluding the only record with this name
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);
                result.GetProperty("isUnique").GetBoolean().Should().BeTrue();
            }
            finally
            {
                // Cleanup
                await Client.DeleteAsync($"/api/ItemCategoryMaster/{categoryId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetItemCategories_WithPagination_ShouldReturnPagedResults()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act
                var response = await Client.GetAsync("/api/ItemCategoryMaster?CompanyId=1&PageNumber=1&PageSize=5");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PagedResponse<ItemCategoryMasterDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                result.Should().NotBeNull();
                result!.PageNumber.Should().Be(1);
                result.PageSize.Should().Be(5);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetItemCategories_WithSearch_ShouldReturnFilteredResults()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var searchTerm = "SRCH" + Guid.NewGuid().ToString().Substring(0, 3);
            var createRequest = new CreateItemCategoryMasterRequest
            {
                CategoryName = searchTerm,
                CompanyId = 1,
                IsAutoShortClose = false,
                IsActive = true
            };

            var createJson = JsonSerializer.Serialize(createRequest);
            var createContent = new StringContent(createJson, Encoding.UTF8, "application/json");
            var createResponse = await Client.PostAsync("/api/ItemCategoryMaster", createContent);
            var categoryId = int.Parse(await createResponse.Content.ReadAsStringAsync());

            try
            {
                // Act
                var response = await Client.GetAsync($"/api/ItemCategoryMaster?CompanyId=1&SearchTerm={searchTerm}");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PagedResponse<ItemCategoryMasterDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                result.Should().NotBeNull();
                result!.Data.Should().Contain(c => c.CategoryName == searchTerm.ToUpper()); // Handler converts to uppercase
            }
            finally
            {
                // Cleanup
                await Client.DeleteAsync($"/api/ItemCategoryMaster/{categoryId}");
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CreateItemCategory_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateItemCategoryMasterRequest
            {
                CategoryName = "", // Empty name
                CompanyId = 1
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Act
                var response = await Client.PostAsync("/api/ItemCategoryMaster", content);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}

