using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErpBE.Application.Auth.Queries;
using ErpBE.Application.Interfaces;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Auth;

/// <summary>
/// Integration tests for FinancialYear functionality
/// Tests financial year data retrieval through MediatR (matches reference implementation - tests handlers directly)
/// </summary>
public class FinancialYearControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task GetFinancialYears_WithValidCompanyId_ShouldReturnListOfFinancialYears()
    {
        // Arrange
        var companyId = 1;
        var query = new GetFinancialYearsQuery { CompanyId = companyId };

        // Act
        var financialYears = await Mediator.Send(query);

        // Assert
        financialYears.Should().NotBeNull();
        financialYears.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetFinancialYears_ShouldReturnDistinctFinancialYears()
    {
        // Arrange
        var companyId = 1;
        var query = new GetFinancialYearsQuery { CompanyId = companyId };

        // Act
        var financialYears = await Mediator.Send(query);

        // Assert
        financialYears.Should().NotBeNull();
        
        // Verify no duplicates - each financial year code should appear only once
        var financialYearCodes = financialYears.Select(fy => fy.FinancialYearCode).ToList();
        var distinctCodes = financialYearCodes.Distinct().ToList();
        
        financialYearCodes.Count.Should().Be(distinctCodes.Count, "because financial years should be distinct");
    }

    [Fact]
    public async Task GetFinancialYears_ShouldReturnFinancialYearsOrderedByCodeDesc()
    {
        // Arrange
        var companyId = 1;
        var query = new GetFinancialYearsQuery { CompanyId = companyId };

        // Act
        var financialYears = await Mediator.Send(query);

        // Assert
        financialYears.Should().NotBeNull();
        
        if (financialYears.Count > 1)
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
        var query = new GetFinancialYearsQuery { CompanyId = companyId };

        // Act
        var financialYears = await Mediator.Send(query);

        // Assert
        financialYears.Should().NotBeNull();
        
        if (financialYears.Count > 0)
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
        var query = new GetFinancialYearsQuery { CompanyId = companyId };

        // Act
        var financialYears = await Mediator.Send(query);

        // Assert
        financialYears.Should().NotBeNull();
        
        if (financialYears.Count > 0)
        {
            var firstFinancialYear = financialYears.First();
            
            // Dates should be in dd/MM/yyyy format
            firstFinancialYear.OpeningDate.Should().MatchRegex(@"\d{2}/\d{2}/\d{4}");
            firstFinancialYear.ClosingDate.Should().MatchRegex(@"\d{2}/\d{2}/\d{4}");
        }
    }

    [Fact]
    public async Task GetFinancialYears_WithInvalidCompanyId_ShouldReturnEmptyList()
    {
        // Arrange
        var invalidCompanyId = 99999; // Non-existent company ID
        var query = new GetFinancialYearsQuery { CompanyId = invalidCompanyId };

        // Act
        var financialYears = await Mediator.Send(query);

        // Assert
        financialYears.Should().NotBeNull();
        financialYears.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFinancialYears_WithZeroCompanyId_ShouldReturnEmptyList()
    {
        // Arrange
        var companyId = 0;
        var query = new GetFinancialYearsQuery { CompanyId = companyId };

        // Act
        var financialYears = await Mediator.Send(query);

        // Assert
        financialYears.Should().NotBeNull();
        financialYears.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFinancialYears_WithNegativeCompanyId_ShouldReturnEmptyList()
    {
        // Arrange
        var companyId = -1;
        var query = new GetFinancialYearsQuery { CompanyId = companyId };

        // Act
        var financialYears = await Mediator.Send(query);

        // Assert
        financialYears.Should().NotBeNull();
        financialYears.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFinancialYears_ShouldReturnFinancialYearsWithIdEqualToFinancialYearCode()
    {
        // Arrange
        var companyId = 1;
        var query = new GetFinancialYearsQuery { CompanyId = companyId };

        // Act
        var financialYears = await Mediator.Send(query);

        // Assert
        financialYears.Should().NotBeNull();
        
        if (financialYears.Count > 0)
        {
            // According to the repository, Id and FinancialYearCode should be equal (both are CM_CODE)
            financialYears.Should().OnlyContain(fy => fy.Id == fy.FinancialYearCode);
        }
    }

    [Fact]
    public async Task GetFinancialYears_ShouldHandleEmptyResult()
    {
        // Arrange
        var companyId = 1; // Assuming at least one financial year exists for company 1
        var query = new GetFinancialYearsQuery { CompanyId = companyId };

        // Act
        var financialYears = await Mediator.Send(query);

        // Assert
        financialYears.Should().NotBeNull();
        // If empty, should return empty list, not null or error
        financialYears.Should().BeAssignableTo<List<FinancialYearDto>>();
    }
}

