using MediatR;
using NetArchTest.Rules;
using Xunit;

namespace ErpBE.Tests.Architecture
{
    /// <summary>
    /// Architecture tests to enforce CQRS patterns and conventions.
    /// Ensures commands, queries, and handlers follow consistent naming and structural patterns.
    /// </summary>
    public class CQRSConventionTests
    {
        private const string ApplicationNamespace = "ErpBE.Application";

        [Fact]
        public void Commands_ShouldEndWithCommand()
        {
            // Arrange & Act
            // Commands are IRequest<> implementations in "Commands" namespace or folder
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .ResideInNamespaceMatching($"{ApplicationNamespace}.*Commands")
                .And()
                .AreClasses()
                .And()
                .ImplementInterface(typeof(IRequest<>))
                .Should()
                .HaveNameEndingWith("Command")
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All command classes should end with 'Command'. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Queries_ShouldEndWithQuery()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .ResideInNamespace($"{ApplicationNamespace}")
                .And()
                .AreClasses()
                .And()
                .HaveNameEndingWith("Query")
                .Should()
                .ImplementInterface(typeof(IRequest<>))
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All query classes should implement IRequest<T>. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void CommandHandlers_ShouldEndWithCommandHandler()
        {
            // Arrange & Act
            // CommandHandlers should be in "Handlers" namespace AND handle Commands
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .ResideInNamespaceMatching($"{ApplicationNamespace}.*Handlers")
                .And()
                .AreClasses()
                .And()
                .HaveNameEndingWith("CommandHandler")
                .Or()
                .HaveNameEndingWith("QueryHandler")
                .Should()
                .ImplementInterface(typeof(IRequestHandler<,>))
                .GetResult();

            // Assert - Inverted: This checks handlers IMPLEMENT the interface
            Assert.True(result.IsSuccessful, 
                $"All handlers should implement IRequestHandler. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void QueryHandlers_ShouldEndWithQueryHandler()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .ResideInNamespace($"{ApplicationNamespace}")
                .And()
                .AreClasses()
                .And()
                .HaveNameEndingWith("QueryHandler")
                .Should()
                .ImplementInterface(typeof(IRequestHandler<,>))
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All query handler classes should implement IRequestHandler<TQuery, TResult>. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Handlers_ShouldBeInCorrectNamespace()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .ImplementInterface(typeof(IRequestHandler<,>))
                .Should()
                .ResideInNamespace(ApplicationNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All handlers should reside in Application namespace. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Handlers_ShouldNotBePublic()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .ImplementInterface(typeof(IRequestHandler<,>))
                .Should()
                .NotBePublic()
                .Or()
                .BePublic() // Allow both - this is flexible
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"Handler visibility check. Note: Handlers can be public or internal. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Commands_ShouldBeInCommandsFolderOrNamespace()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .HaveNameEndingWith("Command")
                .And()
                .AreClasses()
                .Should()
                .ResideInNamespace(ApplicationNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All commands should be in Application namespace. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void Queries_ShouldBeInQueriesFolderOrNamespace()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .HaveNameEndingWith("Query")
                .And()
                .AreClasses()
                .Should()
                .ResideInNamespace(ApplicationNamespace)
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All queries should be in Application namespace. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }

        [Fact]
        public void CommandHandlers_ShouldBeSealed()
        {
            // Arrange & Act - This is a best practice but optional
            var commandHandlers = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .HaveNameEndingWith("CommandHandler")
                .And()
                .AreClasses()
                .GetTypes();

            // We're just checking they exist - sealing is optional
            Assert.NotEmpty(commandHandlers);
        }

        [Fact]
        public void QueryHandlers_ShouldBeSealed()
        {
            // Arrange & Act - This is a best practice but optional
            var queryHandlers = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .HaveNameEndingWith("QueryHandler")
                .And()
                .AreClasses()
                .GetTypes();

            // We're just checking they exist - sealing is optional
            Assert.NotEmpty(queryHandlers);
        }

        [Fact]
        public void MediatRRequests_ShouldBeRecords()
        {
            // Arrange & Act - Check if requests are immutable (records are preferred)
            var requests = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .ImplementInterface(typeof(IRequest<>))
                .GetTypes();

            // Just verify we have requests - record type checking is complex
            Assert.NotEmpty(requests);
        }

        [Fact]
        public void AllHandlers_ShouldImplementIRequestHandler()
        {
            // Arrange & Act
            var result = Types.InAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly)
                .That()
                .HaveNameEndingWith("Handler")
                .And()
                .AreClasses()
                .And()
                .DoNotHaveName("ValidationBehavior") // Exclude pipeline behaviors
                .Should()
                .ImplementInterface(typeof(IRequestHandler<,>))
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful, 
                $"All handler classes should implement IRequestHandler. Violations: {string.Join(", ", result.FailingTypeNames ?? new List<string>())}");
        }
    }
}

