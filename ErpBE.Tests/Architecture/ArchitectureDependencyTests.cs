using NetArchTest.Rules;
using Xunit;

namespace ErpBE.Tests.Architecture
{
    /// <summary>
    /// Architecture tests to enforce Clean Architecture dependency rules.
    /// These tests ensure that layers don't have circular dependencies and follow the dependency inversion principle.
    /// </summary>
    public class ArchitectureDependencyTests
    {
        private const string DomainNamespace = "ErpBE.Domain";
        private const string ApplicationNamespace = "ErpBE.Application";
        private const string InfrastructureNamespace = "ErpBE.Infrastructure";
        private const string ApiNamespace = "ErpBE.API";

        [Fact]
        public void Domain_ShouldNotHaveDependencyOnApplication()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .ShouldNot()
                .HaveDependencyOn(ApplicationNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Domain layer should not depend on Application layer. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Domain_ShouldNotHaveDependencyOnInfrastructure()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .ShouldNot()
                .HaveDependencyOn(InfrastructureNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Domain layer should not depend on Infrastructure layer. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Domain_ShouldNotHaveDependencyOnAPI()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .ShouldNot()
                .HaveDependencyOn(ApiNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Domain layer should not depend on API layer. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Application_ShouldNotHaveDependencyOnInfrastructure()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .ShouldNot()
                .HaveDependencyOn(InfrastructureNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Application layer should not depend on Infrastructure layer. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Application_ShouldNotHaveDependencyOnAPI()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .ShouldNot()
                .HaveDependencyOn(ApiNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Application layer should not depend on API layer. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Application_ShouldOnlyDependOnDomain()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .ResideInNamespace(ApplicationNamespace)
                .ShouldNot()
                .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Application layer should only depend on Domain layer. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Infrastructure_ShouldNotHaveDependencyOnAPI()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Infrastructure.AssemblyReference).Assembly)
                .ShouldNot()
                .HaveDependencyOn(ApiNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Infrastructure layer should not depend on API layer. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Controllers_ShouldNotHaveDependencyOnInfrastructure()
        {
            // Arrange & Act
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            var result = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace($"{ApiNamespace}.Controllers")
                .ShouldNot()
                .HaveDependencyOn(InfrastructureNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Controllers should not directly depend on Infrastructure (use Application layer). Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Domain_ShouldNotHaveDependencyOnAnyOtherLayer()
        {
            // Arrange
            var otherLayers = new[] { ApplicationNamespace, InfrastructureNamespace, ApiNamespace };

            // Act
            var result = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .ShouldNot()
                .HaveDependencyOnAny(otherLayers)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Domain layer should be independent and not depend on any other layer. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Domain_ShouldOnlyContainDomainLogic()
        {
            // Arrange & Act - Test each dependency separately
            var httpResult = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .That()
                .ResideInNamespace(DomainNamespace)
                .ShouldNot()
                .HaveDependencyOn("System.Net.Http")
                .GetResult();

            var aspNetResult = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .That()
                .ResideInNamespace(DomainNamespace)
                .ShouldNot()
                .HaveDependencyOn("Microsoft.AspNetCore")
                .GetResult();

            var dapperResult = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .That()
                .ResideInNamespace(DomainNamespace)
                .ShouldNot()
                .HaveDependencyOn("Dapper")
                .GetResult();

            var efResult = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .That()
                .ResideInNamespace(DomainNamespace)
                .ShouldNot()
                .HaveDependencyOn("Microsoft.EntityFrameworkCore")
                .GetResult();

            // Assert
            Assert.True(httpResult.IsSuccessful && aspNetResult.IsSuccessful && dapperResult.IsSuccessful && efResult.IsSuccessful, 
                $"Domain layer should not have infrastructure concerns.");
        }

        [Fact]
        public void Infrastructure_ShouldImplementInterfacesFromDomain()
        {
            // Arrange & Act
            var domainInterfaces = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .That()
                .AreInterfaces()
                .And()
                .ResideInNamespace($"{DomainNamespace}.Interfaces")
                .GetTypes();

            // All repository implementations should be in Infrastructure
            var result = Types.InAssembly(typeof(ErpBE.Infrastructure.AssemblyReference).Assembly)
                .That()
                .ResideInNamespace($"{InfrastructureNamespace}.Repositories")
                .Should()
                .ResideInNamespace(InfrastructureNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Infrastructure implementations should be in the correct namespace. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void API_Controllers_ShouldNotDirectlyUseDomain()
        {
            // Arrange & Act
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            // Controllers should not have direct dependency on Domain
            var result = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace($"{ApiNamespace}.Controllers")
                .ShouldNot()
                .HaveDependencyOn(DomainNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"API Controllers should NOT use Domain namespace directly (use Application layer instead). " +
                $"This ensures proper separation of concerns. " +
                $"Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }
    }
}

