using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace PaymentSystem.API.Middleware;

public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimiter _rateLimiter;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _rateLimiter = CreateRateLimiter();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using var lease = await _rateLimiter.AcquireAsync();
        if (lease.IsAcquired)
        {
            await _next(context);
        }
        else
        {
            _logger.LogWarning("Rate limit exceeded for IP: {RemoteIpAddress}", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Too many requests. Please try again later.");
        }
    }

    private static RateLimiter CreateRateLimiter()
    {
        return new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 100,
            QueueLimit = 10,
            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
            TokensPerPeriod = 20,
            AutoReplenishment = true
        });
    }
}
