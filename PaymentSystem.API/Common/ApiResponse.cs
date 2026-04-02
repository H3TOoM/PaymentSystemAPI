namespace PaymentSystem.API.Common;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public string[] Errors { get; init; } = [];
    public string CorrelationId { get; init; } = string.Empty;

    public static ApiResponse<T> SuccessResult(T data, string message = "Operation completed successfully.", string correlationId = "")
        => new() { Success = true, Message = message, Data = data, CorrelationId = correlationId };

    public static ApiResponse<T> ErrorResult(string message, string[] errors, string correlationId = "")
        => new() { Success = false, Message = message, Errors = errors, CorrelationId = correlationId };
}
