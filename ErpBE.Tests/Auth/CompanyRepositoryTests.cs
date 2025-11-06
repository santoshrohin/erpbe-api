using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErpBE.Application.Interfaces;
using ErpBE.Infrastructure.Repositories;
using ErpBE.Tests.Integration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ErpBE.Tests.Auth;

/// <summary>
/// Integration tests for CompanyRepository
/// Tests company data retrieval from database
/// </summary>
public class CompanyRepositoryTests : IntegrationTestBase
{
    private ICompanyRepository _repository = default!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        var configuration = GetService<IConfiguration>();
        _repository = new CompanyRepository(configuration);
    }

    [Fact]
    public async Task GetActiveCompaniesAsync_ShouldReturnCompanies()
    {
        // Act
        var companies = await _repository.GetActiveCompaniesAsync();

        // Assert
        companies.Should().NotBeNull();
        companies.Should().NotBeEmpty();
        companies.Should().OnlyContain(c => c.Id != 0); // IDENTITY starts from -2147483648, so IDs can be negative
        companies.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.DisplayName));
    }

    [Fact]
    public async Task GetActiveCompaniesAsync_ShouldReturnDistinctCompanies()
    {
        // Act
        var companies = await _repository.GetActiveCompaniesAsync();

        // Assert
        companies.Should().NotBeNull();
        companies.Select(c => c.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GetActiveCompaniesAsync_ShouldReturnCompaniesOrderedByName()
    {
        // Act
        var companies = await _repository.GetActiveCompaniesAsync();

        // Assert
        companies.Should().NotBeNull();
        if (companies.Count > 1)
        {
            companies.Select(c => c.DisplayName).Should().BeInAscendingOrder();
        }
    }

    [Fact]
    public async Task GetActiveCompaniesAsync_ShouldReturnValidCompanyStructure()
    {
        // Act
        var companies = await _repository.GetActiveCompaniesAsync();

        // Assert
        companies.Should().NotBeNull();
        if (companies.Any())
        {
            var firstCompany = companies.First();
            firstCompany.Id.Should().NotBe(0); // IDENTITY starts from -2147483648, so IDs can be negative
            firstCompany.DisplayName.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task GetActiveCompaniesAsync_ShouldHandleNoActiveCompanies()
    {
        // This test requires a way to temporarily disable all companies or mock the DB.
        // For now, we assume there are always active companies in the test DB.
        // If no companies are found, it should return an empty list, not throw an error.
        var companies = await _repository.GetActiveCompaniesAsync();
        companies.Should().NotBeNull(); // Should return an empty list, not null
    }

    [Fact]
    public async Task GetActiveCompaniesAsync_ShouldReturnCorrectDtoType()
    {
        // Act
        var companies = await _repository.GetActiveCompaniesAsync();

        // Assert
        companies.Should().BeAssignableTo<List<CompanyDto>>();
    }
}

