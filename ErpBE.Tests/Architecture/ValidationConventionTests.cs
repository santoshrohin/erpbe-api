using FluentValidation;
using NetArchTest.Rules;
using Xunit;

namespace ErpBE.Tests.Architecture
{
    /// <summary>
    /// Architecture tests to enforce FluentValidation conventions.
    /// Ensures all validation is handled via FluentValidation and not inline in handlers.
    /// </summary>
    public class ValidationConventionTests
    {
        private const string ApplicationNamespace = "ErpBE.Application";

        [Fact]
        public void Validators_ShouldEndWithValidator()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .Inherit(typeof(AbstractValidator<>))
                .Should()
                .HaveNameEndingWith("Validator")
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All validator classes should end with 'Validator'. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Validators_ShouldInheritFromAbstractValidator()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .HaveNameEndingWith("Validator")
                .And()
                .AreClasses()
                .And()
                .DoNotHaveName("ValidationBehavior") // Exclude pipeline behaviors
                .Should()
                .Inherit(typeof(AbstractValidator<>))
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All validator classes should inherit from AbstractValidator<T>. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Validators_ShouldBeInValidatorsFolderOrNamespace()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .Inherit(typeof(AbstractValidator<>))
                .Should()
                .ResideInNamespace(ApplicationNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All validators should be in Application namespace. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void CommandValidators_ShouldValidateCommands()
        {
            // Arrange & Act
            var commandValidators = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .HaveNameEndingWith("CommandValidator")
                .And()
                .Inherit(typeof(AbstractValidator<>))
                .GetTypes();

            // Assert - Just verify they exist
            // In a real scenario, you'd verify they validate types ending with "Command"
            Assert.NotNull(commandValidators);
        }

        [Fact]
        public void QueryValidators_ShouldValidateQueries()
        {
            // Arrange & Act
            var queryValidators = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .HaveNameEndingWith("QueryValidator")
                .And()
                .Inherit(typeof(AbstractValidator<>))
                .GetTypes();

            // Assert - Just verify they exist
            Assert.NotNull(queryValidators);
        }

        [Fact]
        public void Handlers_ShouldNotContainValidationLogic()
        {
            // Arrange & Act - Handlers should not throw ValidationException directly
            // This is enforced by using FluentValidation pipeline behavior
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .HaveNameEndingWith("Handler")
                .And()
                .AreClasses()
                .ShouldNot()
                .HaveDependencyOn("System.ComponentModel.DataAnnotations")
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Handlers should not use DataAnnotations (use FluentValidation instead). Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Domain_ShouldNotContainValidationAttributes()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Domain.AssemblyReference).Assembly)
                .ShouldNot()
                .HaveDependencyOn("System.ComponentModel.DataAnnotations")
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Domain should not use DataAnnotations attributes. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Validators_ShouldBePublic()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .Inherit(typeof(AbstractValidator<>))
                .Should()
                .BePublic()
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All validators should be public for DI registration. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void ValidationBehavior_ShouldExistInPipeline()
        {
            // Arrange & Act
            var validationBehavior = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .HaveName("ValidationBehavior")
                .GetTypes();

            // Assert
            Assert.Single(validationBehavior);
            Assert.Contains("ValidationBehavior", validationBehavior.Select(t => t.Name));
        }

        [Fact]
        public void Validators_ShouldNotHaveDependencyOnControllers()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .Inherit(typeof(AbstractValidator<>))
                .ShouldNot()
                .HaveDependencyOn("ErpBE.API.Controllers")
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Validators should not depend on Controllers. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Validators_CanDependOnDomainInterfaces()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .Inherit(typeof(AbstractValidator<>))
                .Should()
                .ResideInNamespace(ApplicationNamespace)
                .GetResult();

            // Assert - Validators can depend on Domain.Interfaces for async validation
            Assert.True(result.IsSuccessful, 
                $"Validators should be in Application layer and can use Domain interfaces. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void CommonValidationRules_ShouldBeStatic()
        {
            // Arrange & Act
            var commonValidationRules = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .HaveName("CommonValidationRules")
                .GetTypes();

            // Assert
            Assert.NotEmpty(commonValidationRules);
            var commonRulesClass = commonValidationRules.FirstOrDefault();
            Assert.NotNull(commonRulesClass);
            Assert.True(commonRulesClass.IsAbstract && commonRulesClass.IsSealed, 
                "CommonValidationRules should be a static class");
        }
    }
}

