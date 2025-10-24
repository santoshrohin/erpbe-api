using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ErpBE.Application.Common.Models;
using ErpBE.Application.DTOs;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ErpBE.Tests.CustomerTypeMaster
{
    public class CustomerTypeMasterControllerTests : IntegrationTestBase
    {
        public CustomerTypeMasterControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateCustomerTypeMaster_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueCode = $"TEST_CT_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var request = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = uniqueCode,
                TypeDescription = "Test Customer Type",
                FirstLetter = "T"
            };

            try
            {
                // Act
                var response = await Client.PostAsJsonAsync("/api/CustomerTypeMaster", request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.Created);
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                var jsonElement = (JsonElement)result!;
                var customerTypeId = jsonElement.GetProperty("id").GetInt32();
                customerTypeId.Should().NotBe(0);

                // Cleanup
                await Client.DeleteAsync($"/api/CustomerTypeMaster/{customerTypeId}?companyId=1");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CreateCustomerTypeMaster_WithDuplicateTypeCode_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueCode = $"TEST_CT_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var request = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = uniqueCode,
                TypeDescription = "Test Customer Type",
                FirstLetter = "T"
            };

            try
            {
                // Create first
                var createResponse = await Client.PostAsJsonAsync("/api/CustomerTypeMaster", request);
                createResponse.EnsureSuccessStatusCode();
                var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
                var jsonElement = (JsonElement)createResult!;
                var customerTypeId = jsonElement.GetProperty("id").GetInt32();

                // Act - Try to create duplicate
                var response = await Client.PostAsJsonAsync("/api/CustomerTypeMaster", request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Cleanup
                await Client.DeleteAsync($"/api/CustomerTypeMaster/{customerTypeId}?companyId=1");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Theory]
        [InlineData("", "Test Description", "T")]
        [InlineData("TEST_CODE", "", "T")]
        [InlineData("TEST_CODE", "Test Description", "")]
        public async Task CreateCustomerTypeMaster_WithMissingRequiredFields_ShouldReturnBadRequest(
            string typeCode, string typeDescription, string firstLetter)
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = typeCode,
                TypeDescription = typeDescription,
                FirstLetter = firstLetter
            };

            try
            {
                // Act
                var response = await Client.PostAsJsonAsync("/api/CustomerTypeMaster", request);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task UpdateCustomerTypeMaster_WithValidData_ShouldReturnNoContent()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueCode = $"TEST_CT_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var createRequest = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = uniqueCode,
                TypeDescription = "Test Customer Type",
                FirstLetter = "T"
            };

            try
            {
                // Create
                var createResponse = await Client.PostAsJsonAsync("/api/CustomerTypeMaster", createRequest);
                createResponse.EnsureSuccessStatusCode();
                var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
                var jsonElement = (JsonElement)createResult!;
                var customerTypeId = jsonElement.GetProperty("id").GetInt32();

                // Act - Update
                var updateRequest = new UpdateCustomerTypeMasterRequest
                {
                    Id = customerTypeId,
                    CompanyId = 1,
                    TypeCode = uniqueCode,
                    TypeDescription = "Updated Customer Type Description",
                    FirstLetter = "U"
                };

                var response = await Client.PutAsJsonAsync($"/api/CustomerTypeMaster/{customerTypeId}", updateRequest);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                // Cleanup
                await Client.DeleteAsync($"/api/CustomerTypeMaster/{customerTypeId}?companyId=1");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task UpdateCustomerTypeMaster_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var updateRequest = new UpdateCustomerTypeMasterRequest
            {
                Id = 999999,
                CompanyId = 1,
                TypeCode = "TEST_CODE",
                TypeDescription = "Test Description",
                FirstLetter = "T"
            };

            try
            {
                // Act
                var response = await Client.PutAsJsonAsync($"/api/CustomerTypeMaster/999999", updateRequest);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task DeleteCustomerTypeMaster_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueCode = $"TEST_CT_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var createRequest = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = uniqueCode,
                TypeDescription = "Test Customer Type",
                FirstLetter = "T"
            };

            try
            {
                // Create
                var createResponse = await Client.PostAsJsonAsync("/api/CustomerTypeMaster", createRequest);
                createResponse.EnsureSuccessStatusCode();
                var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
                var jsonElement = (JsonElement)createResult!;
                var customerTypeId = jsonElement.GetProperty("id").GetInt32();

                // Act
                var response = await Client.DeleteAsync($"/api/CustomerTypeMaster/{customerTypeId}?companyId=1");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task DeleteCustomerTypeMaster_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act
                var response = await Client.DeleteAsync($"/api/CustomerTypeMaster/999999?companyId=1");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetCustomerTypeMasterById_WithValidId_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueCode = $"TEST_CT_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var createRequest = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = uniqueCode,
                TypeDescription = "Test Customer Type",
                FirstLetter = "T"
            };

            try
            {
                // Create
                var createResponse = await Client.PostAsJsonAsync("/api/CustomerTypeMaster", createRequest);
                createResponse.EnsureSuccessStatusCode();
                var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
                var jsonElement = (JsonElement)createResult!;
                var customerTypeId = jsonElement.GetProperty("id").GetInt32();

                // Act
                var response = await Client.GetAsync($"/api/CustomerTypeMaster/{customerTypeId}?companyId=1");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadFromJsonAsync<CustomerTypeMasterDto>();
                result.Should().NotBeNull();
                result!.TypeCode.Should().Be(uniqueCode);

                // Cleanup
                await Client.DeleteAsync($"/api/CustomerTypeMaster/{customerTypeId}?companyId=1");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetCustomerTypeMasterById_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act
                var response = await Client.GetAsync($"/api/CustomerTypeMaster/999999?companyId=1");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetCustomerTypeMasters_WithPagination_ShouldReturnPagedResults()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            try
            {
                // Act
                var response = await Client.GetAsync("/api/CustomerTypeMaster?CompanyId=1&PageNumber=1&PageSize=10&SortDirection=ASC");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerTypeMasterDto>>();
                result.Should().NotBeNull();
                result!.PageNumber.Should().Be(1);
                result.PageSize.Should().Be(10);
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetCustomerTypeMasters_WithSearch_ShouldReturnFilteredResults()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueCode = $"SEARCH_TEST_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var createRequest = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = uniqueCode,
                TypeDescription = "Searchable Test Customer Type",
                FirstLetter = "S"
            };

            try
            {
                // Create test record
                var createResponse = await Client.PostAsJsonAsync("/api/CustomerTypeMaster", createRequest);
                createResponse.EnsureSuccessStatusCode();
                var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
                var jsonElement = (JsonElement)createResult!;
                var customerTypeId = jsonElement.GetProperty("id").GetInt32();

                // Act
                var response = await Client.GetAsync($"/api/CustomerTypeMaster?CompanyId=1&SearchTerm={uniqueCode}&PageNumber=1&PageSize=10&SortDirection=ASC");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerTypeMasterDto>>();
                result.Should().NotBeNull();
                result!.Data.Should().Contain(ct => ct.TypeCode == uniqueCode);

                // Cleanup
                await Client.DeleteAsync($"/api/CustomerTypeMaster/{customerTypeId}?companyId=1");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task GetCustomerTypeMasterByTypeCode_WithValidCode_ShouldReturnOk()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueCode = $"TEST_CT_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var createRequest = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = uniqueCode,
                TypeDescription = "Test Customer Type",
                FirstLetter = "T"
            };

            try
            {
                // Create
                var createResponse = await Client.PostAsJsonAsync("/api/CustomerTypeMaster", createRequest);
                createResponse.EnsureSuccessStatusCode();
                var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
                var jsonElement = (JsonElement)createResult!;
                var customerTypeId = jsonElement.GetProperty("id").GetInt32();

                // Act
                var response = await Client.GetAsync($"/api/CustomerTypeMaster/byTypeCode/{uniqueCode}?companyId=1");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadFromJsonAsync<CustomerTypeMasterDto>();
                result.Should().NotBeNull();
                result!.TypeCode.Should().Be(uniqueCode);

                // Cleanup
                await Client.DeleteAsync($"/api/CustomerTypeMaster/{customerTypeId}?companyId=1");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CheckTypeCodeUnique_WithUniqueCode_ShouldReturnTrue()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueCode = $"UNIQUE_{Guid.NewGuid().ToString().Substring(0, 8)}";

            try
            {
                // Act
                var response = await Client.GetAsync($"/api/CustomerTypeMaster/checkUnique?typeCode={uniqueCode}&companyId=1");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadFromJsonAsync<bool>();
                result.Should().BeTrue();
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CheckTypeCodeUnique_WithExistingCode_ShouldReturnFalse()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueCode = $"TEST_CT_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var createRequest = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = uniqueCode,
                TypeDescription = "Test Customer Type",
                FirstLetter = "T"
            };

            try
            {
                // Create
                var createResponse = await Client.PostAsJsonAsync("/api/CustomerTypeMaster", createRequest);
                createResponse.EnsureSuccessStatusCode();
                var createResult = await createResponse.Content.ReadFromJsonAsync<dynamic>();
                var jsonElement = (JsonElement)createResult!;
                var customerTypeId = jsonElement.GetProperty("id").GetInt32();

                // Act
                var response = await Client.GetAsync($"/api/CustomerTypeMaster/checkUnique?typeCode={uniqueCode}&companyId=1");

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadFromJsonAsync<bool>();
                result.Should().BeFalse();

                // Cleanup
                await Client.DeleteAsync($"/api/CustomerTypeMaster/{customerTypeId}?companyId=1");
            }
            finally
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }
        }

        [Fact]
        public async Task CreateCustomerTypeMaster_WithUnauthorized_ShouldReturn401()
        {
            // Arrange
            var request = new CreateCustomerTypeMasterRequest
            {
                CompanyId = 1,
                TypeCode = "TEST_CODE",
                TypeDescription = "Test Description",
                FirstLetter = "T"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/CustomerTypeMaster", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}

