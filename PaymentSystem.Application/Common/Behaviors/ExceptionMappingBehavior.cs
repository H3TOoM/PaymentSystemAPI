using MediatR;
using PaymentSystem.Application.Common.Exceptions;

namespace PaymentSystem.Application.Common.Behaviors;

public sealed class ExceptionMappingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (BusinessRuleViolationException)
        {
            throw;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or ArgumentOutOfRangeException)
        {
            throw new BusinessRuleViolationException(ex.Message);
        }
    }
}
