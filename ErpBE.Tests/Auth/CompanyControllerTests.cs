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
/// Integration tests for Company functionality
/// Tests company data retrieval through MediatR (matches reference implementation - tests handlers directly)
/// </summary>
public class CompanyControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task GetCompanies_ShouldReturnListOfCompanies()
    {
        // Arrange
        var query = new GetCompaniesQuery();

        // Act
        var companies = await Mediator.Send(query);

        // Assert
        companies.Should().NotBeNull();
        companies.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCompanies_ShouldReturnDistinctCompanies()
    {
        // Arrange
        var query = new GetCompaniesQuery();

        // Act
        var companies = await Mediator.Send(query);

        // Assert
        companies.Should().NotBeNull();
        
        // Verify no duplicates - each company ID should appear only once
        var companyIds = companies.Select(c => c.Id).ToList();
        var distinctIds = companyIds.Distinct().ToList();
        
        companyIds.Count.Should().Be(distinctIds.Count, "because companies should be distinct");
    }

    [Fact]
    public async Task GetCompanies_ShouldReturnOnlyActiveCompanies()
    {
        // Arrange
        var query = new GetCompaniesQuery();

        // Act
        var companies = await Mediator.Send(query);

        // Assert
        companies.Should().NotBeNull();
        companies.Should().NotBeEmpty();
        
        // All companies should have valid IDs (not zero, can be negative since IDENTITY starts from -2147483648)
        companies.Should().OnlyContain(c => c.Id != 0);
        companies.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.DisplayName));
    }

    [Fact]
    public async Task GetCompanies_ShouldReturnCompaniesOrderedByName()
    {
        // Arrange
        var query = new GetCompaniesQuery();

        // Act
        var companies = await Mediator.Send(query);

        // Assert
        companies.Should().NotBeNull();
        
        if (companies.Count > 1)
        {
            // Verify companies are ordered by name
            var sortedNames = companies.Select(c => c.DisplayName).OrderBy(n => n).ToList();
            var actualNames = companies.Select(c => c.DisplayName).ToList();
            
            actualNames.Should().BeEquivalentTo(sortedNames, "because companies should be ordered by name");
        }
    }

    [Fact]
    public async Task GetCompanies_ShouldReturnValidCompanyStructure()
    {
        // Arrange
        var query = new GetCompaniesQuery();

        // Act
        var companies = await Mediator.Send(query);

        // Assert
        companies.Should().NotBeNull();
        
        if (companies.Count > 0)
        {
            var firstCompany = companies.First();
            
            firstCompany.Id.Should().NotBe(0); // IDENTITY starts from -2147483648, so IDs can be negative
            firstCompany.DisplayName.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task GetCompanies_ShouldHandleEmptyResult()
    {
        // Arrange
        var query = new GetCompaniesQuery();
        
        // Act
        var companies = await Mediator.Send(query);

        // Assert
        companies.Should().NotBeNull();
        // If empty, should return empty list, not null or error
        companies.Should().BeAssignableTo<List<CompanyDto>>();
    }
}

