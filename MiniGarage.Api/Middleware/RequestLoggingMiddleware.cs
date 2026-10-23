using System.Diagnostics;

namespace MiniGarage.Api.Middleware;

/// <summary>
/// Custom ASP.NET Core middleware that logs every HTTP request and response,
/// including the HTTP protocol version (HTTP/1.1, HTTP/2, or HTTP/3).
/// 
/// How middleware works:
/// ASP.NET Core's HTTP pipeline is a chain of middleware components.
/// Each component receives a RequestDelegate called "next" — a delegate pointing to the
/// next middleware in the chain.
/// 
/// A middleware component can:
///   1. Do work BEFORE calling next (pre-processing incoming request).
///   2. Call next(context) to pass control deeper into the pipeline.
///   3. Do work AFTER next returns (post-processing outgoing response).
/// 
/// If a middleware does not call next, it short-circuits the pipeline.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // ── PRE-PROCESSING ───────────────────────────────────────────────────
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path;
        var protocol = context.Request.Protocol; // e.g. "HTTP/1.1", "HTTP/2", "HTTP/3"

        // Deliberately avoid logging sensitive bodies, passwords, or tokens
        _logger.LogInformation("→ [{Protocol}] Incoming request: {Method} {Path}", protocol, method, path);

        try
        {
            // Pass control to the next middleware in the pipeline
            await _next(context);
        }
        finally
        {
            // ── POST-PROCESSING ──────────────────────────────────────────────
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            if (statusCode >= 500)
            {
                _logger.LogError("← [{Protocol}] Response: {Method} {Path} → {StatusCode} ({Elapsed}ms)",
                    protocol, method, path, statusCode, elapsed);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning("← [{Protocol}] Response: {Method} {Path} → {StatusCode} ({Elapsed}ms)",
                    protocol, method, path, statusCode, elapsed);
            }
            else
            {
                _logger.LogInformation("← [{Protocol}] Response: {Method} {Path} → {StatusCode} ({Elapsed}ms)",
                    protocol, method, path, statusCode, elapsed);
            }
        }
    }
}
