namespace PaymentSystem.Domain.Common;

public static class GuardExtensions
{
    public static Guid EnsureNotEmpty(this Guid value, string paramName, string message)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(message, paramName);
        }

        return value;
    }

    public static string EnsureRequired(this string? value, string paramName, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message, paramName);
        }

        return value.Trim();
    }

    public static decimal EnsurePositive(this decimal value, string paramName, string message)
    {
        if (value <= 0m)
        {
            throw new ArgumentOutOfRangeException(paramName, message);
        }

        return value;
    }

    public static void Ensure(this bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
