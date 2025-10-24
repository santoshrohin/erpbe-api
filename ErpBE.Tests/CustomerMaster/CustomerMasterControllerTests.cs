using ErpBE.Application.DTOs;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ErpBE.Tests.CustomerMaster
{
    public class CustomerMasterControllerTests : IntegrationTestBase
    {
        private readonly string _baseUrl = "/api/CustomerMaster";

        public CustomerMasterControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task CreateCustomerMaster_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var token = await GetAuthTokenAsync();
            Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
            var request = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"Test Customer {Guid.NewGuid()}",
                ContactPerson = "John Doe",
                Abbreviation = $"TC{Guid.NewGuid().ToString().Substring(0, 4)}",
                Address = "123 Test Street",
                Phone = "1234567890",
                Mobile = "9876543210",
                Email = "test@customer.com",
                Website = "www.testcustomer.com",
                FaxNo = "1234567890",
                AreaCode = 1,
                CustomerType = 1,
                CountryCode = 1,
                StateCode = 1,
                CityCode = 1,
                PinCode = "123456",
                VatTinNo = "VAT123",
                CstNo = "CST123",
                GstNo = "GST123",
                PanNo = "ABCDE1234F",
                ServiceTaxNo = "ST123",
                TallyName = "Test Customer Tally",
                OpeningBalance = 1000.00m,
                OpeningBalanceType = "CR",
                CreditLimit = 50000.00m,
                CreditDays = 30,
                BankName = "Test Bank",
                BankAccountNo = "1234567890",
                BankBranchName = "Test Branch",
                BankIfscCode = "TEST0001234",
                IsLbtApplicable = true,
                IsSezCustomer = false,
                IsCompositeDealer = false,
                Remark = "Test customer record"
            };

            // Act
            var response = await Client.PostAsJsonAsync(_baseUrl, request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var result = await response.Content.ReadFromJsonAsync<CustomerMasterDto>();
            result.Should().NotBeNull();
            result!.Id.Should().BeGreaterThan(0);
            result.PartyName.Should().Be(request.PartyName);
            result.ContactPerson.Should().Be(request.ContactPerson);
            result.Abbreviation.Should().Be(request.Abbreviation.ToUpper()); // Legacy converts to uppercase
            result.Email.Should().Be(request.Email);

            // Cleanup
            await Client.DeleteAsync($"{_baseUrl}/{result.Id}?companyId={request.CompanyId}");
        }

        [Fact]
        public async Task CreateCustomerMaster_WithMissingRequiredFields_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = "", // Missing required field
                AreaCode = 0, // Missing required field
                CustomerType = 0 // Missing required field
            };

            // Act
            var response = await Client.PostAsJsonAsync(_baseUrl, request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCustomerMaster_WithDuplicatePartyName_ShouldReturnBadRequest()
        {
            // Arrange - Create first customer
            var firstRequest = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"Duplicate Test {Guid.NewGuid()}",
                AreaCode = 1,
                CustomerType = 1
            };

            var firstResponse = await Client.PostAsJsonAsync(_baseUrl, firstRequest);
            var firstResult = await firstResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();

            // Act - Try to create second customer with same name
            var secondRequest = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = firstRequest.PartyName, // Duplicate name
                AreaCode = 1,
                CustomerType = 1
            };

            var secondResponse = await Client.PostAsJsonAsync(_baseUrl, secondRequest);

            // Assert
            secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Cleanup
            await Client.DeleteAsync($"{_baseUrl}/{firstResult!.Id}?companyId={firstRequest.CompanyId}");
        }

        [Fact]
        public async Task CreateCustomerMaster_WithLbtApplicableButNoGstNo_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"LBT Test {Guid.NewGuid()}",
                AreaCode = 1,
                CustomerType = 1,
                IsLbtApplicable = true,
                GstNo = null // Missing GST when LBT is applicable
            };

            // Act
            var response = await Client.PostAsJsonAsync(_baseUrl, request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateCustomerMaster_WithInvalidEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"Email Test {Guid.NewGuid()}",
                AreaCode = 1,
                CustomerType = 1,
                Email = "invalid-email" // Invalid email format
            };

            // Act
            var response = await Client.PostAsJsonAsync(_baseUrl, request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetCustomerMasterById_WithValidId_ShouldReturnCustomer()
        {
            // Arrange - Create a customer first
            var createRequest = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"GetById Test {Guid.NewGuid()}",
                AreaCode = 1,
                CustomerType = 1
            };

            var createResponse = await Client.PostAsJsonAsync(_baseUrl, createRequest);
            var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();

            // Act
            var response = await Client.GetAsync($"{_baseUrl}/{createdCustomer!.Id}?companyId={createRequest.CompanyId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<CustomerMasterDto>();
            result.Should().NotBeNull();
            result!.Id.Should().Be(createdCustomer.Id);
            result.PartyName.Should().Be(createdCustomer.PartyName);

            // Cleanup
            await Client.DeleteAsync($"{_baseUrl}/{createdCustomer.Id}?companyId={createRequest.CompanyId}");
        }

        [Fact]
        public async Task GetCustomerMasterById_WithNonExistentId_ShouldReturnNotFound()
        {
            // Act
            var response = await Client.GetAsync($"{_baseUrl}/999999?companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetCustomerMasters_WithPagination_ShouldReturnPagedResults()
        {
            // Act
            var response = await Client.GetAsync($"{_baseUrl}?CompanyId=1&PageNumber=1&PageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResponseDto<CustomerMasterDto>>();
            result.Should().NotBeNull();
            result!.Data.Should().NotBeNull();
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetCustomerMasters_WithSearchTerm_ShouldReturnFilteredResults()
        {
            // Arrange - Create a customer with specific name
            var uniqueName = $"SearchTest{Guid.NewGuid().ToString().Substring(0, 8)}";
            var createRequest = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = uniqueName,
                AreaCode = 1,
                CustomerType = 1
            };

            var createResponse = await Client.PostAsJsonAsync(_baseUrl, createRequest);
            var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();

            // Act
            var response = await Client.GetAsync($"{_baseUrl}?CompanyId=1&SearchTerm={uniqueName}&PageNumber=1&PageSize=10");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<PagedResponseDto<CustomerMasterDto>>();
            result.Should().NotBeNull();
            result!.Data.Should().Contain(c => c.PartyName == uniqueName);

            // Cleanup
            await Client.DeleteAsync($"{_baseUrl}/{createdCustomer!.Id}?companyId={createRequest.CompanyId}");
        }

        [Fact]
        public async Task UpdateCustomerMaster_WithValidData_ShouldReturnNoContent()
        {
            // Arrange - Create a customer first
            var createRequest = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"Update Test {Guid.NewGuid()}",
                AreaCode = 1,
                CustomerType = 1
            };

            var createResponse = await Client.PostAsJsonAsync(_baseUrl, createRequest);
            var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();

            // Act - Update the customer
            var updateRequest = new UpdateCustomerMasterRequest
            {
                Id = createdCustomer!.Id,
                CompanyId = createdCustomer.CompanyId,
                PartyCode = createdCustomer.PartyCode,
                PartyName = "Updated Customer Name",
                ContactPerson = "Updated Contact",
                AreaCode = createdCustomer.AreaCode ?? 1,
                CustomerType = createdCustomer.CustomerType ?? 1
            };

            var updateResponse = await Client.PutAsJsonAsync($"{_baseUrl}/{createdCustomer.Id}", updateRequest);

            // Assert
            updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the update
            var getResponse = await Client.GetAsync($"{_baseUrl}/{createdCustomer.Id}?companyId={createdCustomer.CompanyId}");
            var updatedCustomer = await getResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();
            updatedCustomer!.PartyName.Should().Be("Updated Customer Name");
            updatedCustomer.ContactPerson.Should().Be("Updated Contact");

            // Cleanup
            await Client.DeleteAsync($"{_baseUrl}/{createdCustomer.Id}?companyId={createdCustomer.CompanyId}");
        }

        [Fact]
        public async Task UpdateCustomerMaster_WithNonExistentId_ShouldReturnNotFound()
        {
            // Arrange
            var updateRequest = new UpdateCustomerMasterRequest
            {
                Id = 999999,
                CompanyId = 1,
                PartyCode = "TEST999",
                PartyName = "Non-Existent Customer",
                AreaCode = 1,
                CustomerType = 1
            };

            // Act
            var response = await Client.PutAsJsonAsync($"{_baseUrl}/999999", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteCustomerMaster_WithValidId_ShouldReturnNoContent()
        {
            // Arrange - Create a customer first
            var createRequest = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"Delete Test {Guid.NewGuid()}",
                AreaCode = 1,
                CustomerType = 1
            };

            var createResponse = await Client.PostAsJsonAsync(_baseUrl, createRequest);
            var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();

            // Act
            var deleteResponse = await Client.DeleteAsync($"{_baseUrl}/{createdCustomer!.Id}?companyId={createdCustomer.CompanyId}");

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verify the customer is deleted
            var getResponse = await Client.GetAsync($"{_baseUrl}/{createdCustomer.Id}?companyId={createdCustomer.CompanyId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteCustomerMaster_WithNonExistentId_ShouldReturnNotFound()
        {
            // Act
            var response = await Client.DeleteAsync($"{_baseUrl}/999999?companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CheckPartyNameUnique_WithUniqueName_ShouldReturnTrue()
        {
            // Arrange
            var uniqueName = $"Unique{Guid.NewGuid()}";

            // Act
            var response = await Client.GetAsync($"{_baseUrl}/check-party-name-unique?partyName={uniqueName}&companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<bool>();
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CheckPartyNameUnique_WithDuplicateName_ShouldReturnFalse()
        {
            // Arrange - Create a customer first
            var createRequest = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"UniqueCheck Test {Guid.NewGuid()}",
                AreaCode = 1,
                CustomerType = 1
            };

            var createResponse = await Client.PostAsJsonAsync(_baseUrl, createRequest);
            var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();

            // Act
            var response = await Client.GetAsync($"{_baseUrl}/check-party-name-unique?partyName={createRequest.PartyName}&companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<bool>();
            result.Should().BeFalse();

            // Cleanup
            await Client.DeleteAsync($"{_baseUrl}/{createdCustomer!.Id}?companyId={createRequest.CompanyId}");
        }

        [Fact]
        public async Task CheckAbbreviationUnique_WithUniqueAbbreviation_ShouldReturnTrue()
        {
            // Arrange
            var uniqueAbbr = $"UA{Guid.NewGuid().ToString().Substring(0, 4)}";

            // Act
            var response = await Client.GetAsync($"{_baseUrl}/check-abbreviation-unique?abbreviation={uniqueAbbr}&companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<bool>();
            result.Should().BeTrue();
        }

        [Fact]
        public async Task CheckAbbreviationUnique_WithDuplicateAbbreviation_ShouldReturnFalse()
        {
            // Arrange - Create a customer first
            var uniqueAbbr = $"DA{Guid.NewGuid().ToString().Substring(0, 4)}";
            var createRequest = new CreateCustomerMasterRequest
            {
                CompanyId = 1,
                PartyName = $"AbbrCheck Test {Guid.NewGuid()}",
                Abbreviation = uniqueAbbr,
                AreaCode = 1,
                CustomerType = 1
            };

            var createResponse = await Client.PostAsJsonAsync(_baseUrl, createRequest);
            var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerMasterDto>();

            // Act
            var response = await Client.GetAsync($"{_baseUrl}/check-abbreviation-unique?abbreviation={uniqueAbbr}&companyId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var result = await response.Content.ReadFromJsonAsync<bool>();
            result.Should().BeFalse();

            // Cleanup
            await Client.DeleteAsync($"{_baseUrl}/{createdCustomer!.Id}?companyId={createRequest.CompanyId}");
        }
    }

    // Helper DTO for deserialization
    public class PagedResponseDto<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}

