using System.Data;
using System.Threading.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi.Models;
using MediatR;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
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
using ErpBE.Infrastructure.BackgroundServices;
using ErpBE.API.Middleware;
using ErpBE.API.Common;
using ErpBE.API.Common.Settings;

var builder = WebApplication.CreateBuilder(args);

// ══════════════════════════════════════════════════════════════════════════════
// SERILOG — must be first so all subsequent startup is logged
// ══════════════════════════════════════════════════════════════════════════════
var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft",                  LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console();

try
{
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(connStr))
    {
        loggerConfig.WriteTo.MSSqlServer(
            connectionString: connStr,
            sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
            {
                TableName       = "Logs",
                SchemaName      = "dbo",
                AutoCreateSqlTable = true
            },
            columnOptions: new Serilog.Sinks.MSSqlServer.ColumnOptions
            {
                AdditionalColumns =
                [
                    new() { ColumnName = "UserId",     DataType = System.Data.SqlDbType.NVarChar, DataLength = 50  },
                    new() { ColumnName = "RequestId",  DataType = System.Data.SqlDbType.NVarChar, DataLength = 50  },
                    new() { ColumnName = "ActionName", DataType = System.Data.SqlDbType.NVarChar, DataLength = 100 }
                ]
            });
    }
}
catch (Exception ex)
{
    Console.WriteLine($"WARNING: SQL Server log sink failed: {ex.Message}. Using console only.");
}

Log.Logger = loggerConfig.CreateLogger();
builder.Host.UseSerilog();
Log.Information("ErpBE API starting — Environment: {Env}", builder.Environment.EnvironmentName);

// ══════════════════════════════════════════════════════════════════════════════
// CONTROLLERS + SWAGGER
// ══════════════════════════════════════════════════════════════════════════════
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor(); // required for ICompanyContext
builder.Services.Configure<CookieSettings>(builder.Configuration.GetSection(CookieSettings.SectionName));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "ErpBE API",
        Version     = "v1",
        Description = "SunElectro ERP — Modern Backend (Phased Migration)"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer token. Example: \"Bearer {token}\"",
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            []
        }
    });
    var xmlPath = Path.Combine(AppContext.BaseDirectory,
        $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

// ══════════════════════════════════════════════════════════════════════════════
// MEDIATR + FLUENT VALIDATION
// ══════════════════════════════════════════════════════════════════════════════
builder.Services.AddMediatR(typeof(AssemblyReference).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddValidatorsFromAssembly(typeof(ErpBE.Application.AssemblyReference).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped<ValidationExceptionHandler>();
builder.Services.AddControllers(o => o.Filters.Add<ValidationExceptionHandler>());

// ══════════════════════════════════════════════════════════════════════════════
// DATABASE (Dapper)
// ══════════════════════════════════════════════════════════════════════════════
// Transient — each resolve gets a new SqlConnection (Dapper manages open/close)
builder.Services.AddTransient<IDbConnection>(sp =>
{
    var cfg   = sp.GetRequiredService<IConfiguration>();
    var connStr = cfg.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
    return new SqlConnection(connStr);
});

// ══════════════════════════════════════════════════════════════════════════════
// AUTH SERVICES
// ══════════════════════════════════════════════════════════════════════════════
builder.Services.AddScoped<IJwtTokenGenerator,     JwtTokenGenerator>();
builder.Services.AddScoped<ILoginRepository,       LoginRepository>();
builder.Services.AddScoped<IPermissionLoader,      PermissionLoader>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ICompanyContext,        CompanyContext>();

// ══════════════════════════════════════════════════════════════════════════════
// DOMAIN SERVICES
// ══════════════════════════════════════════════════════════════════════════════
builder.Services.AddScoped<IDropdownRepository,          DropdownRepository>();
builder.Services.AddScoped<IAuditService,                AuditService>();
builder.Services.AddScoped<IAuditRepository,             AuditRepository>();
builder.Services.AddScoped<IUserManagementRepository,    UserManagementRepository>();
builder.Services.AddScoped<IUserManagementService,       UserManagementService>();
builder.Services.AddScoped<IUnitMasterRepository,        UnitMasterRepository>();
builder.Services.AddScoped<IUnitMasterService,           UnitMasterService>();
builder.Services.AddScoped<IItemCategoryMasterRepository, ItemCategoryMasterRepository>();
builder.Services.AddScoped<ISoTypeMasterRepository,      SoTypeMasterRepository>();
builder.Services.AddScoped<ICustomerTypeMasterRepository, CustomerTypeMasterRepository>();
builder.Services.AddScoped<ICustomerMasterRepository,    CustomerMasterRepository>();
builder.Services.AddScoped<ITaxInvoiceRepository,        TaxInvoiceRepository>();
builder.Services.AddScoped<IPdfService,                  TaxInvoicePdfService>();
builder.Services.AddScoped<ICustomerPoPdfService,        CustomerPoPdfService>();
builder.Services.AddScoped<ICustomerPoRepository,        CustomerPoRepository>();
builder.Services.AddScoped<ICompanyRepository,           CompanyRepository>();
builder.Services.AddScoped<IFinancialYearRepository,     FinancialYearRepository>();
builder.Services.AddScoped<ILogsRepository,              LogsRepository>();
builder.Services.AddScoped<IActivityLogService,          ActivityLogService>();
builder.Services.AddScoped<IDeliveryChallanRepository,      DeliveryChallanRepository>();
builder.Services.AddScoped<IDeliveryChallanPdfService,      DeliveryChallanPdfService>();
builder.Services.AddScoped<IIssueMasterRepository,          IssueMasterRepository>();
builder.Services.AddScoped<IProductionToStoreRepository,    ProductionToStoreRepository>();
builder.Services.AddScoped<IUserRightRepository,            UserRightRepository>();
builder.Services.AddScoped<ILabourChargeInvoiceRepository,  LabourChargeInvoiceRepository>();
builder.Services.AddScoped<ILabourChargeInvoicePdfService,  LabourChargeInvoicePdfService>();
builder.Services.AddScoped<IAdminLockRepository,            AdminLockRepository>();

// Runs inside this process — no separate deployment needed.
// Sweeps stale locks every 15 min. Add new module tables to ERP_CleanStaleLocks.sql only.
builder.Services.AddHostedService<StaleLockCleanupService>();

// ══════════════════════════════════════════════════════════════════════════════
// JWT AUTHENTICATION
// ══════════════════════════════════════════════════════════════════════════════
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var jwtSecret   = jwtSettings["Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret is not configured. Set via environment variable JWTSETTINGS__SECRET.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings["Issuer"]   ?? "ErpBE.API",
            ValidAudience            = jwtSettings["Audience"] ?? "ErpBE.Client",
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew                = TimeSpan.FromSeconds(30)
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Log.Warning("JWT authentication failed: {Error}", ctx.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

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

// ══════════════════════════════════════════════════════════════════════════════
// CORS — restrictive; no AllowAnyOrigin in production
// ══════════════════════════════════════════════════════════════════════════════
var allowedOrigin = builder.Configuration["Cors:AllowedOrigin"];
if (string.IsNullOrWhiteSpace(allowedOrigin))
{
    Log.Warning("Cors:AllowedOrigin is not configured. CORS will block all cross-origin requests.");
    allowedOrigin = "http://localhost:5173"; // safe dev default
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("ERP", policy =>
    {
        policy.WithOrigins(allowedOrigin)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();      // required for httpOnly cookie on cross-origin
    });
});

// ══════════════════════════════════════════════════════════════════════════════
// RATE LIMITING
// ══════════════════════════════════════════════════════════════════════════════
builder.Services.AddRateLimiter(options =>
{
    // Login endpoint: max 10 attempts per minute per IP
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit      = 10;
        opt.Window           = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit       = 0;
    });

    // General API: 300 requests/minute per IP (reasonable for an ERP)
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.PermitLimit = 300;
        opt.Window      = TimeSpan.FromMinutes(1);
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (ctx, _) =>
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await ctx.HttpContext.Response.WriteAsJsonAsync(new
        {
            status  = 429,
            error   = "Too Many Requests",
            message = "Rate limit exceeded. Please slow down."
        });
    };
});

// ══════════════════════════════════════════════════════════════════════════════
// BUILD
// ══════════════════════════════════════════════════════════════════════════════
var app = builder.Build();

// ──────────────────────────────────────────────────────────────────────────────
// Security headers — applied before any other middleware
// ──────────────────────────────────────────────────────────────────────────────
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options",  "nosniff");
    context.Response.Headers.Append("X-Frame-Options",         "DENY");
    context.Response.Headers.Append("X-XSS-Protection",        "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy",         "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy",      "geolocation=(), microphone=()");
    // CSP is permissive for the SPA served from the same origin
    context.Response.Headers.Append("Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;");
    await next();
});

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment() ||
    app.Configuration.GetValue<bool>("EnableSwaggerInProduction", false))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ErpBE API v1");
        c.RoutePrefix            = "swagger";
        c.DocumentTitle          = "ErpBE API";
        c.DefaultModelsExpandDepth(-1);
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
    });
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapGet("/logs", () => Results.Redirect("/logs.html"));

app.UseRouting();
app.UseCors("ERP");

app.UseRateLimiter();

app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);
Log.Information("ErpBE API started successfully.");
app.Run();

public partial class Program { }
