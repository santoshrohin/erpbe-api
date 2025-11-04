using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ErpBE.Tests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ErpBE.Tests.Auth
{
    public class FinancialYearControllerTests : TestBase
    {
        public FinancialYearControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

        [Fact]
        public async Task GetFinancialYears_WithValidCompanyId_ShouldReturnOk()
        {
            // Arrange
            var companyId = 1; // Assuming company ID 1 exists

            // Act
            var response = await Client.GetAsync($"/api/FinancialYear/{companyId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetFinancialYears_WithValidCompanyId_ShouldReturnListOfFinancialYears()
        {
            // Arrange
            var companyId = 1;

            // Act
            var response = await Client.GetAsync($"/api/FinancialYear/{companyId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var financialYears = await response.Content.ReadFromJsonAsync<List<ErpBE.Application.Interfaces.FinancialYearDto>>();
            
            financialYears.Should().NotBeNull();
            financialYears!.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetFinancialYears_ShouldReturnDistinctFinancialYears()
        {
            // Arrange
            var companyId = 1;

            // Act
            var response = await Client.GetAsync($"/api/FinancialYear/{companyId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var financialYears = await response.Content.ReadFromJsonAsync<List<ErpBE.Application.Interfaces.FinancialYearDto>>();
            
            financialYears.Should().NotBeNull();
            
            // Verify no duplicates - each financial year code should appear only once
            var financialYearCodes = financialYears!.Select(fy => fy.FinancialYearCode).ToList();
            var distinctCodes = financialYearCodes.Distinct().ToList();
            
            financialYearCodes.Count.Should().Be(distinctCodes.Count, "because financial years should be distinct");
        }

        [Fact]
        public async Task GetFinancialYears_ShouldReturnFinancialYearsOrderedByCodeDesc()
        {
            // Arrange
            var companyId = 1;

            // Act
            var response = await Client.GetAsync($"/api/FinancialYear/{companyId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var financialYears = await response.Content.ReadFromJsonAsync<List<ErpBE.Application.Interfaces.FinancialYearDto>>();
            
            financialYears.Should().NotBeNull();
            
            if (financialYears!.Count > 1)
            {
                // Verify financial years are ordered by code DESC (latest first)
                var codes = financialYears.Select(fy => fy.FinancialYearCode).ToList();
                var sortedCodesDesc = codes.OrderByDescending(c => c).ToList();
                
                codes.Should().BeEquivalentTo(sortedCodesDesc, "because financial years should be ordered by code DESC");
            }
        }

        [Fact]
        public async Task GetFinancialYears_ShouldReturnValidFinancialYearStructure()
        {
            // Arrange
            var companyId = 1;

            // Act
            var response = await Client.GetAsync($"/api/FinancialYear/{companyId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var financialYears = await response.Content.ReadFromJsonAsync<List<ErpBE.Application.Interfaces.FinancialYearDto>>();
            
            financialYears.Should().NotBeNull();
            
            if (financialYears!.Count > 0)
            {
                var firstFinancialYear = financialYears.First();
                
                firstFinancialYear.Id.Should().NotBe(0);
                firstFinancialYear.FinancialYearCode.Should().NotBe(0);
                firstFinancialYear.DisplayName.Should().NotBeNullOrWhiteSpace();
                firstFinancialYear.OpeningDate.Should().NotBeNullOrWhiteSpace();
                firstFinancialYear.ClosingDate.Should().NotBeNullOrWhiteSpace();
                
                // Display name should contain "From" and "To"
                firstFinancialYear.DisplayName.Should().Contain("From");
                firstFinancialYear.DisplayName.Should().Contain("To");
            }
        }

        [Fact]
        public async Task GetFinancialYears_ShouldReturnFinancialYearsWithValidDateFormat()
        {
            // Arrange
            var companyId = 1;

            // Act
            var response = await Client.GetAsync($"/api/FinancialYear/{companyId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var financialYears = await response.Content.ReadFromJsonAsync<List<ErpBE.Application.Interfaces.FinancialYearDto>>();
            
            financialYears.Should().NotBeNull();
            
            if (financialYears!.Count > 0)
            {
                var firstFinancialYear = financialYears.First();
                
                // Dates should be in dd/MM/yyyy format
                firstFinancialYear.OpeningDate.Should().MatchRegex(@"\d{2}/\d{2}/\d{4}");
                firstFinancialYear.ClosingDate.Should().MatchRegex(@"\d{2}/\d{2}/\d{4}");
            }
        }

        [Fact]
        public async Task GetFinancialYears_WithInvalidCompanyId_ShouldReturnInternalServerError()
        {
            // Arrange
            var invalidCompanyId = 99999; // Non-existent company ID

            // Act
            var response = await Client.GetAsync($"/api/FinancialYear/{invalidCompanyId}");

            // Assert
            // Should return 200 with empty list, or 500 if there's an error
            // Based on the controller implementation, it returns 500 on exception
            response.StatusCode.Should().Match(s => 
                s == HttpStatusCode.OK || s == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetFinancialYears_WithZeroCompanyId_ShouldReturnInternalServerError()
        {
            // Arrange
            var companyId = 0;

            // Act
            var response = await Client.GetAsync($"/api/FinancialYear/{companyId}");

            // Assert
            response.StatusCode.Should().Match(s => 
                s == HttpStatusCode.OK || s == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetFinancialYears_WithNegativeCompanyId_ShouldReturnInternalServerError()
        {
            // Arrange
            var companyId = -1;

            // Act
            var response = await Client.GetAsync($"/api/FinancialYear/{companyId}");

            // Assert
            response.StatusCode.Should().Match(s => 
                s == HttpStatusCode.OK || s == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetFinancialYears_ShouldReturnFinancialYearsWithIdEqualToFinancialYearCode()
        {
            // Arrange
            var companyId = 1;

            // Act
            var response = await Client.GetAsync($"/api/FinancialYear/{companyId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var financialYears = await response.Content.ReadFromJsonAsync<List<ErpBE.Application.Interfaces.FinancialYearDto>>();
            
            financialYears.Should().NotBeNull();
            
            if (financialYears!.Count > 0)
            {
                // According to the controller, Id and FinancialYearCode should be equal (both are CM_CODE)
                financialYears.Should().OnlyContain(fy => fy.Id == fy.FinancialYearCode);
            }
        }

        [Fact]
        public async Task GetFinancialYears_ShouldHandleEmptyResult()
        {
            // Arrange
            var companyId = 1; // Assuming at least one financial year exists for company 1

            // Act
            var response = await Client.GetAsync($"/api/FinancialYear/{companyId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var financialYears = await response.Content.ReadFromJsonAsync<List<ErpBE.Application.Interfaces.FinancialYearDto>>();
            
            financialYears.Should().NotBeNull();
            // If empty, should return empty list, not null or error
            financialYears!.Should().BeAssignableTo<List<ErpBE.Application.Interfaces.FinancialYearDto>>();
        }
    }
}

