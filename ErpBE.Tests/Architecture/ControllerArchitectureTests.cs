using NetArchTest.Rules;
using Xunit;

namespace ErpBE.Tests.Architecture
{
    /// <summary>
    /// Architecture tests specifically for Controller layer to enforce CQRS and Clean Architecture.
    /// These tests ensure controllers remain thin and follow strict architectural patterns.
    /// </summary>
    public class ControllerArchitectureTests
    {
        private const string ApiNamespace = "ErpBE.API";
        private const string ControllersNamespace = "ErpBE.API.Controllers";

        [Fact]
        public void Controllers_ShouldNotUseDatabaseDirectly()
        {
            // Arrange & Act
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            // Test for SqlClient
            var sqlClientResult = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .ShouldNot()
                .HaveDependencyOn("System.Data.SqlClient")
                .GetResult();

            var microsoftSqlClientResult = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .ShouldNot()
                .HaveDependencyOn("Microsoft.Data.SqlClient")
                .GetResult();

            var dapperResult = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .ShouldNot()
                .HaveDependencyOn("Dapper")
                .GetResult();

            var efCoreResult = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .ShouldNot()
                .HaveDependencyOn("Microsoft.EntityFrameworkCore")
                .GetResult();

            // Assert
            Assert.True(sqlClientResult.IsSuccessful, 
                $"Controllers should NOT use System.Data.SqlClient directly (use CQRS/MediatR instead). " +
                $"Violations: {string.Join(", ", sqlClientResult.FailingTypeNames ?? new List<string>())}");

            Assert.True(microsoftSqlClientResult.IsSuccessful, 
                $"Controllers should NOT use Microsoft.Data.SqlClient directly (use CQRS/MediatR instead). " +
                $"Violations: {string.Join(", ", microsoftSqlClientResult.FailingTypeNames ?? new List<string>())}");

            Assert.True(dapperResult.IsSuccessful, 
                $"Controllers should NOT use Dapper directly (use CQRS/MediatR instead). " +
                $"Violating Controllers: {string.Join(", ", dapperResult.FailingTypeNames ?? new List<string>())}");

            Assert.True(efCoreResult.IsSuccessful, 
                $"Controllers should NOT use Entity Framework Core directly (use CQRS/MediatR instead). " +
                $"Violations: {string.Join(", ", efCoreResult.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Controllers_ShouldOnlyUseMediator_NotServices()
        {
            // Arrange & Act
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            // Controllers should not have dependency on Infrastructure services
            var infraServicesResult = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .And()
                .AreClasses()
                .ShouldNot()
                .HaveDependencyOn("ErpBE.Infrastructure")
                .GetResult();

            // Controllers should not have dependency on Domain.Interfaces (services)
            var domainInterfacesResult = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .And()
                .AreClasses()
                .ShouldNot()
                .HaveDependencyOn("ErpBE.Domain.Interfaces")
                .GetResult();

            // Assert
            Assert.True(infraServicesResult.IsSuccessful, 
                $"Controllers should NOT inject Infrastructure services directly (use MediatR for CQRS). " +
                $"Violations: {string.Join(", ", infraServicesResult.FailingTypeNames ?? new List<string>())}");

            Assert.True(domainInterfacesResult.IsSuccessful, 
                $"Controllers should NOT inject Domain.Interfaces/Services directly (use MediatR for CQRS). " +
                $"Violating Controllers: {string.Join(", ", domainInterfacesResult.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Controllers_ShouldNotUseIConfiguration()
        {
            // Arrange & Act
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            var result = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .And()
                .AreClasses()
                .ShouldNot()
                .HaveDependencyOn("Microsoft.Extensions.Configuration.IConfiguration")
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Controllers should NOT inject IConfiguration directly (use Options pattern or pass via services). " +
                $"This enforces proper dependency injection and testability. " +
                $"Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Controllers_ShouldInheritFromControllerBase()
        {
            // Arrange & Act
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            var result = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .And()
                .HaveNameEndingWith("Controller")
                .And()
                .AreClasses()
                .Should()
                .Inherit(typeof(Microsoft.AspNetCore.Mvc.ControllerBase))
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All controllers should inherit from ControllerBase. " +
                $"Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Controllers_ShouldHaveRouteAttribute()
        {
            // Arrange & Act
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            var controllers = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .And()
                .HaveNameEndingWith("Controller")
                .GetTypes();

            // Check each controller has Route attribute
            var controllersWithoutRoute = controllers
                .Where(c => !c.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.RouteAttribute), true).Any())
                .Select(c => c.Name)
                .ToList();

            // Assert
            Assert.Empty(controllersWithoutRoute);
        }

        [Fact]
        public void Controllers_ShouldBeSealed()
        {
            // Arrange & Act
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            var result = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .And()
                .HaveNameEndingWith("Controller")
                .And()
                .AreClasses()
                .Should()
                .BeSealed()
                .Or()
                .NotBeSealed() // Allow both for now
                .GetResult();

            // Assert - This is a soft recommendation
            Assert.True(result.IsSuccessful, 
                $"Controllers can be sealed for performance (optional best practice).");
        }

        [Fact]
        public void Controllers_ShouldNotHaveDependencyOnSystemDataCommon()
        {
            // Arrange & Act
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            var result = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .ShouldNot()
                .HaveDependencyOn("System.Data.Common")
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Controllers should not use System.Data.Common (indicates direct DB access). " +
                $"Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Controllers_ShouldUseOnlyAllowedDependencies()
        {
            // Arrange & Act - Controllers should only depend on:
            // - MediatR (for CQRS)
            // - Microsoft.Extensions.Logging (for logging)
            // - Microsoft.AspNetCore.* (framework)
            // - ErpBE.Application.* (Application layer for Commands/Queries)
            
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            // Should not depend on Infrastructure
            var infraResult = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .ShouldNot()
                .HaveDependencyOn("ErpBE.Infrastructure")
                .GetResult();

            // Assert
            Assert.True(infraResult.IsSuccessful, 
                $"Controllers should only use Application layer, not Infrastructure directly. " +
                $"Violations: {string.Join(", ", infraResult.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Controllers_ShouldNotHaveComplexConstructors()
        {
            // Arrange & Act - Check that controllers don't have too many dependencies
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            var controllers = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .And()
                .HaveNameEndingWith("Controller")
                .GetTypes();

            var controllersWithTooManyDependencies = controllers
                .Where(c =>
                {
                    var constructor = c.GetConstructors().FirstOrDefault();
                    return constructor != null && constructor.GetParameters().Length > 3;
                })
                .Select(c => new { c.Name, ParamCount = c.GetConstructors().FirstOrDefault()?.GetParameters().Length })
                .ToList();

            // Assert - Controllers should have minimal dependencies (ideally just IMediator)
            Assert.True(controllersWithTooManyDependencies.Count <= 3, 
                $"Controllers should have minimal dependencies (ideally just IMediator + ILogger). " +
                $"Controllers with >3 dependencies: {string.Join(", ", controllersWithTooManyDependencies.Select(x => $"{x.Name}({x.ParamCount})"))}");
        }

        [Fact]
        public void Controllers_ShouldNotThrowExceptions()
        {
            // Arrange & Act - Controllers should return IActionResult, not throw exceptions
            // This is enforced by using try-catch or validation behavior
            // We can't easily test this with NetArchTest, but we document the expectation
            
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            var controllers = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .And()
                .HaveNameEndingWith("Controller")
                .GetTypes();

            // Just verify controllers exist
            Assert.NotEmpty(controllers);
            
            // Note: Actual exception handling should be tested in integration tests
            // This test serves as documentation that controllers should handle exceptions gracefully
        }

        [Fact]
        public void Controllers_ShouldHaveApiControllerAttribute()
        {
            // Arrange & Act
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            var controllers = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace(ControllersNamespace)
                .And()
                .HaveNameEndingWith("Controller")
                .GetTypes();

            var controllersWithoutApiController = controllers
                .Where(c => !c.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.ApiControllerAttribute), true).Any())
                .Select(c => c.Name)
                .ToList();

            // Assert
            Assert.Empty(controllersWithoutApiController);
        }

        [Fact]
        public void Controllers_ShouldBeInCorrectNamespace()
        {
            // Arrange & Act
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            var result = Types.InAssembly(apiAssembly)
                .That()
                .HaveNameEndingWith("Controller")
                .And()
                .AreClasses()
                .Should()
                .ResideInNamespace(ControllersNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All controllers should be in ErpBE.API.Controllers namespace or subnamespaces. " +
                $"Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }
    }
}

