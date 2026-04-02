using System.Diagnostics;
using System.Text.Json;

namespace PaymentSystem.API.Middleware;

public sealed class RequestLoggingMiddleware
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
        var correlationId = GetCorrelationId(context);
        var stopwatch = Stopwatch.StartNew();

        var request = await FormatRequestAsync(context);
        _logger.LogInformation(
            "HTTP {Method} {Path} - CorrelationId: {CorrelationId} - Request: {Request}",
            context.Request.Method,
            context.Request.Path,
            correlationId,
            request);

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var response = await FormatResponseAsync(context.Response);
            _logger.LogInformation(
                "HTTP {Method} {Path} - {StatusCode} - {ElapsedMs}ms - CorrelationId: {CorrelationId} - Response: {Response}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId,
                response);

            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private static async Task<string> FormatRequestAsync(HttpContext context)
    {
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        var requestObj = new
        {
            Scheme = context.Request.Scheme,
            Host = context.Request.Host.ToString(),
            Path = context.Request.Path,
            QueryString = context.Request.QueryString.ToString(),
            Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Body = body
        };

        return JsonSerializer.Serialize(requestObj, new JsonSerializerOptions { WriteIndented = false });
    }

    private static async Task<string> FormatResponseAsync(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(response.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        var responseObj = new
        {
            StatusCode = response.StatusCode,
            Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Body = body
        };

        return JsonSerializer.Serialize(responseObj, new JsonSerializerOptions { WriteIndented = false });
    }

    private static string GetCorrelationId(HttpContext context)
    {
        return context.Response.Headers.TryGetValue("X-Correlation-ID", out var correlationId)
            ? correlationId!
            : Guid.NewGuid().ToString();
    }
}
