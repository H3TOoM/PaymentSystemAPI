using PaymentSystem.Application.Common.Exceptions;

namespace PaymentSystem.Application.Common.Extensions;

public static class ApplicationGuardExtensions
{
    public static void Ensure(this bool condition, string message)
    {
        if (!condition)
        {
            throw new BusinessRuleViolationException(message);
        }
    }

    public static Guid EnsureNotEmpty(this Guid value, string message)
    {
        (value != Guid.Empty).Ensure(message);
        return value;
    }

    public static decimal EnsurePositive(this decimal value, string message)
    {
        (value > 0m).Ensure(message);
        return value;
    }

    public static string EnsureRequired(this string? value, string message)
    {
        (!string.IsNullOrWhiteSpace(value)).Ensure(message);
        return value!.Trim();
    }

    public static T EnsureFound<T>(this T? value, string message) where T : class
    {
        (value is not null).Ensure(message);
        return value!;
    }

    public static T EnsureNotNull<T>(this T? value, string message) where T : class
    {
        (value is not null).Ensure(message);
        return value!;
    }
}
