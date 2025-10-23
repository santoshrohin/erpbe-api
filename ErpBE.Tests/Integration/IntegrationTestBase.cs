using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ErpBE.Tests.Integration
{
    public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly WebApplicationFactory<Program> Factory;
        protected readonly HttpClient Client;
        protected readonly IConfiguration Configuration;

            protected IntegrationTestBase(WebApplicationFactory<Program> factory)
            {
                // Start test database container before running tests
                StartTestDatabase();
                
                Factory = factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        // Override connection string to use container database
                        config.AddInMemoryCollection(new Dictionary<string, string>
                        {
                            ["ConnectionStrings:DefaultConnection"] = "Server=localhost,1434;Database=ErpBE_Test;User Id=sa;Password=TestPassword123!;TrustServerCertificate=true;"
                        }!);
                    });
                    
                    builder.ConfigureServices(services =>
                    {
                        services.AddLogging(logging =>
                        {
                            logging.SetMinimumLevel(LogLevel.Warning);
                        });
                    });
                });
                
                Client = Factory.CreateClient();
                Configuration = Factory.Services.GetRequiredService<IConfiguration>();
            }
            
            private void StartTestDatabase()
            {
                try
                {
                    // Check if container is already running
                    var checkProcess = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "docker",
                            Arguments = "ps --filter name=erp-test-db --format \"{{.Names}}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    checkProcess.Start();
                    var output = checkProcess.StandardOutput.ReadToEnd();
                    checkProcess.WaitForExit();
                    
                    if (!output.Contains("erp-test-db"))
                    {
                        // Start the test database container
                        var startProcess = new System.Diagnostics.Process
                        {
                            StartInfo = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "docker-compose",
                                Arguments = "-f docker-compose.test.yml up -d",
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                CreateNoWindow = true
                            }
                        };
                        startProcess.Start();
                        startProcess.WaitForExit();
                        
                        // Wait for database to be ready
                        System.Threading.Thread.Sleep(10000);
                    }
                }
                catch (System.Exception ex)
                {
                    // If Docker is not available, tests will fail with clear error
                    throw new System.InvalidOperationException("Test database container could not be started. Make sure Docker is running.", ex);
                }
            }

        protected T GetService<T>() where T : class
        {
            return Factory.Services.GetRequiredService<T>();
        }

        protected async Task<string> GetAuthTokenAsync()
        {
            var loginRequest = new
            {
                Username = "Mohan",
                Password = "1234",
                CompanyId = 1,
                FinancialYearCode = -2147483641
            };

            var json = System.Text.Json.JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await Client.PostAsync("/api/Login", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(responseContent);
            
            return result.GetProperty("token").GetString()!;
        }
    }
}
