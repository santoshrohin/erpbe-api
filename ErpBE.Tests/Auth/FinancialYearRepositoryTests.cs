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
/// Integration tests for FinancialYearRepository
/// Tests financial year data retrieval from database
/// </summary>
public class FinancialYearRepositoryTests : IntegrationTestBase
{
    private IFinancialYearRepository _repository = default!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        var configuration = GetService<IConfiguration>();
        _repository = new FinancialYearRepository(configuration);
    }

    [Fact]
    public async Task GetFinancialYearsByCompanyIdAsync_WithValidCompanyId_ShouldReturnFinancialYears()
    {
        // Arrange
        var companyId = 1; // Assuming company ID 1 exists and has financial years

        // Act
        var financialYears = await _repository.GetFinancialYearsByCompanyIdAsync(companyId);

        // Assert
        financialYears.Should().NotBeNull();
        financialYears.Should().NotBeEmpty();
        financialYears.Should().OnlyContain(fy => fy.Id != 0); // IDENTITY starts from -2147483648, so IDs can be negative
        financialYears.Should().OnlyContain(fy => !string.IsNullOrWhiteSpace(fy.DisplayName));
        financialYears.Should().OnlyContain(fy => fy.FinancialYearCode != 0); // FinancialYearCode can be negative
        financialYears.Should().OnlyContain(fy => !string.IsNullOrWhiteSpace(fy.OpeningDate));
        financialYears.Should().OnlyContain(fy => !string.IsNullOrWhiteSpace(fy.ClosingDate));
    }

    [Fact]
    public async Task GetFinancialYearsByCompanyIdAsync_WithInvalidCompanyId_ShouldReturnEmptyList()
    {
        // Arrange
        var companyId = 99999; // Non-existent company ID

        // Act
        var financialYears = await _repository.GetFinancialYearsByCompanyIdAsync(companyId);

        // Assert
        financialYears.Should().NotBeNull();
        financialYears.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFinancialYearsByCompanyIdAsync_ShouldReturnFinancialYearsOrderedByCodeDesc()
    {
        // Arrange
        var companyId = 1;

        // Act
        var financialYears = await _repository.GetFinancialYearsByCompanyIdAsync(companyId);

        // Assert
        financialYears.Should().NotBeNull();
        if (financialYears.Count > 1)
        {
            financialYears.Select(fy => fy.FinancialYearCode).Should().BeInDescendingOrder();
        }
    }

    [Fact]
    public async Task GetFinancialYearsByCompanyIdAsync_ShouldReturnFinancialYearsWithValidDateFormat()
    {
        // Arrange
        var companyId = 1;

        // Act
        var financialYears = await _repository.GetFinancialYearsByCompanyIdAsync(companyId);

        // Assert
        financialYears.Should().NotBeNull();
        if (financialYears.Any())
        {
            financialYears.Should().OnlyContain(fy => System.Text.RegularExpressions.Regex.IsMatch(fy.OpeningDate, @"^\d{2}/\d{2}/\d{4}$"));
            financialYears.Should().OnlyContain(fy => System.Text.RegularExpressions.Regex.IsMatch(fy.ClosingDate, @"^\d{2}/\d{2}/\d{4}$"));
        }
    }

    [Fact]
    public async Task GetFinancialYearsByCompanyIdAsync_ShouldReturnFinancialYearsWithIdEqualToFinancialYearCode()
    {
        // Arrange
        var companyId = 1;

        // Act
        var financialYears = await _repository.GetFinancialYearsByCompanyIdAsync(companyId);

        // Assert
        financialYears.Should().NotBeNull();
        financialYears.Should().OnlyContain(fy => fy.Id == fy.FinancialYearCode);
    }

    [Fact]
    public async Task GetFinancialYearsByCompanyIdAsync_ShouldReturnCorrectDtoType()
    {
        // Arrange
        var companyId = 1;

        // Act
        var financialYears = await _repository.GetFinancialYearsByCompanyIdAsync(companyId);

        // Assert
        financialYears.Should().BeAssignableTo<List<FinancialYearDto>>();
    }
}

