using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ErpBE.Tests.Utilities;

namespace ErpBE.Tests.Integration;

/// <summary>
/// WebApplicationFactory that authenticates every request as a non-admin user
/// with zero permissions on all modules. Use this to verify that business
/// controller endpoints correctly return 403 when the caller has no permission bits.
/// </summary>
public class TestNoPermWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestNoPerm";
                options.DefaultChallengeScheme = "TestNoPerm";
                options.DefaultScheme = "TestNoPerm";
            })
            .AddScheme<AuthenticationSchemeOptions, TestNoPermAuthHandler>("TestNoPerm", _ => { });
        });
    }
}
