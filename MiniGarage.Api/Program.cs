using System.Reflection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using MiniGarage.Api.Data;
using MiniGarage.Api.Domain;
using MiniGarage.Api.Middleware;

// ════════════════════════════════════════════════════════════════════════════
// Program.cs — Mini Garage API
// Configured with HTTP/3, Entity Framework Core, Rate Limiting, CORS,
// Security Headers, and Centralized ProblemDetails Exception Handling.
// ════════════════════════════════════════════════════════════════════════════

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────────────────
// Kestrel Server Configuration (HTTP/1.1, HTTP/2, and HTTP/3 via QUIC)
// ─────────────────────────────────────────────────────────────────────────────
builder.WebHost.ConfigureKestrel(options =>
{
    // Plain HTTP endpoint (development/internal) — HTTP/1.1 & HTTP/2 over TCP
    options.ListenLocalhost(5065, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });

    // HTTPS endpoint — HTTP/1.1 & HTTP/2 (over TCP+TLS) AND HTTP/3 (over UDP+QUIC)
    options.ListenLocalhost(7299, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
        listenOptions.UseHttps();
    });
});

// ════════════════════════════════════════════════════════════════════════════
// PHASE 1: SERVICE REGISTRATION (Dependency Injection Container)
// ════════════════════════════════════════════════════════════════════════════

// 1. Controller Services
builder.Services.AddControllers();

// 2. Deep Domain Module (Workshop Registry Seam)
builder.Services.AddScoped<IWorkshopRegistry, WorkshopRegistry>();

// 3. Database Context (Entity Framework Core with SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });
});

// 3. CORS Policy Configuration (Defense against unauthorized cross-origin requests)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://garage.example.com", "https://admin.garage.example.com")
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization", "X-Admin-Key")
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });

    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "https://localhost:7299")
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization", "X-Admin-Key")
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// 4. Rate Limiting Middleware (.NET 8/9 built-in rate limiter for DoS / brute-force mitigation)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Fixed window policy: up to 60 requests per minute
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 60;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0; // Reject immediately with 429 when limit is exceeded
    });
});

// 5. Swagger / OpenAPI Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Mini Garage API",
        Version = "v1",
        Description = "ASP.NET Core Web API for managing car repairs in a workshop.\n\n" +
                      "Secured with Rate Limiting, CORS, Security Headers, and Admin Key authorization.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Workshop Security & Dev Team",
            Email = "security@minigarage.local"
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// ════════════════════════════════════════════════════════════════════════════
// BUILD APPLICATION
// ════════════════════════════════════════════════════════════════════════════
var app = builder.Build();

// ════════════════════════════════════════════════════════════════════════════
// PHASE 2: MIDDLEWARE PIPELINE
// Exact Order: ExceptionHandling -> SecurityHeaders -> Logging -> HTTPS Redirection ->
//              Swagger -> Routing -> CORS -> RateLimiting -> Authorization -> Endpoints
// ════════════════════════════════════════════════════════════════════════════

// 1. Global Exception Handler (Returns RFC 7807 ProblemDetails without leaking server internals)
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        if (exceptionFeature?.Error is not null)
        {
            logger.LogError(exceptionFeature.Error,
                "Unhandled exception occurred while processing request {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An internal server error occurred.",
            Detail = app.Environment.IsDevelopment()
                ? exceptionFeature?.Error?.Message
                : "An unexpected error occurred. Please contact the administrator.",
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

// 2. Security Headers Middleware (CSP, X-Content-Type-Options, X-Frame-Options, HSTS)
app.UseMiddleware<SecurityHeadersMiddleware>();

// 3. Custom Request Logging Middleware (Method, Path, Protocol, Status, Duration)
app.UseMiddleware<RequestLoggingMiddleware>();

// 4. HTTPS Redirection
app.UseHttpsRedirection();

// 5. Static Files & Default Page (Serves the Workshop Frontend SPA from wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

// 6. Swagger & Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mini Garage API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Mini Garage API - Swagger UI";
});

// 6. Routing Middleware
app.UseRouting();

// 7. CORS Middleware (Must be after UseRouting and before UseAuthorization)
app.UseCors(app.Environment.IsDevelopment() ? "Development" : "Production");

// 8. Rate Limiter Middleware (Applies configured rate limiting rules)
app.UseRateLimiter();

// 9. Authorization Middleware
app.UseAuthorization();

// 10. Endpoint Dispatching
app.MapControllers();

app.Run();

// Expose Program class for WebApplicationFactory in integration tests
public partial class Program { }
