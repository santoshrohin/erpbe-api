using NetArchTest.Rules;
using Xunit;

namespace ErpBE.Tests.Architecture
{
    /// <summary>
    /// Architecture tests to enforce CQRS pattern - Controllers must use MediatR, not repositories directly
    /// </summary>
    public class CqrsArchitectureTests
    {
        private const string ApiNamespace = "ErpBE.API";
        private const string ApplicationNamespace = "ErpBE.Application";
        private const string InfrastructureNamespace = "ErpBE.Infrastructure";

        [Fact]
        public void Controllers_ShouldNotHaveRepositoryDependencies()
        {
            // Arrange & Act - Controllers should not have repository interface dependencies
            // Service interfaces (like IPdfService, ICustomerPoPdfService) are allowed
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            // Get all controller classes
            var controllers = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace($"{ApiNamespace}.Controllers")
                .And()
                .AreClasses()
                .GetTypes();

            var violations = new List<string>();

            foreach (var controller in controllers)
            {
                // Check constructor parameters for repository interfaces (not service interfaces)
                var constructors = controller.GetConstructors();
                foreach (var constructor in constructors)
                {
                    var repositoryParams = constructor.GetParameters()
                        .Where(p => p.ParameterType.IsInterface &&
                               (p.ParameterType.Name.Contains("Repository") || 
                                p.ParameterType.Name.Contains("ICompanyRepository") ||
                                p.ParameterType.Name.Contains("IFinancialYearRepository") ||
                                p.ParameterType.Name.Contains("ICustomerPoRepository") ||
                                p.ParameterType.Name.Contains("ITaxInvoiceRepository") ||
                                p.ParameterType.Name.Contains("ILoginRepository") ||
                                p.ParameterType.Name.Contains("IDropdownRepository") ||
                                p.ParameterType.Name.Contains("IAuditRepository") ||
                                p.ParameterType.Name.Contains("ILogsRepository") ||
                                p.ParameterType.Name.Contains("IUnitMasterRepository") ||
                                p.ParameterType.Name.Contains("IUserManagementRepository") ||
                                p.ParameterType.Name.Contains("ICustomerMasterRepository") ||
                                p.ParameterType.Name.Contains("IItemCategoryMasterRepository") ||
                                p.ParameterType.Name.Contains("ICustomerTypeMasterRepository") ||
                                p.ParameterType.Name.Contains("ISoTypeMasterRepository") ||
                                p.ParameterType.Name.Contains("IRoleRepository") ||
                                p.ParameterType.Name.Contains("IUserRepository")) &&
                               !p.ParameterType.Name.Contains("Service") && // Exclude services
                               !p.ParameterType.Name.Contains("Pdf")); // Exclude PDF services

                    if (repositoryParams.Any())
                    {
                        violations.Add($"{controller.Name} constructor has repository parameter: {string.Join(", ", repositoryParams.Select(p => p.ParameterType.Name))}");
                    }
                }
            }

            // Assert
            Assert.Empty(violations);
        }

        [Fact]
        public void Controllers_ShouldUseMediatR()
        {
            // Arrange & Act - Controllers should depend on MediatR (except TestExceptionController which is just for testing)
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            var controllers = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace($"{ApiNamespace}.Controllers")
                .And()
                .AreClasses()
                .GetTypes()
                .Where(t => t.Name != "TestExceptionController"); // Exclude test controller

            var violations = new List<string>();

            foreach (var controller in controllers)
            {
                // Check if controller has MediatR dependency
                var hasMediatR = controller.GetConstructors()
                    .SelectMany(c => c.GetParameters())
                    .Any(p => p.ParameterType.Name == "IMediator");

                if (!hasMediatR)
                {
                    violations.Add(controller.Name);
                }
            }

            // Assert
            Assert.Empty(violations);
        }

        [Fact]
        public void Controllers_ShouldNotCallRepositoryMethods()
        {
            // Arrange & Act - Controllers should not have methods that call repository methods
            // This is a structural check - we check that controllers don't have repository fields
            var apiAssembly = System.Reflection.Assembly.Load("ErpBE.API");
            
            // Get all controller classes
            var controllers = Types.InAssembly(apiAssembly)
                .That()
                .ResideInNamespace($"{ApiNamespace}.Controllers")
                .And()
                .AreClasses()
                .GetTypes();

            var violations = new List<string>();

            foreach (var controller in controllers)
            {
                // Check for repository fields
                var repositoryFields = controller.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Where(f => f.FieldType.Name.Contains("Repository") && f.FieldType.IsInterface);

                if (repositoryFields.Any())
                {
                    violations.Add($"{controller.Name} has repository dependency: {string.Join(", ", repositoryFields.Select(f => f.FieldType.Name))}");
                }

                // Check constructor parameters
                var constructors = controller.GetConstructors();
                foreach (var constructor in constructors)
                {
                    var repositoryParams = constructor.GetParameters()
                        .Where(p => p.ParameterType.Name.Contains("Repository") && p.ParameterType.IsInterface);

                    if (repositoryParams.Any())
                    {
                        violations.Add($"{controller.Name} constructor has repository parameter: {string.Join(", ", repositoryParams.Select(p => p.ParameterType.Name))}");
                    }
                }
            }

            // Assert
            Assert.Empty(violations);
        }

        [Fact]
        public void Handlers_ShouldHaveRepositoryDependencies()
        {
            // Arrange & Act - Query/Command handlers SHOULD have repository dependencies (they're allowed)
            var applicationAssembly = System.Reflection.Assembly.Load("ErpBE.Application");
            var handlers = Types.InAssembly(applicationAssembly)
                .That()
                .HaveNameEndingWith("Handler")
                .And()
                .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
                .GetTypes();

            // Assert - Just verify handlers exist and can have repository dependencies
            // This test ensures handlers are the correct place for repository access
            Assert.NotEmpty(handlers);
        }
    }
}

