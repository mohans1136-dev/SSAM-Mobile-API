using System.Text.Json.Serialization;
using Asp.Versioning;
using SsamMobileApi.Data;
using SsamMobileApi.Infrastructure;
using SsamMobileApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Scalar.AspNetCore;
using Serilog;

// ---------------------------------------------------------------------------
// Bootstrap logger: catches failures that happen before the host is built.
// ---------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // -----------------------------------------------------------------------
    // Logging (Serilog) - reads the "Serilog" section from appsettings.json
    // -----------------------------------------------------------------------
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // -----------------------------------------------------------------------
    // Database (EF Core + SQL Server)
    // Connection string comes from configuration key "ConnectionStrings:SqlDb".
    // Locally: appsettings.Development.json or user-secrets.
    // In Azure: App Service "Connection strings" blade or Key Vault.
    // -----------------------------------------------------------------------
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("SqlDb"),
            sql =>
            {
                sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                sql.CommandTimeout(30);
            }));

    // -----------------------------------------------------------------------
    // Authentication - validates the OIDC / Microsoft Entra ID bearer token
    // that OutSystems sends in the "Authorization: Bearer <jwt>" header.
    // Settings live in the "AzureAd" section of appsettings.json.
    //
    // DevAuth escape hatch: outside the AVD you may have no Entra ID access.
    // Set "DevAuth:Enabled": true in appsettings.Development.json to bypass token
    // validation and run as a fake authenticated user. Only honoured in the
    // Development environment - it can never activate in Azure.
    // -----------------------------------------------------------------------
    var useDevAuth = builder.Environment.IsDevelopment()
        && builder.Configuration.GetValue<bool>("DevAuth:Enabled");

    if (useDevAuth)
    {
        builder.Services
            .AddAuthentication(DevAuthHandler.SchemeName)
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DevAuthHandler>(DevAuthHandler.SchemeName, null);
    }
    else
    {
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
    }

    builder.Services.AddAuthorization(options =>
    {
        // Every endpoint requires a valid token unless it opts out with [AllowAnonymous].
        options.FallbackPolicy = options.DefaultPolicy;

        // Example scope-based policy. The app registration in Entra ID exposes
        // an API scope (e.g. "access_as_app" or "Distributors.Read"); require it here.
        options.AddPolicy("ReadAccess", policy =>
            policy.RequireAssertion(ctx =>
                ctx.User.HasClaim(c => (c.Type == "scp" || c.Type == "http://schemas.microsoft.com/identity/claims/scope")
                    && c.Value.Split(' ').Contains("Distributors.Read"))
                || ctx.User.HasClaim(c => c.Type == "roles" && c.Value == "Distributors.Read")));
    });

    // -----------------------------------------------------------------------
    // Controllers + JSON options
    // -----------------------------------------------------------------------
    builder.Services.AddControllers()
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

    // Turns model-validation failures and unhandled exceptions into RFC 7807 problem+json.
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // -----------------------------------------------------------------------
    // API versioning - lets you ship /api/v1, /api/v2 without breaking callers.
    // -----------------------------------------------------------------------
    builder.Services
        .AddApiVersioning(o =>
        {
            o.DefaultApiVersion = new ApiVersion(1, 0);
            o.AssumeDefaultVersionWhenUnspecified = true;
            o.ReportApiVersions = true;
            o.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc()
        .AddApiExplorer(o =>
        {
            o.GroupNameFormat = "'v'VVV";
            o.SubstituteApiVersionInUrl = true;
        });

    // -----------------------------------------------------------------------
    // OpenAPI - the machine-readable contract (served at /openapi/v1.json) that
    // OutSystems imports to generate its REST connector. Scalar renders a
    // human-friendly UI on top of it at /scalar in Development.
    // -----------------------------------------------------------------------
    builder.Services.AddOpenApi("v1", options =>
    {
        options.AddDocumentTransformer((doc, ctx, ct) =>
        {
            doc.Info.Title = "SSAM Mobile API";
            doc.Info.Version = "v1";
            return Task.CompletedTask;
        });
    });

    // -----------------------------------------------------------------------
    // CORS - only needed if a browser calls this API directly. OutSystems
    // server-side integrations do NOT need this; keep it locked down.
    // -----------------------------------------------------------------------
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    builder.Services.AddCors(o => o.AddPolicy("Default", p =>
        p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

    // -----------------------------------------------------------------------
    // Health checks - Azure App Service / load balancer pings /health.
    // -----------------------------------------------------------------------
    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("sql-db", tags: ["db", "ready"]);

    // -----------------------------------------------------------------------
    // Application services (your business logic lives behind these interfaces)
    // -----------------------------------------------------------------------
    builder.Services.AddScoped<IDistributorService, DistributorService>();

    var app = builder.Build();

    if (useDevAuth)
        app.Logger.LogWarning("DevAuth is ENABLED - all requests run as a fake authenticated user. Never deploy with this on.");

    // -----------------------------------------------------------------------
    // HTTP pipeline. Order matters.
    // -----------------------------------------------------------------------
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi().AllowAnonymous();            // GET /openapi/v1.json
        app.MapScalarApiReference().AllowAnonymous(); // GET /scalar  (interactive UI)
    }
    else
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseCors("Default");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Full exception/server detail in the JSON only outside Production.
    var verboseHealth = !app.Environment.IsProduction();

    // Liveness: is the process up? (checks no dependencies)
    app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false })
        .AllowAnonymous();

    // Readiness + DB connectivity: runs "SELECT 1" against SQL, returns JSON.
    // Use /health/db to confirm the API can talk to the database.
    app.MapHealthChecks("/health", HealthCheckResponse.Options(verboseHealth)).AllowAnonymous();
    app.MapHealthChecks("/health/db",
        HealthCheckResponse.Options(verboseHealth, c => c.Tags.Contains("db"))).AllowAnonymous();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Needed so integration tests (WebApplicationFactory<Program>) can reference this class.
public partial class Program;
