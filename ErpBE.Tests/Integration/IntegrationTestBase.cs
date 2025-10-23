using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ErpBE.Tests.Integration
{
    /// <summary>
    /// Base class for integration tests using production database
    /// Tests should create, test, and delete their own test data
    /// </summary>
    public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly WebApplicationFactory<Program> Factory;
        protected readonly HttpClient Client;

        protected IntegrationTestBase(WebApplicationFactory<Program> factory)
        {
            Factory = factory;
            Client = Factory.CreateClient();
        }

        protected T GetService<T>() where T : class
        {
            return Factory.Services.GetRequiredService<T>();
        }

        /// <summary>
        /// Gets authentication token for testing
        /// Uses dedicated TestUser account with Admin role
        /// Note: TestUser must be created using Setup_Test_User.sql script
        /// </summary>
        protected async Task<string> GetAuthTokenAsync()
        {
            var loginRequest = new
            {
                Username = "TestUser",
                Password = "Test@123",
                CompanyId = 1,
                FinancialYearCode = -2147483641
            };

            var json = System.Text.Json.JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await Client.PostAsync("/api/Login", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new System.Exception($"Login failed for TestUser. Status: {response.StatusCode}, Error: {errorContent}. " +
                    "Make sure to run 'Setup_Test_User.sql' script first!");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(responseContent);
            
            return result.GetProperty("token").GetString()!;
        }
    }
}
