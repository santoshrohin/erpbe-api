using ErpBE.Application.CustomerPo.Commands;
using ErpBE.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ErpBE.Tests.Sales;

public class CustomerPoControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    private const int TestCompanyId = 1;
    private string? _authToken;

    public CustomerPoControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        if (!string.IsNullOrEmpty(_authToken))
            return _authToken;

        var loginRequest = new
        {
            username = "Mohan",
            password = "1234",
            companyId = TestCompanyId,
            financialYearCode = -2147483641
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/Login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<dynamic>();
        _authToken = loginResult?.GetProperty("token").GetString();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

        return _authToken!;
    }

    [Fact]
    public async Task CreateCustomerPo_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await GetAuthTokenAsync();

        var command = new CreateCustomerPoCommand
        {
            CustomerCode = -2147482898, // Test Customer from database
            PoNumber = $"TEST-PO-{Guid.NewGuid().ToString().Substring(0, 8)}",
            PoType = 1,
            PoDate = DateTime.Now,
            CreditDays = 30,
            CompanyId = TestCompanyId,
            WorkOrderNumber = $"WO-{DateTime.Now:yyyyMMdd}",
            PaymentTerms = "Net 30",
            IsAuthorized = true,
            CustomerPoDate = DateTime.Now,
            TaxPercentage = 18.0,
            BasicAmount = 10000.0,
            GrandTotal = 11800.0,
            ProjectCode = -2147483648, // NA Project
            ProjectName = "NA",
            Details = new List<CreateCustomerPoDetailCommand>
            {
                new CreateCustomerPoDetailCommand
                {
                    ItemCode = -2147478461, // SLEEVE from database
                    UomCode = -2147483647, // UPDATED096 from database
                    OrderedQuantity = 100,
                    Rate = 100,
                    Amount = 10000,
                    CustomerItemCode = "CUST-ITEM-001",
                    CustomerItemName = "Customer Item Name",
                    Status = 0,
                    DispatchedQuantity = 0,
                    IsOrder = true
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/CustomerPo", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CustomerPoMasterDto>();
        result.Should().NotBeNull();
        result!.PoCode.Should().NotBe(0); // Can be negative in this database
        result.PoNumber.Should().Be(command.PoNumber);
        result.Details.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllCustomerPos_WithValidParameters_ShouldReturnPagedData()
    {
        // Arrange
        await GetAuthTokenAsync();

        // Act
        var response = await _client.GetAsync($"/api/CustomerPo?CompanyId={TestCompanyId}&PageNumber=1&PageSize=10&IsActive=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("data");
        content.Should().Contain("totalCount");
    }

    [Fact]
    public async Task GetCustomerPoById_WithValidId_ShouldReturnPo()
    {
        // Arrange
        await GetAuthTokenAsync();

        // First create a PO
        var createCommand = new CreateCustomerPoCommand
        {
            CustomerCode = -2147482898,
            PoNumber = $"TEST-GET-{Guid.NewGuid().ToString().Substring(0, 8)}",
            PoType = 1,
            PoDate = DateTime.Now,
            CreditDays = 30,
            CompanyId = TestCompanyId,
            ProjectCode = -2147483648,
            ProjectName = "NA",
            GrandTotal = 1000.0,
            Details = new List<CreateCustomerPoDetailCommand>
            {
                new CreateCustomerPoDetailCommand
                {
                    ItemCode = -2147478461,
                    UomCode = -2147483647,
                    OrderedQuantity = 10,
                    Rate = 100,
                    Amount = 1000,
                    IsOrder = true
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/CustomerPo", createCommand);
        var createdPo = await createResponse.Content.ReadFromJsonAsync<CustomerPoMasterDto>();

        // Act
        var response = await _client.GetAsync($"/api/CustomerPo/{createdPo!.PoCode}?companyId={TestCompanyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CustomerPoMasterDto>();
        result.Should().NotBeNull();
        result!.PoCode.Should().Be(createdPo.PoCode);
        result.Details.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateCustomerPo_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await GetAuthTokenAsync();

        // First create a PO
        var createCommand = new CreateCustomerPoCommand
        {
            CustomerCode = -2147482898,
            PoNumber = $"TEST-UPD-{Guid.NewGuid().ToString().Substring(0, 8)}",
            PoType = 1,
            PoDate = DateTime.Now,
            CreditDays = 30,
            CompanyId = TestCompanyId,
            ProjectCode = -2147483648,
            ProjectName = "NA",
            GrandTotal = 1000.0,
            Details = new List<CreateCustomerPoDetailCommand>
            {
                new CreateCustomerPoDetailCommand
                {
                    ItemCode = -2147478461,
                    UomCode = -2147483647,
                    OrderedQuantity = 10,
                    Rate = 100,
                    Amount = 1000,
                    IsOrder = true
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/CustomerPo", createCommand);
        var createdPo = await createResponse.Content.ReadFromJsonAsync<CustomerPoMasterDto>();

        // Prepare update command
        var updateCommand = new UpdateCustomerPoCommand
        {
            PoCode = createdPo!.PoCode,
            CustomerCode = createdPo.CustomerCode,
            PoNumber = createdPo.PoNumber,
            PoType = createdPo.PoType,
            PoDate = createdPo.PoDate,
            CreditDays = 45, // Updated
            CompanyId = createdPo.CompanyId,
            ProjectCode = createdPo.ProjectCode,
            ProjectName = createdPo.ProjectName,
            GrandTotal = 2000.0, // Updated
            Details = new List<CreateCustomerPoDetailCommand>
            {
                new CreateCustomerPoDetailCommand
                {
                    ItemCode = -2147478461,
                    UomCode = -2147483647,
                    OrderedQuantity = 20, // Updated
                    Rate = 100,
                    Amount = 2000, // Updated
                    IsOrder = true
                }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/CustomerPo/{createdPo.PoCode}", updateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CustomerPoMasterDto>();
        result.Should().NotBeNull();
        result!.CreditDays.Should().Be(45);
        result.GrandTotal.Should().Be(2000.0);
        result.AmendmentCount.Should().Be(1); // Should increment
    }

    [Fact]
    public async Task DeleteCustomerPo_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await GetAuthTokenAsync();

        // First create a PO
        var createCommand = new CreateCustomerPoCommand
        {
            CustomerCode = -2147482898,
            PoNumber = $"TEST-DEL-{Guid.NewGuid().ToString().Substring(0, 8)}",
            PoType = 1,
            PoDate = DateTime.Now,
            CreditDays = 30,
            CompanyId = TestCompanyId,
            ProjectCode = -2147483648,
            ProjectName = "NA",
            GrandTotal = 1000.0,
            Details = new List<CreateCustomerPoDetailCommand>
            {
                new CreateCustomerPoDetailCommand
                {
                    ItemCode = -2147478461,
                    UomCode = -2147483647,
                    OrderedQuantity = 10,
                    Rate = 100,
                    Amount = 1000,
                    IsOrder = true
                }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/CustomerPo", createCommand);
        var createdPo = await createResponse.Content.ReadFromJsonAsync<CustomerPoMasterDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/CustomerPo/{createdPo!.PoCode}?companyId={TestCompanyId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify it's soft deleted
        var getResponse = await _client.GetAsync($"/api/CustomerPo/{createdPo.PoCode}?companyId={TestCompanyId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCustomerPo_WithoutLineItems_ShouldReturnBadRequest()
    {
        // Arrange
        await GetAuthTokenAsync();

        var command = new CreateCustomerPoCommand
        {
            CustomerCode = -2147482898,
            PoNumber = $"TEST-NO-ITEMS-{Guid.NewGuid().ToString().Substring(0, 8)}",
            PoType = 1,
            PoDate = DateTime.Now,
            CreditDays = 30,
            CompanyId = TestCompanyId,
            ProjectCode = -2147483648,
            ProjectName = "NA",
            Details = new List<CreateCustomerPoDetailCommand>() // Empty list
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/CustomerPo", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCustomerPo_WithInvalidQuantity_ShouldReturnBadRequest()
    {
        // Arrange
        await GetAuthTokenAsync();

        var command = new CreateCustomerPoCommand
        {
            CustomerCode = -2147482898,
            PoNumber = $"TEST-BAD-QTY-{Guid.NewGuid().ToString().Substring(0, 8)}",
            PoType = 1,
            PoDate = DateTime.Now,
            CreditDays = 30,
            CompanyId = TestCompanyId,
            ProjectCode = -2147483648,
            ProjectName = "NA",
            Details = new List<CreateCustomerPoDetailCommand>
            {
                new CreateCustomerPoDetailCommand
                {
                    ItemCode = -2147478461,
                    UomCode = -2147483647,
                    OrderedQuantity = -10, // Invalid negative quantity
                    Rate = 100,
                    Amount = 1000,
                    IsOrder = true
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/CustomerPo", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

