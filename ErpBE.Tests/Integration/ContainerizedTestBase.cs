using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using ErpBE.Tests.Utilities;

namespace ErpBE.Tests.Integration
{
    public abstract class ContainerizedTestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly WebApplicationFactory<Program> Factory;
        protected readonly HttpClient Client;
        protected readonly IConfiguration Configuration;
        protected readonly DatabaseResetUtility DatabaseReset;

        protected ContainerizedTestBase(WebApplicationFactory<Program> factory)
        {
            Factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.Test.json", optional: false);
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
            DatabaseReset = new DatabaseResetUtility(Configuration);
        }

        protected T GetService<T>() where T : class
        {
            return Factory.Services.GetRequiredService<T>();
        }

        protected async Task ResetDatabaseAsync()
        {
            await DatabaseReset.ResetDatabaseAsync();
        }

        protected async Task<string> GetAuthTokenAsync()
        {
            var loginRequest = new
            {
                Username = "mohan",
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
            
            return result.GetProperty("accessToken").GetString()!;
        }
    }
}
