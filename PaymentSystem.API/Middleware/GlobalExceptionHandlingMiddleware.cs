using PaymentSystem.API.Common;
using PaymentSystem.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace PaymentSystem.API.Middleware;

public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. CorrelationId: {CorrelationId}", GetCorrelationId(context));
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.Clear();
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            FluentValidation.ValidationException validationEx => (StatusCodes.Status400BadRequest, "Validation failed.", validationEx.Errors.Select(e => e.ErrorMessage).ToArray()),
            BusinessRuleViolationException => (StatusCodes.Status400BadRequest, exception.Message, Array.Empty<string>()),
            ArgumentException => (StatusCodes.Status400BadRequest, exception.Message, Array.Empty<string>()),
            InvalidOperationException => (StatusCodes.Status400BadRequest, exception.Message, Array.Empty<string>()),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized.", Array.Empty<string>()),
            _ => (StatusCodes.Status500InternalServerError, "An internal server error occurred.", Array.Empty<string>())
        };

        context.Response.StatusCode = statusCode;

        var correlationId = GetCorrelationId(context);
        var response = ApiResponse<object>.ErrorResult(message, errors, correlationId);

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);

        await context.Response.WriteAsync(jsonResponse);
    }

    private static string GetCorrelationId(HttpContext context)
    {
        return context.Response.Headers.TryGetValue("X-Correlation-ID", out var correlationId)
            ? correlationId!
            : Guid.NewGuid().ToString();
    }
}
