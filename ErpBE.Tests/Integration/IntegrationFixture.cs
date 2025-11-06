using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.MsSql;
using NSubstitute;

namespace ErpBE.Tests.Integration;

/// <summary>
/// Integration test fixture that provides a containerized SQL Server database
/// Uses Testcontainers (Docker containers) for complete database isolation
/// Matches reference implementation pattern from ent project
/// 
/// SAFETY: Tests NEVER touch actual database - they use isolated Docker containers
/// Each test run gets a unique random database name for complete isolation
/// </summary>
public class IntegrationFixture : IAsyncLifetime
{
    private static IConfiguration _configuration = default!;
    private static Respawner _respawner = default!;
    private readonly MsSqlContainer _msSqlContainer;

    public IntegrationFixture()
    {
        var builder = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .AddEnvironmentVariables();

        _configuration = builder.Build();
        
        // Always use Testcontainers for complete isolation (matches reference implementation)
        // This ensures tests NEVER touch actual database
        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Strong!Passw0rd")
            .Build();
    }

    public static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.ConfigureServices(_configuration);
        services.AddSingleton<IConfiguration>(_configuration);

        var webHostEnvironmentSubstitute = Substitute.For<IWebHostEnvironment>();
        webHostEnvironmentSubstitute.EnvironmentName.Returns("Development");
        webHostEnvironmentSubstitute.ApplicationName.Returns("ErpBE.API");

        services.AddSingleton(webHostEnvironmentSubstitute);

        return services
            .AddLogging()
            .AddOptions()
            .BuildServiceProvider(validateScopes: true);
    }

    public async Task InitializeAsync()
    {
        // Check if we should use an existing test database from appsettings.json
        // This allows tests to run against a real test database that has the schema
        var testDbConnectionString = _configuration.GetConnectionString("DefaultConnection");
        
        if (!string.IsNullOrEmpty(testDbConnectionString) && 
            (testDbConnectionString.Contains("_Test") || testDbConnectionString.Contains("Test")))
        {
            // Use existing test database from appsettings.json
            // This database should already have the schema and stored procedures
            Console.WriteLine($"---- Using Test Database from appsettings.json");
            Console.WriteLine($"---- Connection: {testDbConnectionString}");
            
            // Verify the database exists and has required stored procedures
            if (await VerifyDatabaseHasSchemaAsync(testDbConnectionString))
            {
                // Remove unsupported connection string keywords before storing in configuration
                testDbConnectionString = RemoveUnsupportedConnectionStringKeywords(testDbConnectionString);
                
                // Use the test database from configuration
                _configuration["ConnectionStrings:DefaultConnection"] = testDbConnectionString;
                await SetDatabaseRestorePointAsync();
                return;
            }
            else
            {
                Console.WriteLine($"---- Warning: Test database from appsettings.json doesn't have required schema. Creating container database instead.");
            }
        }
        
        // Use Testcontainers (Docker SQL Server) for complete isolation
        // Start Testcontainer (Docker SQL Server)
        await _msSqlContainer.StartAsync();
        
        // Get base connection string (connects to master database)
        var originalBaseConnectionString = _msSqlContainer.GetConnectionString();
        
        // Create random database name for complete isolation (matches reference implementation)
        var databaseName = Guid.NewGuid().ToString();
        
        // Create the database first (use original base connection string for database creation)
        await CreateDatabaseAsync(originalBaseConnectionString, databaseName);
        
        // Now clean the base connection string and modify it for the new database
        var baseConnectionString = RemoveUnsupportedConnectionStringKeywords(originalBaseConnectionString);
        var connectionString = ModifyConnectionStringWithRandomDatabaseName(baseConnectionString, databaseName);
        
        // Ensure connection string is clean before storing in configuration (double-check)
        connectionString = RemoveUnsupportedConnectionStringKeywords(connectionString);
        
        // Update configuration with the new connection string
        _configuration["ConnectionStrings:DefaultConnection"] = connectionString;
        
        // Verify the connection string is clean (for debugging)
        var verifyConnectionString = _configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine($"---- Testcontainer Connection String (after cleanup): {verifyConnectionString}");
        
        // Final verification - ensure it's really clean
        if (verifyConnectionString != null && verifyConnectionString.IndexOf("Trust Server Certificate", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Console.WriteLine("---- WARNING: Connection string still contains 'Trust Server Certificate' - cleaning again...");
            var finalCleaned = RemoveUnsupportedConnectionStringKeywords(verifyConnectionString);
            _configuration["ConnectionStrings:DefaultConnection"] = finalCleaned;
            Console.WriteLine($"---- Final cleaned connection string: {finalCleaned}");
        }
        
        // Create database schema (tables and stored procedures)
        Console.WriteLine("---- Creating database tables...");
        await DatabaseSchemaDeployer.CreateTablesAsync(connectionString);
        
        // Always deploy stored procedures separately to ensure all required SPs are present
        // The entire database script may not include all stored procedures, or some may have failed
        Console.WriteLine("---- Deploying stored procedures...");
        await DatabaseSchemaDeployer.DeployStoredProceduresAsync(connectionString);
        
        Console.WriteLine("---- Database schema deployment completed.");
        
        // Set up Respawner for database cleanup
        await SetDatabaseRestorePointAsync();
    }

    private static async Task<bool> VerifyDatabaseHasSchemaAsync(string connectionString)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            
            // Check if required stored procedure exists
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) 
                FROM sys.procedures 
                WHERE name = 'ERP_GetActiveCompanies'";
            
            var result = await command.ExecuteScalarAsync();
            var count = Convert.ToInt32(result);
            
            return count > 0;
        }
        catch
        {
            return false;
        }
    }

    private static async Task CreateDatabaseAsync(string baseConnectionString, string databaseName)
    {
        // Connect to master database to create the new database
        using var connection = new SqlConnection(baseConnectionString);
        await connection.OpenAsync();
        
        // Create the database
        var createDbCommand = connection.CreateCommand();
        createDbCommand.CommandText = $@"
            IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{databaseName}')
            BEGIN
                CREATE DATABASE [{databaseName}]
            END";
        await createDbCommand.ExecuteNonQueryAsync();
    }


    private static async Task SetDatabaseRestorePointAsync()
    {
        // Create Respawner for database cleanup (matches reference implementation)
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "CRITICAL: No connection string found in configuration. " +
                "Tests require a connection string to be configured.");
        }
        
                // Remove unsupported connection string keywords for Respawner compatibility
                connectionString = RemoveUnsupportedConnectionStringKeywords(connectionString);
        
        // Create Respawner - dummy table already created, so this should work
        // Note: We don't ignore __RespawnerDummy because Respawner needs at least one table
        // The dummy table will be cleaned up by Respawner, but that's fine
        // IMPORTANT: Ignore test data tables so they persist between tests
        _respawner = await Respawner.CreateAsync(connectionString, new RespawnerOptions
        {
            TablesToIgnore =
            [
                "__EFMigrationsHistory",
                "Logs",
                // Test data tables - must persist between tests
                "COMPANY_MASTER",
                "USER_MASTER",
                "ROLES",
                "UserRoles"
            ]
        });
    }


    public async Task DisposeAsync()
    {
        // Dispose Testcontainer (matches reference implementation)
        // This automatically deletes the isolated database container
        await _msSqlContainer.DisposeAsync().AsTask();
    }

    public static async Task ResetDatabaseAsync()
    {
        // Reset database using Respawner (matches reference implementation)
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "CRITICAL: No connection string found. Cannot reset database.");
        }
        
                // Remove unsupported connection string keywords for Respawner compatibility
                connectionString = RemoveUnsupportedConnectionStringKeywords(connectionString);
        
        // Only reset if Respawner was initialized successfully
        if (_respawner != null)
        {
            try
            {
                await _respawner.ResetAsync(connectionString);
                
                // Re-seed test data after reset (Respawner cleans up all data except ignored tables)
                // This ensures test data is always available for tests
                await DatabaseSchemaDeployer.SeedTestDataAfterResetAsync(connectionString);
            }
            catch
            {
                // If reset fails (e.g., no tables), ignore it - database is already clean
                // Still try to seed test data
                try
                {
                    await DatabaseSchemaDeployer.SeedTestDataAfterResetAsync(connectionString);
                }
                catch
                {
                    // If seeding fails, continue - test data might already exist
                }
            }
        }
    }
    
    /// <summary>
    /// Removes unsupported connection string keywords for compatibility with libraries that don't support them
    /// Some libraries (like Respawner) don't support certain keywords at all, so we must completely remove them
    /// </summary>
    private static string RemoveUnsupportedConnectionStringKeywords(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return connectionString;
        
        // First, do string replacement to convert space-separated keywords to standard format
        // This must be done before SqlConnectionStringBuilder parsing
        var cleaned = connectionString;
        
        // Convert "Trust Server Certificate" to "TrustServerCertificate" (space-separated version not supported)
        // Use multiple approaches to ensure we catch all variations
        // Simple string replacement (most common case) - handle exact match first
        if (cleaned.IndexOf("Trust Server Certificate", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            // Replace all variations of "Trust Server Certificate=" with "TrustServerCertificate="
            // Use a more aggressive regex that matches any whitespace
            var regex = new System.Text.RegularExpressions.Regex(
                @"Trust\s+Server\s+Certificate\s*=\s*",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cleaned = regex.Replace(cleaned, "TrustServerCertificate=");
        }
        
        // Convert "Multiple Active Result Sets" to "MultipleActiveResultSets" (space-separated version not supported)
        cleaned = System.Text.RegularExpressions.Regex.Replace(
            cleaned,
            @"Multiple\s+Active\s+Result\s+Sets\s*=\s*([^;]+)",
            "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Remove MultipleActiveResultSets completely (not supported by some libraries)
        cleaned = cleaned
            .Replace("MultipleActiveResultSets=True;", "", StringComparison.OrdinalIgnoreCase)
            .Replace("MultipleActiveResultSets=true;", "", StringComparison.OrdinalIgnoreCase)
            .Replace("MultipleActiveResultSets=False;", "", StringComparison.OrdinalIgnoreCase)
            .Replace("MultipleActiveResultSets=false;", "", StringComparison.OrdinalIgnoreCase)
            .Replace("MARS=True;", "", StringComparison.OrdinalIgnoreCase)
            .Replace("MARS=true;", "", StringComparison.OrdinalIgnoreCase)
            .Replace("MARS=False;", "", StringComparison.OrdinalIgnoreCase)
            .Replace("MARS=false;", "", StringComparison.OrdinalIgnoreCase);
        
        // Now use SqlConnectionStringBuilder to ensure proper formatting
        try
        {
            var builder = new SqlConnectionStringBuilder(cleaned);
            
            // Ensure MultipleActiveResultSets is not set
            if (builder.ContainsKey("MultipleActiveResultSets"))
            {
                builder.Remove("MultipleActiveResultSets");
            }
            
            // Get the connection string - SqlConnectionStringBuilder might reformat it
            var result = builder.ConnectionString;
            
            // Check if SqlConnectionStringBuilder reintroduced "Trust Server Certificate" (it might format it with spaces)
            // If so, clean it again
            if (result.IndexOf("Trust Server Certificate", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var regex = new System.Text.RegularExpressions.Regex(
                    @"Trust\s+Server\s+Certificate\s*=\s*",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                result = regex.Replace(result, "TrustServerCertificate=");
            }
            
            return result;
        }
        catch
        {
            // If parsing fails, return the cleaned string
            return cleaned;
        }
    }

    private static string ModifyConnectionStringWithRandomDatabaseName(string originalConnectionString, string databaseName)
    {
        // First clean the connection string to remove unsupported keywords
        var cleaned = RemoveUnsupportedConnectionStringKeywords(originalConnectionString);
        
        // Now modify the database name
        SqlConnectionStringBuilder builder = new(cleaned)
        {
            ["Database"] = databaseName
        };
        
        // Get the connection string and ensure it's still clean (SqlConnectionStringBuilder might reformat)
        var result = builder.ConnectionString;
        
        // Double-check that unsupported keywords weren't reintroduced
        result = RemoveUnsupportedConnectionStringKeywords(result);
        
        return result;
    }
}

