namespace Fidelity.Server.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Prevent clickjacking attacks
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        
        // Prevent MIME-sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        
        // Enable XSS protection (legacy browsers)
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        
        // Control referrer information
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        
        // Content Security Policy
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' data:; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'");
        
        // Permissions Policy (formerly Feature-Policy)
        context.Response.Headers.Append("Permissions-Policy",
            "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
        
        // Remove server header for security through obscurity
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");
        
        await _next(context);
    }
}
