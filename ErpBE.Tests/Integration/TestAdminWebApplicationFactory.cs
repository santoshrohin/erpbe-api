using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ErpBE.Tests.Utilities;

namespace ErpBE.Tests.Integration;

/// <summary>
/// WebApplicationFactory that replaces JWT authentication with a fake handler
/// that always authenticates as Admin. Used for admin controller HTTP tests so
/// the [AuthorizeAdmin] policy passes and controller code gets exercised.
/// Connection string comes from the test project's appsettings.json (test DB).
/// </summary>
public class TestAdminWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace the default JWT auth with the test handler so every request
            // arrives as an authenticated Admin without needing a real DB user.
            // Connection string is left as-is (uses API's appsettings.Development.json)
            // which is the same database used by CustomerPoControllerTests.
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestAdmin";
                options.DefaultChallengeScheme = "TestAdmin";
                options.DefaultScheme = "TestAdmin";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAdminAuthHandler>("TestAdmin", _ => { });
        });
    }
}
