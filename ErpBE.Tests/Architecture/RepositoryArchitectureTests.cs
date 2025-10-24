using System.Reflection;
using System.Text.RegularExpressions;
using ErpBE.Infrastructure.Repositories;
using FluentAssertions;
using Xunit;

namespace ErpBE.Tests.Architecture
{
    /// <summary>
    /// Architecture tests to enforce repository standards:
    /// - NO inline SQL queries allowed
    /// - Only stored procedures should be used
    /// </summary>
    public class RepositoryArchitectureTests
    {
        private readonly Assembly _infrastructureAssembly;

        public RepositoryArchitectureTests()
        {
            _infrastructureAssembly = typeof(UnitMasterRepository).Assembly;
        }

        [Fact]
        public void Repositories_ShouldNotContainInlineSelectQueries()
        {
            // Arrange
            var repositoryTypes = _infrastructureAssembly.GetTypes()
                .Where(t => t.Name.EndsWith("Repository") && t.IsClass && !t.IsAbstract)
                .ToList();

            var violations = new List<string>();
            var debugInfo = new List<string>();

            // Act
            foreach (var repositoryType in repositoryTypes)
            {
                var sourceCode = GetSourceCode(repositoryType);
                debugInfo.Add($"Repository: {repositoryType.Name}, Source Code Length: {sourceCode.Length}");
                
                if (!string.IsNullOrEmpty(sourceCode))
                {
                    var inlineSelectQueries = FindInlineSelectQueries(sourceCode, repositoryType.Name);
                    
                    if (inlineSelectQueries.Any())
                    {
                        violations.AddRange(inlineSelectQueries);
                    }
                }
            }

            // Assert
            violations.Should().BeEmpty(
                $"Found {violations.Count} inline SELECT queries in repositories. " +
                $"All database queries should use stored procedures only.\n\n" +
                $"Violations:\n{string.Join("\n", violations)}");
        }

        [Fact]
        public void Repositories_ShouldNotContainInlineInsertQueries()
        {
            // Arrange
            var repositoryTypes = _infrastructureAssembly.GetTypes()
                .Where(t => t.Name.EndsWith("Repository") && t.IsClass && !t.IsAbstract)
                .ToList();

            var violations = new List<string>();

            // Act
            foreach (var repositoryType in repositoryTypes)
            {
                var sourceCode = GetSourceCode(repositoryType);
                var inlineInsertQueries = FindInlineInsertQueries(sourceCode, repositoryType.Name);
                
                if (inlineInsertQueries.Any())
                {
                    violations.AddRange(inlineInsertQueries);
                }
            }

            // Assert
            violations.Should().BeEmpty(
                $"Found {violations.Count} inline INSERT queries in repositories. " +
                $"All database queries should use stored procedures only.\n" +
                $"Violations:\n{string.Join("\n", violations)}");
        }

        [Fact]
        public void Repositories_ShouldNotContainInlineUpdateQueries()
        {
            // Arrange
            var repositoryTypes = _infrastructureAssembly.GetTypes()
                .Where(t => t.Name.EndsWith("Repository") && t.IsClass && !t.IsAbstract)
                .ToList();

            var violations = new List<string>();

            // Act
            foreach (var repositoryType in repositoryTypes)
            {
                var sourceCode = GetSourceCode(repositoryType);
                var inlineUpdateQueries = FindInlineUpdateQueries(sourceCode, repositoryType.Name);
                
                if (inlineUpdateQueries.Any())
                {
                    violations.AddRange(inlineUpdateQueries);
                }
            }

            // Assert
            violations.Should().BeEmpty(
                $"Found {violations.Count} inline UPDATE queries in repositories. " +
                $"All database queries should use stored procedures only.\n" +
                $"Violations:\n{string.Join("\n", violations)}");
        }

        [Fact]
        public void Repositories_ShouldNotContainInlineDeleteQueries()
        {
            // Arrange
            var repositoryTypes = _infrastructureAssembly.GetTypes()
                .Where(t => t.Name.EndsWith("Repository") && t.IsClass && !t.IsAbstract)
                .ToList();

            var violations = new List<string>();

            // Act
            foreach (var repositoryType in repositoryTypes)
            {
                var sourceCode = GetSourceCode(repositoryType);
                var inlineDeleteQueries = FindInlineDeleteQueries(sourceCode, repositoryType.Name);
                
                if (inlineDeleteQueries.Any())
                {
                    violations.AddRange(inlineDeleteQueries);
                }
            }

            // Assert
            violations.Should().BeEmpty(
                $"Found {violations.Count} inline DELETE queries in repositories. " +
                $"All database queries should use stored procedures only.\n" +
                $"Violations:\n{string.Join("\n", violations)}");
        }

        [Fact]
        public void Repositories_ShouldOnlyUseStoredProcedures()
        {
            // Arrange
            var repositoryTypes = _infrastructureAssembly.GetTypes()
                .Where(t => t.Name.EndsWith("Repository") && t.IsClass && !t.IsAbstract)
                .ToList();

            var violations = new List<string>();

            // Act
            foreach (var repositoryType in repositoryTypes)
            {
                var sourceCode = GetSourceCode(repositoryType);
                
                // Check for CommandType.Text (inline queries)
                var textCommandTypeUsages = FindCommandTypeText(sourceCode, repositoryType.Name);
                if (textCommandTypeUsages.Any())
                {
                    violations.AddRange(textCommandTypeUsages);
                }
            }

            // Assert
            violations.Should().BeEmpty(
                $"Found {violations.Count} instances of CommandType.Text in repositories. " +
                $"All Dapper calls should use CommandType.StoredProcedure.\n" +
                $"Violations:\n{string.Join("\n", violations)}");
        }

        #region Helper Methods

        private string GetSourceCode(Type type)
        {
            // Get the workspace root from the current assembly location
            // Test assembly is in: D:\...\API\API\ErpBE.Tests\bin\Debug\net9.0\ErpBE.Tests.dll
            // We need to go to: D:\...\API\API\ErpBE.Infrastructure\Repositories\
            var testAssemblyLocation = typeof(RepositoryArchitectureTests).Assembly.Location;
            var testBinDir = Path.GetDirectoryName(testAssemblyLocation); // .../ErpBE.Tests/bin/Debug/net9.0
            var testProjectRoot = Path.GetFullPath(Path.Combine(testBinDir!, "..", "..", "..")); // .../ErpBE.Tests
            var workspaceRoot = Path.GetFullPath(Path.Combine(testProjectRoot, "..")); // .../API
            var sourceFilePath = Path.Combine(workspaceRoot, "ErpBE.Infrastructure", "Repositories", $"{type.Name}.cs");

            if (!File.Exists(sourceFilePath))
            {
                // For debugging
                return string.Empty;
            }

            return File.ReadAllText(sourceFilePath);
        }

        private List<string> FindInlineSelectQueries(string sourceCode, string repositoryName)
        {
            var violations = new List<string>();

            // Pattern: Any string literal containing SELECT with FROM/WHERE/JOIN
            // This covers "SELECT...", @"SELECT...", $"SELECT..."
            var selectPattern = new Regex(@"[@$]?""[^""]*?\bSELECT\b[^""]*?\b(FROM|WHERE|JOIN)\b[^""]*?""", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = selectPattern.Matches(sourceCode);

            foreach (Match match in matches)
            {
                // Exclude if it's part of a stored procedure name (e.g., "ERP_GetSomething")
                var matchValue = match.Value;
                if (!matchValue.Contains("ERP_", StringComparison.OrdinalIgnoreCase) && 
                    !matchValue.Contains("SP_", StringComparison.OrdinalIgnoreCase))
                {
                    var lineNumber = GetLineNumber(sourceCode, match.Index);
                    var preview = matchValue.Length > 80 ? matchValue.Substring(0, 80) + "..." : matchValue;
                    violations.Add($"  ❌ {repositoryName} (Line {lineNumber}): {preview}");
                }
            }

            return violations;
        }

        private List<string> FindInlineInsertQueries(string sourceCode, string repositoryName)
        {
            var violations = new List<string>();

            var insertPattern = new Regex(@"[@$]?""[^""]*?\bINSERT\b[^""]*?\bINTO\b[^""]*?""", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = insertPattern.Matches(sourceCode);

            foreach (Match match in matches)
            {
                var matchValue = match.Value;
                if (!matchValue.Contains("ERP_", StringComparison.OrdinalIgnoreCase) && 
                    !matchValue.Contains("SP_", StringComparison.OrdinalIgnoreCase))
                {
                    var lineNumber = GetLineNumber(sourceCode, match.Index);
                    var preview = matchValue.Length > 80 ? matchValue.Substring(0, 80) + "..." : matchValue;
                    violations.Add($"  ❌ {repositoryName} (Line {lineNumber}): {preview}");
                }
            }

            return violations;
        }

        private List<string> FindInlineUpdateQueries(string sourceCode, string repositoryName)
        {
            var violations = new List<string>();

            var updatePattern = new Regex(@"[@$]?""[^""]*?\bUPDATE\b[^""]*?\bSET\b[^""]*?""", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = updatePattern.Matches(sourceCode);

            foreach (Match match in matches)
            {
                var matchValue = match.Value;
                if (!matchValue.Contains("ERP_", StringComparison.OrdinalIgnoreCase) && 
                    !matchValue.Contains("SP_", StringComparison.OrdinalIgnoreCase))
                {
                    var lineNumber = GetLineNumber(sourceCode, match.Index);
                    var preview = matchValue.Length > 80 ? matchValue.Substring(0, 80) + "..." : matchValue;
                    violations.Add($"  ❌ {repositoryName} (Line {lineNumber}): {preview}");
                }
            }

            return violations;
        }

        private List<string> FindInlineDeleteQueries(string sourceCode, string repositoryName)
        {
            var violations = new List<string>();

            var deletePattern = new Regex(@"[@$]?""[^""]*?\bDELETE\b[^""]*?\bFROM\b[^""]*?""", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var matches = deletePattern.Matches(sourceCode);

            foreach (Match match in matches)
            {
                var matchValue = match.Value;
                if (!matchValue.Contains("ERP_", StringComparison.OrdinalIgnoreCase) && 
                    !matchValue.Contains("SP_", StringComparison.OrdinalIgnoreCase))
                {
                    var lineNumber = GetLineNumber(sourceCode, match.Index);
                    var preview = matchValue.Length > 80 ? matchValue.Substring(0, 80) + "..." : matchValue;
                    violations.Add($"  ❌ {repositoryName} (Line {lineNumber}): {preview}");
                }
            }

            return violations;
        }

        private List<string> FindCommandTypeText(string sourceCode, string repositoryName)
        {
            var violations = new List<string>();

            // Look for CommandType.Text or commandType: CommandType.Text
            var commandTypePattern = new Regex(@"CommandType\.Text|commandType:\s*CommandType\.Text", RegexOptions.IgnoreCase);
            var matches = commandTypePattern.Matches(sourceCode);

            foreach (Match match in matches)
            {
                var lineNumber = GetLineNumber(sourceCode, match.Index);
                violations.Add($"  ❌ {repositoryName} (Line ~{lineNumber}): Using CommandType.Text instead of CommandType.StoredProcedure");
            }

            return violations;
        }

        private int GetLineNumber(string sourceCode, int index)
        {
            var lineNumber = 1;
            for (int i = 0; i < index && i < sourceCode.Length; i++)
            {
                if (sourceCode[i] == '\n')
                {
                    lineNumber++;
                }
            }
            return lineNumber;
        }

        #endregion
    }
}

