using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Serilog.Events;

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
using ErpBE.API.Middleware;
using ErpBE.API.Common;
using Microsoft.AspNetCore.Components.WebAssembly.Server;

var builder = WebApplication.CreateBuilder(args);

// ------------------ Serilog Configuration ------------------
// Configure Serilog - Use SQL Server logging for both development and production
// Skip SQL Server logging in Testing environment to avoid conflicts with test database
var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console();

// Configure SQL Server logging with error handling
try
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connectionString))
    {
        loggerConfig.WriteTo.MSSqlServer(
            connectionString: connectionString,
            sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
            {
                TableName = "Logs",
                SchemaName = "dbo",
                AutoCreateSqlTable = true
            },
            columnOptions: new Serilog.Sinks.MSSqlServer.ColumnOptions
            {
                // Additional custom columns
                AdditionalColumns = new List<Serilog.Sinks.MSSqlServer.SqlColumn>
                {
                    new Serilog.Sinks.MSSqlServer.SqlColumn { ColumnName = "UserId", DataType = System.Data.SqlDbType.NVarChar, DataLength = 50 },
                    new Serilog.Sinks.MSSqlServer.SqlColumn { ColumnName = "RequestId", DataType = System.Data.SqlDbType.NVarChar, DataLength = 50 },
                    new Serilog.Sinks.MSSqlServer.SqlColumn { ColumnName = "ActionName", DataType = System.Data.SqlDbType.NVarChar, DataLength = 100 }
                }
            });
    }
}
catch (Exception ex)
{
    // If SQL Server logging fails, continue with console logging only
    Console.WriteLine($"WARNING: Could not initialize SQL Server logging: {ex.Message}");
    Console.WriteLine("Application will continue with console logging only.");
}

Log.Logger = loggerConfig.CreateLogger();

builder.Host.UseSerilog();

// Test logging immediately
Log.Information("Application starting up at {Timestamp}", DateTime.Now);
Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);

// ------------------ Services Registration ------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "ErpBE API", 
        Version = "v1",
        Description = "ERP Backend API with Role-Based Authorization, Auditing, and Logging",
        Contact = new OpenApiContact
        {
            Name = "ERP Development Team",
            Email = "support@erp.com"
        }
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    
    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// MediatR + Validation
builder.Services.AddMediatR(typeof(AssemblyReference).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddValidatorsFromAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Add validation exception handler
builder.Services.AddScoped<ValidationExceptionHandler>();

// Add exception filter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationExceptionHandler>();
});

// Dapper-compatible DB Connection
// Use sp.GetRequiredService<IConfiguration>() instead of builder.Configuration
// This allows test overrides to work by reading config at runtime, not at registration time
builder.Services.AddTransient<IDbConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new SqlConnection(connectionString);
});

// Application-specific services
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<ILoginRepository, LoginRepository>();
builder.Services.AddScoped<IDropdownRepository, DropdownRepository>();

// Audit services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();

// User Management services
builder.Services.AddScoped<IUserManagementRepository, UserManagementRepository>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

// Unit Master services
builder.Services.AddScoped<IUnitMasterRepository, UnitMasterRepository>();
builder.Services.AddScoped<IUnitMasterService, UnitMasterService>();

// Item Category Master services
builder.Services.AddScoped<IItemCategoryMasterRepository, ItemCategoryMasterRepository>();

// SO Type Master services
builder.Services.AddScoped<ISoTypeMasterRepository, SoTypeMasterRepository>();

// Customer Type Master services
builder.Services.AddScoped<ICustomerTypeMasterRepository, CustomerTypeMasterRepository>();

// Customer Master services
builder.Services.AddScoped<ICustomerMasterRepository, CustomerMasterRepository>();

// Tax Invoice services
builder.Services.AddScoped<ITaxInvoiceRepository, TaxInvoiceRepository>();
builder.Services.AddScoped<IPdfService, TaxInvoicePdfService>();

// Customer PO services
builder.Services.AddScoped<ICustomerPoRepository, CustomerPoRepository>();

// Logs Repository
builder.Services.AddScoped<ILogsRepository, LogsRepository>();

// Audit services
builder.Services.AddScoped<IAuditService, AuditService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
builder.Services.AddAuthorization(options =>
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

// CORS - Allow all (for dev; restrict in prod)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ------------------ App Build & Middleware ------------------

var app = builder.Build();

// IMPORTANT: Use custom exception handler for ALL environments (dev and production)
// This returns detailed exception information instead of generic 500 errors
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Enable Swagger in development and for SmarterASP temp URL
if (app.Environment.IsDevelopment() || 
    app.Configuration.GetValue<bool>("EnableSwaggerInProduction", false))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ErpBE API v1");
        c.RoutePrefix = "swagger"; // Use /swagger path in production
        c.DocumentTitle = "ErpBE API Documentation";
        c.DefaultModelsExpandDepth(-1); // Hide models section by default
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
    });
}
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

// Serve the logs viewer
app.MapGet("/logs", () => Results.Redirect("/logs.html"));
app.UseRouting();        // ✅ Required before CORS
app.UseCors();           // ✅ CORS policy applied

// Add custom middleware
app.UseMiddleware<RequestResponseLoggingMiddleware>();
// app.UseMiddleware<AuditMiddleware>(); // Removed - using service-based audit logging

app.UseAuthentication(); // ✅ JWT Authentication
app.UseAuthorization();  // ✅ Role-based Authorization

app.MapControllers();    // Map endpoints
app.MapFallbackToFile("index.html");

// Ensure logs are flushed on shutdown
app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

app.Run();

// Make Program class accessible for testing
public partial class Program { }
