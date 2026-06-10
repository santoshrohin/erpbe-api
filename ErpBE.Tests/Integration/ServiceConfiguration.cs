using System.Data;
using Microsoft.Data.SqlClient;
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ErpBE.Application;
using ErpBE.Application.Common.Behaviors;
using ErpBE.Application.Auth.Queries.Login;
using ErpBE.Application.Audit;
using ErpBE.Application.UserManagement;
using ErpBE.Application.UnitMaster;
using ErpBE.Application.CustomerMaster.Interfaces;
using ErpBE.Application.Interfaces;
using ErpBE.Domain.Auth;
using ErpBE.Domain.Common;
using ErpBE.Infrastructure.Auth;
using ErpBE.Infrastructure.Common;
using ErpBE.Infrastructure.Repositories;
using ErpBE.Infrastructure.Services;
using ErpBE.Application.Interfaces;
using NSubstitute;

namespace ErpBE.Tests.Integration;

/// <summary>
/// Service configuration for integration tests
/// Matches reference implementation pattern - extracts service registration from Program.cs
/// </summary>
public static class ServiceConfiguration
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Controllers
        services.AddControllers();
        
        // MediatR + Validation
        services.AddMediatR(typeof(AssemblyReference).Assembly);
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        services.AddValidatorsFromAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Dapper-compatible DB Connection
        services.AddTransient<IDbConnection>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            
            // Remove unsupported connection string keywords for compatibility with libraries that don't support them
            // Some libraries don't support certain keywords at all, so we must completely remove or convert them
            if (!string.IsNullOrEmpty(connectionString))
            {
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
                
                // Convert "Multiple Active Result Sets" to "MultipleActiveResultSets" and remove it
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
                    var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(cleaned);
                    
                    // Ensure MultipleActiveResultSets is not set
                    if (builder.ContainsKey("MultipleActiveResultSets"))
                    {
                        builder.Remove("MultipleActiveResultSets");
                    }
                    
                    connectionString = builder.ConnectionString;
                }
                catch
                {
                    // If parsing fails, return the cleaned string
                    connectionString = cleaned;
                }
            }
            
            return new SqlConnection(connectionString);
        });

        // HTTP context (required by CompanyContext which reads JWT claims)
        services.AddHttpContextAccessor();

        // Application-specific services
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ILoginRepository, LoginRepository>();
        services.AddScoped<IPermissionLoader, PermissionLoader>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICompanyContext, CompanyContext>();
        services.AddScoped<IDropdownRepository, DropdownRepository>();

        // Audit services
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAuditRepository, AuditRepository>();

        // User Management services
        services.AddScoped<IUserManagementRepository, UserManagementRepository>();
        services.AddScoped<IUserManagementService, UserManagementService>();

        // Unit Master services
        services.AddScoped<IUnitMasterRepository, UnitMasterRepository>();
        services.AddScoped<IUnitMasterService, UnitMasterService>();

        // Item Category Master services
        services.AddScoped<IItemCategoryMasterRepository, ItemCategoryMasterRepository>();

        // SO Type Master services
        services.AddScoped<ISoTypeMasterRepository, SoTypeMasterRepository>();

        // Customer Type Master services
        services.AddScoped<ICustomerTypeMasterRepository, CustomerTypeMasterRepository>();

        // Customer Master services
        services.AddScoped<ICustomerMasterRepository, CustomerMasterRepository>();

        // Tax Invoice services
        services.AddScoped<ITaxInvoiceRepository, TaxInvoiceRepository>();
        services.AddScoped<IPdfService, TaxInvoicePdfService>();

        // Customer PO services
        services.AddScoped<ICustomerPoPdfService, CustomerPoPdfService>();
        services.AddScoped<ICustomerPoRepository, CustomerPoRepository>();

        // Company and Financial Year services
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IFinancialYearRepository, FinancialYearRepository>();

        // Logs Repository
        services.AddScoped<ILogsRepository, LogsRepository>();

        // Activity log (LOG_MASTER — cross-system audit trail)
        services.AddScoped<IActivityLogService, ActivityLogService>();

        // Delivery Challan services
        services.AddScoped<IDeliveryChallanRepository, DeliveryChallanRepository>();

        // Labour Charge Invoice services
        services.AddScoped<ILabourChargeInvoiceRepository, LabourChargeInvoiceRepository>();

        // Issue Master services
        services.AddScoped<IIssueMasterRepository, IssueMasterRepository>();

        // Production To Store services
        services.AddScoped<IProductionToStoreRepository, ProductionToStoreRepository>();

        // User Rights
        services.AddScoped<IUserRightRepository, UserRightRepository>();

        // JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? "default-secret-key"))
                };
            });

        // Authorization
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
                policy.RequireRole(AuthorizationRoles.Admin));

            options.AddPolicy(AuthorizationPolicies.SalesAccess, policy =>
                policy.RequireRole(AuthorizationRoles.Admin, AuthorizationRoles.SalesManager));

            options.AddPolicy(AuthorizationPolicies.StoreAccess, policy =>
                policy.RequireRole(AuthorizationRoles.Admin, AuthorizationRoles.StoreManager));

            options.AddPolicy(AuthorizationPolicies.PurchaseAccess, policy =>
                policy.RequireRole(AuthorizationRoles.Admin, AuthorizationRoles.PurchaseManager));

            options.AddPolicy(AuthorizationPolicies.ReadOnlyAccess, policy =>
                policy.RequireRole(AuthorizationRoles.Admin, AuthorizationRoles.ReadOnlyManager,
                              AuthorizationRoles.SalesManager, AuthorizationRoles.StoreManager,
                              AuthorizationRoles.PurchaseManager, AuthorizationRoles.UtilityManager));

            options.AddPolicy(AuthorizationPolicies.UtilityAccess, policy =>
                policy.RequireRole(AuthorizationRoles.Admin, AuthorizationRoles.UtilityManager));

            options.AddPolicy(AuthorizationPolicies.ManagementAccess, policy =>
                policy.RequireRole(AuthorizationRoles.Admin, AuthorizationRoles.SalesManager,
                              AuthorizationRoles.StoreManager, AuthorizationRoles.PurchaseManager));
        });

        // Logging
        services.AddLogging();

        // Options
        services.AddOptions();

        return services;
    }
}

