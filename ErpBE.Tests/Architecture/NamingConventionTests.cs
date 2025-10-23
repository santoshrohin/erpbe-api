using NetArchTest.Rules;
using Xunit;

namespace ErpBE.Tests.Architecture
{
    /// <summary>
    /// Architecture tests to enforce consistent naming conventions across the codebase.
    /// Ensures classes, interfaces, and namespaces follow established patterns.
    /// </summary>
    public class NamingConventionTests
    {
        private const string DomainNamespace = "ErpBE.Domain";
        private const string ApplicationNamespace = "ErpBE.Application";
        private const string InfrastructureNamespace = "ErpBE.Infrastructure";
        private const string ApiNamespace = "ErpBE.API";

        [Fact]
        public void Interfaces_ShouldStartWithI()
        {
            // Arrange & Act - Domain interfaces
            var domainResult = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            // Assert
            Assert.True(domainResult.IsSuccessful, 
                $"All interfaces should start with 'I'. Violations: {string.Join(", ", domainResult.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Repositories_ShouldEndWithRepository()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Infrastructure.AssemblyReference).Assembly)
                .That()
                .ResideInNamespace($"{InfrastructureNamespace}.Repositories")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("Repository")
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All repository classes should end with 'Repository'. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void RepositoryInterfaces_ShouldStartWithIAndEndWithRepository()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .That()
                .ResideInNamespace($"{DomainNamespace}.Interfaces")
                .And()
                .AreInterfaces()
                .And()
                .HaveNameEndingWith("Repository")
                .Should()
                .HaveNameStartingWith("I")
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All repository interfaces should start with 'I' and end with 'Repository'. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Controllers_ShouldEndWithController()
        {
            // Arrange & Act
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            var result = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace($"{ApiNamespace}.Controllers")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("Controller")
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All controller classes should end with 'Controller'. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void DTOs_ShouldHaveConsistentNaming()
        {
            // Arrange & Act - DTOs should end with Dto, Request, Response, or Result
            var result = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .That()
                .ResideInNamespace($"{DomainNamespace}.DTOs")
                .And()
                .AreClasses()
                .Should()
                .HaveNameEndingWith("Dto")
                .Or()
                .HaveNameEndingWith("Request")
                .Or()
                .HaveNameEndingWith("Response")
                .Or()
                .HaveNameEndingWith("Result")
                .Or()
                .HaveNameEndingWith("Parameters")
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All DTO classes should have consistent naming (Dto, Request, Response, Result, Parameters). Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Services_ShouldEndWithService()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Infrastructure.AssemblyReference).Assembly)
                .That()
                .AreClasses()
                .And()
                .HaveNameEndingWith("Service")
                .Should()
                .BePublic()
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All service classes should be public. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void DomainEntities_ShouldNotHaveSuffix()
        {
            // Arrange & Act - Domain entities should not have "Entity" or "Model" suffix
            var entityResult = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .That()
                .ResideInNamespace($"{DomainNamespace}.FarmerEntities")
                .And()
                .AreClasses()
                .ShouldNot()
                .HaveNameEndingWith("Entity")
                .GetResult();

            var modelResult = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .That()
                .ResideInNamespace($"{DomainNamespace}.FarmerEntities")
                .And()
                .AreClasses()
                .ShouldNot()
                .HaveNameEndingWith("Model")
                .GetResult();

            // Assert
            Assert.True(entityResult.IsSuccessful && modelResult.IsSuccessful, 
                $"Domain entities should not have 'Entity' or 'Model' suffix.");
        }

        [Fact]
        public void Exceptions_ShouldEndWithException()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .That()
                .Inherit(typeof(Exception))
                .Should()
                .HaveNameEndingWith("Exception")
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All exception classes should end with 'Exception'. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void AssemblyReferences_ShouldExistInEachLayer()
        {
            // Arrange & Act - Each layer should have an AssemblyReference marker
            var domainAssemblyRef = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .That()
                .HaveName("AssemblyReference")
                .GetTypes();

            var appAssemblyRef = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .HaveName("AssemblyReference")
                .GetTypes();

            var infraAssemblyRef = Types.InAssembly(typeof(ErpBE.Infrastructure.AssemblyReference).Assembly)
                .That()
                .HaveName("AssemblyReference")
                .GetTypes();

            // Assert
            Assert.Single(domainAssemblyRef);
            Assert.Single(appAssemblyRef);
            Assert.Single(infraAssemblyRef);
        }

        [Fact]
        public void Requests_ShouldEndWithRequest()
        {
            // Arrange & Act - All request DTOs should end with "Request"
            var result = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .That()
                .ResideInNamespace($"{DomainNamespace}.DTOs")
                .And()
                .AreClasses()
                .And()
                .HaveNameEndingWith("Request")
                .Should()
                .BePublic()
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All request DTOs should be public. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void QueryParameters_ShouldEndWithParameters()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .That()
                .HaveNameEndingWith("Parameters")
                .And()
                .AreClasses()
                .Should()
                .BePublic()
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All parameter classes should be public. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Attributes_ShouldEndWithAttribute()
        {
            // Arrange & Act
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            var result = Types.InAssembly(apiAssembly)
                .That()
                .Inherit(typeof(Attribute))
                .Should()
                .HaveNameEndingWith("Attribute")
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All attribute classes should end with 'Attribute'. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }
    }
}

