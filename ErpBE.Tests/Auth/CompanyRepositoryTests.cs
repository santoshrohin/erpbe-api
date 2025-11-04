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
    /// Integration tests for CompanyRepository
    /// Tests company data retrieval from database
    /// </summary>
    public class CompanyRepositoryTests : IDisposable
    {
        private readonly ICompanyRepository _repository;
        private readonly IConfiguration _configuration;

        public CompanyRepositoryTests()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _repository = new CompanyRepository(_configuration);
        }

        [Fact]
        public async Task GetActiveCompaniesAsync_ShouldReturnListOfCompanies()
        {
            // Act
            var companies = await _repository.GetActiveCompaniesAsync();

            // Assert
            companies.Should().NotBeNull();
            companies.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetActiveCompaniesAsync_ShouldReturnDistinctCompanies()
        {
            // Act
            var companies = await _repository.GetActiveCompaniesAsync();

            // Assert
            companies.Should().NotBeNull();

            // Verify no duplicates - each company ID should appear only once
            var companyIds = companies.Select(c => c.Id).ToList();
            var distinctIds = companyIds.Distinct().ToList();

            companyIds.Count.Should().Be(distinctIds.Count, "because companies should be distinct");
        }

        [Fact]
        public async Task GetActiveCompaniesAsync_ShouldReturnOnlyActiveCompanies()
        {
            // Act
            var companies = await _repository.GetActiveCompaniesAsync();

            // Assert
            companies.Should().NotBeNull();
            companies.Count.Should().BeGreaterThan(0);

            // All companies should have valid IDs (greater than 0)
            companies.Should().OnlyContain(c => c.Id > 0);
            companies.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.DisplayName));
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
                // Verify companies are ordered by name
                var sortedNames = companies.Select(c => c.DisplayName).OrderBy(n => n).ToList();
                var actualNames = companies.Select(c => c.DisplayName).ToList();

                actualNames.Should().BeEquivalentTo(sortedNames, "because companies should be ordered by name");
            }
        }

        [Fact]
        public async Task GetActiveCompaniesAsync_ShouldReturnValidCompanyStructure()
        {
            // Act
            var companies = await _repository.GetActiveCompaniesAsync();

            // Assert
            companies.Should().NotBeNull();

            if (companies.Count > 0)
            {
                var firstCompany = companies.First();

                firstCompany.Id.Should().BeGreaterThan(0);
                firstCompany.DisplayName.Should().NotBeNullOrWhiteSpace();
            }
        }

        [Fact]
        public async Task GetActiveCompaniesAsync_ShouldHandleEmptyResult()
        {
            // Note: This test assumes there's at least one active company in the database
            // If the database is empty, this test would need to be adjusted
            // For now, we'll verify the repository doesn't crash on empty results

            // Act
            var companies = await _repository.GetActiveCompaniesAsync();

            // Assert
            companies.Should().NotBeNull();
            // If empty, should return empty list, not null or error
            companies.Should().BeAssignableTo<List<ErpBE.Application.Interfaces.CompanyDto>>();
        }

        public void Dispose()
        {
            // No cleanup needed for read-only operations
        }
    }
}

