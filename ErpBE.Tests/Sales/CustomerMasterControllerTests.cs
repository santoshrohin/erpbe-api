using ErpBE.Application.DTOs;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ErpBE.Tests.Sales
{
    public class CustomerMasterControllerTests : IntegrationTestBase
    {
        public CustomerMasterControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateCustomerMaster_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"Test Customer {Guid.NewGuid().ToString().Substring(0, 8)}",
                ContactPerson = "John Doe",
                Abbreviation = $"TC{Guid.NewGuid().ToString().Substring(0, 4)}",
                VendorCode = "VC001",
                Address = "123 Test Street",
                Phone = "1234567890",
                Mobile = "9876543210",
                Email = "test@customer.com",
                FaxNo = "1234567",
                PinCode = "400001",
                AreaCode = 1,
                CustomerType = "1",
                CountryCode = 1,
                StateCode = 1,
                CityCode = 1,
                CategoryCode = 1,
                EmployeeCode = 1,
                PanNo = "ABCDE1234F",
                CstNo = "CST123",
                VatNo = "VAT456",
                ServiceTaxNo = "ST789",
                EccNo = "ECC123",
                LbtNo = "GST123456",
                ExciseRange = "Range1",
                ExciseDivision = "Division1",
                ExciseCollectorate = "Collectorate1",
                TallyName = "Tally Customer",
                CreditDays = 30,
                TdsPercentage = 2.5,
                IsActive = true,
                IsLbtApplicable = true
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/CustomerMaster", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdCustomer = await response.Content.ReadFromJsonAsync<CustomerMasterDto>();
            createdCustomer.Should().NotBeNull();
            createdCustomer!.Id.Should().NotBe(0);
            createdCustomer.PartyName.Should().Be(request.PartyName);
            createdCustomer.Email.Should().Be(request.Email);

            // Cleanup
            await Client.DeleteAsync($"/api/CustomerMaster/{createdCustomer.Id}?companyId={request.CompanyId}");
        }

        [Fact]
        public async Task CreateCustomerMaster_WithMissingRequiredFields_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                // Missing PartyName (required)
                AreaCode = 1,
                CustomerType = "1"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/CustomerMaster", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCustomerMaster_WithInvalidEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"Test Customer {Guid.NewGuid().ToString().Substring(0, 8)}",
                Email = "invalid-email",  // Invalid email format
                AreaCode = 1,
                CustomerType = "1"
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/CustomerMaster", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCustomerMaster_WithLbtApplicableButNoLbtNo_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"Test Customer {Guid.NewGuid().ToString().Substring(0, 8)}",
                AreaCode = 1,
                CustomerType = "1",
                IsLbtApplicable = true,
                // Missing LbtNo (required when IsLbtApplicable is true)
            };

            // Act
            var response = await Client.PostAsJsonAsync("/api/CustomerMaster", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetCustomerMasterById_WithValidId_ShouldReturnCustomer()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create a customer first
            var createRequest = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"Test Customer {Guid.NewGuid().ToString().Substring(0, 8)}",
                AreaCode = 1,
                CustomerType = "1",
                IsActive = true
            };

            var createResponse = await Client.PostAsJsonAsync("/api/CustomerMaster", createRequest);
            var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();

            // Act
            var response = await Client.GetAsync($"/api/CustomerMaster/{createdCustomer!.Id}?companyId={createRequest.CompanyId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var customer = await response.Content.ReadFromJsonAsync<CustomerMasterDto>();
            customer.Should().NotBeNull();
            customer!.Id.Should().Be(createdCustomer.Id);
            customer.PartyName.Should().Be(createRequest.PartyName);

            // Cleanup
            await Client.DeleteAsync($"/api/CustomerMaster/{createdCustomer.Id}?companyId={createRequest.CompanyId}");
        }

        [Fact]
        public async Task GetCustomerMasterById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/CustomerMaster/999999?companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAllCustomerMasters_ShouldReturnPagedResults()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/CustomerMaster?CompanyId=1&PageNumber=1&PageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerMasterDto>>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeNull();
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetAllCustomerMasters_WithFilters_ShouldReturnFilteredResults()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.GetAsync("/api/CustomerMaster?CompanyId=1&IsActive=true&PageNumber=1&PageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerMasterDto>>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeNull();
            result.Data.Should().OnlyContain(c => c.IsActive == true);
        }

        [Fact]
        public async Task GetAllCustomerMasters_WithSearchTerm_ShouldReturnMatchingResults()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create a customer with unique name
            var uniqueName = $"SearchTest{Guid.NewGuid().ToString().Substring(0, 8)}";
            var createRequest = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = uniqueName,
                AreaCode = 1,
                CustomerType = "1",
                IsActive = true
            };

            var createResponse = await Client.PostAsJsonAsync("/api/CustomerMaster", createRequest);
            var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();

            // Act
            var response = await Client.GetAsync($"/api/CustomerMaster?CompanyId=1&SearchTerm={uniqueName}&PageNumber=1&PageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerMasterDto>>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeEmpty();
            result.Data.Should().Contain(c => c.PartyName == uniqueName);

            // Cleanup
            await Client.DeleteAsync($"/api/CustomerMaster/{createdCustomer!.Id}?companyId={createRequest.CompanyId}");
        }

        [Fact]
        public async Task UpdateCustomerMaster_WithValidData_ShouldReturnNoContent()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create a customer first
            var createRequest = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"Test Customer {Guid.NewGuid().ToString().Substring(0, 8)}",
                AreaCode = 1,
                CustomerType = "1",
                IsActive = true
            };

            var createResponse = await Client.PostAsJsonAsync("/api/CustomerMaster", createRequest);
            var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();

            // Update request
            var updateRequest = new UpdateCustomerMasterRequest
            {
                Id = createdCustomer!.Id,
                CompanyId = createdCustomer.CompanyId ?? 1,
                PartyCode = createdCustomer.PartyCode ?? 1,
                PartyName = "Updated Customer Name",
                ContactPerson = "Jane Doe",
                AreaCode = createdCustomer.AreaCode ?? 1,
                CustomerType = createdCustomer.CustomerType ?? "1",
                IsActive = true
            };

            // Act
            var response = await Client.PutAsJsonAsync($"/api/CustomerMaster/{createdCustomer.Id}", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify update
            var getResponse = await Client.GetAsync($"/api/CustomerMaster/{createdCustomer.Id}?companyId={updateRequest.CompanyId}");
            var updatedCustomer = await getResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();
            updatedCustomer!.PartyName.Should().Be("Updated Customer Name");
            updatedCustomer.ContactPerson.Should().Be("Jane Doe");

            // Cleanup
            await Client.DeleteAsync($"/api/CustomerMaster/{createdCustomer.Id}?companyId={updateRequest.CompanyId}");
        }

        [Fact]
        public async Task UpdateCustomerMaster_WithIdMismatch_ShouldReturnBadRequest()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var updateRequest = new UpdateCustomerMasterRequest
            {
                Id = 100,
                CompanyId = 1,
                PartyCode = 1,
                PartyName = "Test",
                AreaCode = 1,
                CustomerType = "1"
            };

            // Act
            var response = await Client.PutAsJsonAsync("/api/CustomerMaster/200", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeleteCustomerMaster_WithValidId_ShouldReturnNoContent()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create a customer first
            var createRequest = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"Test Customer {Guid.NewGuid().ToString().Substring(0, 8)}",
                AreaCode = 1,
                CustomerType = "1",
                IsActive = true
            };

            var createResponse = await Client.PostAsJsonAsync("/api/CustomerMaster", createRequest);
            var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();

            // Act
            var response = await Client.DeleteAsync($"/api/CustomerMaster/{createdCustomer!.Id}?companyId={createRequest.CompanyId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify deletion
            var getResponse = await Client.GetAsync($"/api/CustomerMaster/{createdCustomer.Id}?companyId={createRequest.CompanyId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteCustomerMaster_WithInvalidId_ShouldHandleGracefully()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await Client.DeleteAsync("/api/CustomerMaster/999999?companyId=1");

            // Assert
            // Should either return NotFound or handle gracefully
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.NoContent, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task CheckPartyNameUnique_WithUniqueName_ShouldReturnTrue()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueName = $"UniqueCustomer{Guid.NewGuid().ToString().Substring(0, 8)}";

            // Act
            var response = await Client.GetAsync($"/api/CustomerMaster/check-partyname?partyName={uniqueName}&companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, bool>>();
            result.Should().NotBeNull();
            result!["isUnique"].Should().BeTrue();
        }

        [Fact]
        public async Task CheckPartyNameUnique_WithExistingName_ShouldReturnFalse()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Create a customer first
            var createRequest = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"Test Customer {Guid.NewGuid().ToString().Substring(0, 8)}",
                AreaCode = 1,
                CustomerType = "1",
                IsActive = true
            };

            var createResponse = await Client.PostAsJsonAsync("/api/CustomerMaster", createRequest);
            var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();

            // Act
            var response = await Client.GetAsync($"/api/CustomerMaster/check-partyname?partyName={createRequest.PartyName}&companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, bool>>();
            result.Should().NotBeNull();
            result!["isUnique"].Should().BeFalse();

            // Cleanup
            await Client.DeleteAsync($"/api/CustomerMaster/{createdCustomer!.Id}?companyId={createRequest.CompanyId}");
        }

        [Fact]
        public async Task CheckAbbreviationUnique_WithUniqueAbbreviation_ShouldReturnTrue()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueAbbr = $"UNQ{Guid.NewGuid().ToString().Substring(0, 4)}";

            // Act
            var response = await Client.GetAsync($"/api/CustomerMaster/check-abbreviation?abbreviation={uniqueAbbr}&companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, bool>>();
            result.Should().NotBeNull();
            result!["isUnique"].Should().BeTrue();
        }

        [Fact]
        public async Task CheckAbbreviationUnique_WithExistingAbbreviation_ShouldReturnFalse()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var uniqueAbbr = $"TST{Guid.NewGuid().ToString().Substring(0, 4)}";

            // Create a customer first
            var createRequest = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"Test Customer {Guid.NewGuid().ToString().Substring(0, 8)}",
                Abbreviation = uniqueAbbr,
                AreaCode = 1,
                CustomerType = "1",
                IsActive = true
            };

            var createResponse = await Client.PostAsJsonAsync("/api/CustomerMaster", createRequest);
            var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();

            // Act
            var response = await Client.GetAsync($"/api/CustomerMaster/check-abbreviation?abbreviation={uniqueAbbr}&companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, bool>>();
            result.Should().NotBeNull();
            result!["isUnique"].Should().BeFalse();

            // Cleanup
            await Client.DeleteAsync($"/api/CustomerMaster/{createdCustomer!.Id}?companyId={createRequest.CompanyId}");
        }
    }

    // Helper class for paged response
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}

