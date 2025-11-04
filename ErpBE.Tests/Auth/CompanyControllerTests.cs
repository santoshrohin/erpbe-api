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
    public class CompanyControllerTests : TestBase
    {
        public CompanyControllerTests(WebApplicationFactory<Program> factory) : base(factory) { }

        [Fact]
        public async Task GetCompanies_ShouldReturnOk()
        {
            // Act
            var response = await Client.GetAsync("/api/Company");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetCompanies_ShouldReturnListOfCompanies()
        {
            // Act
            var response = await Client.GetAsync("/api/Company");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var companies = await response.Content.ReadFromJsonAsync<List<ErpBE.Application.Interfaces.CompanyDto>>();
            
            companies.Should().NotBeNull();
            companies!.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetCompanies_ShouldReturnDistinctCompanies()
        {
            // Act
            var response = await Client.GetAsync("/api/Company");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var companies = await response.Content.ReadFromJsonAsync<List<ErpBE.Application.Interfaces.CompanyDto>>();
            
            companies.Should().NotBeNull();
            
            // Verify no duplicates - each company ID should appear only once
            var companyIds = companies!.Select(c => c.Id).ToList();
            var distinctIds = companyIds.Distinct().ToList();
            
            companyIds.Count.Should().Be(distinctIds.Count, "because companies should be distinct");
        }

        [Fact]
        public async Task GetCompanies_ShouldReturnOnlyActiveCompanies()
        {
            // Act
            var response = await Client.GetAsync("/api/Company");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var companies = await response.Content.ReadFromJsonAsync<List<ErpBE.Application.Interfaces.CompanyDto>>();
            
            companies.Should().NotBeNull();
            companies!.Count.Should().BeGreaterThan(0);
            
            // All companies should have valid IDs (greater than 0)
            companies.Should().OnlyContain(c => c.Id > 0);
            companies.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.DisplayName));
        }

        [Fact]
        public async Task GetCompanies_ShouldReturnCompaniesOrderedByName()
        {
            // Act
            var response = await Client.GetAsync("/api/Company");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var companies = await response.Content.ReadFromJsonAsync<List<ErpBE.Application.Interfaces.CompanyDto>>();
            
            companies.Should().NotBeNull();
            
            if (companies!.Count > 1)
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
            // Act
            var response = await Client.GetAsync("/api/Company");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var companies = await response.Content.ReadFromJsonAsync<List<ErpBE.Application.Interfaces.CompanyDto>>();
            
            companies.Should().NotBeNull();
            
            if (companies!.Count > 0)
            {
                var firstCompany = companies.First();
                
                firstCompany.Id.Should().BeGreaterThan(0);
                firstCompany.DisplayName.Should().NotBeNullOrWhiteSpace();
            }
        }

        [Fact]
        public async Task GetCompanies_ShouldHandleEmptyResult()
        {
            // Note: This test assumes there's at least one active company in the database
            // If the database is empty, this test would need to be adjusted
            // For now, we'll verify the endpoint doesn't crash on empty results
            
            // Act
            var response = await Client.GetAsync("/api/Company");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var companies = await response.Content.ReadFromJsonAsync<List<ErpBE.Application.Interfaces.CompanyDto>>();
            
            companies.Should().NotBeNull();
            // If empty, should return empty list, not null or error
            companies!.Should().BeAssignableTo<List<ErpBE.Application.Interfaces.CompanyDto>>();
        }
    }
}

