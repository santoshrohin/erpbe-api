using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ErpBE.Tests
{
    public abstract class TestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly WebApplicationFactory<Program> Factory;
        protected readonly HttpClient Client;

        protected TestBase(WebApplicationFactory<Program> factory)
        {
            Factory = factory;
            Client = factory.CreateClient();
        }

        protected T GetService<T>() where T : class
        {
            return Factory.Services.GetRequiredService<T>();
        }

        protected void ConfigureLogging(LogLevel level = LogLevel.Warning)
        {
            Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddLogging(logging =>
                    {
                        logging.SetMinimumLevel(level);
                    });
                });
            });
        }
    }
}
