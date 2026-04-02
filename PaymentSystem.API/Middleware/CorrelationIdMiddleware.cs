using System.Diagnostics;

namespace PaymentSystem.API.Middleware;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        context.Response.Headers.Append("X-Correlation-ID", correlationId);

        using var activity = Activity.Current;
        activity?.SetTag("correlation.id", correlationId);

        await _next(context);
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        const string correlationIdHeaderName = "X-Correlation-ID";

        if (context.Request.Headers.TryGetValue(correlationIdHeaderName, out var correlationId) &&
            !string.IsNullOrEmpty(correlationId))
        {
            return correlationId!;
        }

        return Guid.NewGuid().ToString();
    }
}
