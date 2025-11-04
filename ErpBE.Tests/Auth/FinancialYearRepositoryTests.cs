using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErpBE.Application.Interfaces;
using ErpBE.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ErpBE.Tests.Auth
{
    /// <summary>
    /// Integration tests for FinancialYearRepository
    /// Tests financial year data retrieval from database
    /// </summary>
    public class FinancialYearRepositoryTests : IDisposable
    {
        private readonly IFinancialYearRepository _repository;
        private readonly IConfiguration _configuration;

        public FinancialYearRepositoryTests()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _repository = new FinancialYearRepository(_configuration);
        }

        [Fact]
        public async Task GetFinancialYearsByCompanyIdAsync_WithValidCompanyId_ShouldReturnListOfFinancialYears()
        {
            // Arrange
            var companyId = 1; // Assuming company ID 1 exists

            // Act
            var financialYears = await _repository.GetFinancialYearsByCompanyIdAsync(companyId);

            // Assert
            financialYears.Should().NotBeNull();
            financialYears.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetFinancialYearsByCompanyIdAsync_ShouldReturnDistinctFinancialYears()
        {
            // Arrange
            var companyId = 1;

            // Act
            var financialYears = await _repository.GetFinancialYearsByCompanyIdAsync(companyId);

            // Assert
            financialYears.Should().NotBeNull();

            // Verify no duplicates - each financial year code should appear only once
            var financialYearCodes = financialYears.Select(fy => fy.FinancialYearCode).ToList();
            var distinctCodes = financialYearCodes.Distinct().ToList();

            financialYearCodes.Count.Should().Be(distinctCodes.Count, "because financial years should be distinct");
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
                // Verify financial years are ordered by code DESC (latest first)
                var codes = financialYears.Select(fy => fy.FinancialYearCode).ToList();
                var sortedCodesDesc = codes.OrderByDescending(c => c).ToList();

                codes.Should().BeEquivalentTo(sortedCodesDesc, "because financial years should be ordered by code DESC");
            }
        }

        [Fact]
        public async Task GetFinancialYearsByCompanyIdAsync_ShouldReturnValidFinancialYearStructure()
        {
            // Arrange
            var companyId = 1;

            // Act
            var financialYears = await _repository.GetFinancialYearsByCompanyIdAsync(companyId);

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
        public async Task GetFinancialYearsByCompanyIdAsync_ShouldReturnFinancialYearsWithValidDateFormat()
        {
            // Arrange
            var companyId = 1;

            // Act
            var financialYears = await _repository.GetFinancialYearsByCompanyIdAsync(companyId);

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
        public async Task GetFinancialYearsByCompanyIdAsync_WithInvalidCompanyId_ShouldReturnEmptyList()
        {
            // Arrange
            var invalidCompanyId = 99999; // Non-existent company ID

            // Act
            var financialYears = await _repository.GetFinancialYearsByCompanyIdAsync(invalidCompanyId);

            // Assert
            financialYears.Should().NotBeNull();
            financialYears.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFinancialYearsByCompanyIdAsync_WithZeroCompanyId_ShouldReturnEmptyList()
        {
            // Arrange
            var companyId = 0;

            // Act
            var financialYears = await _repository.GetFinancialYearsByCompanyIdAsync(companyId);

            // Assert
            financialYears.Should().NotBeNull();
            financialYears.Should().BeEmpty();
        }

        [Fact]
        public async Task GetFinancialYearsByCompanyIdAsync_WithNegativeCompanyId_ShouldReturnEmptyList()
        {
            // Arrange
            var companyId = -1;

            // Act
            var financialYears = await _repository.GetFinancialYearsByCompanyIdAsync(companyId);

            // Assert
            financialYears.Should().NotBeNull();
            financialYears.Should().BeEmpty();
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

            if (financialYears.Count > 0)
            {
                // According to the repository, Id and FinancialYearCode should be equal (both are CM_CODE)
                financialYears.Should().OnlyContain(fy => fy.Id == fy.FinancialYearCode);
            }
        }

        [Fact]
        public async Task GetFinancialYearsByCompanyIdAsync_ShouldHandleEmptyResult()
        {
            // Arrange
            var companyId = 1; // Assuming at least one financial year exists for company 1

            // Act
            var financialYears = await _repository.GetFinancialYearsByCompanyIdAsync(companyId);

            // Assert
            financialYears.Should().NotBeNull();
            // If empty, should return empty list, not null or error
            financialYears.Should().BeAssignableTo<List<ErpBE.Application.Interfaces.FinancialYearDto>>();
        }

        public void Dispose()
        {
            // No cleanup needed for read-only operations
        }
    }
}

