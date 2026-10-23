namespace MiniGarage.Api.Middleware;

/// <summary>
/// Security Headers Middleware.
/// Injects defense-in-depth HTTP response headers to protect API and UI clients
/// against XSS, clickjacking, MIME-sniffing, and cross-origin resource embedding.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Content Security Policy (CSP):
        // Allows local static assets ('self') and inline styles/fonts while strictly blocking unapproved scripts and iframes.
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com; img-src 'self' data:; connect-src 'self'; frame-ancestors 'none'");

        // 2. Prevent MIME-type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // 3. Prevent Clickjacking
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // 4. Referrer Policy
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // 5. Permissions Policy
        context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");

        // 6. Strict Transport Security (HSTS): Force HTTPS in browsers for 1 year
        if (context.Request.IsHttps)
        {
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
        }

        await _next(context);
    }
}
