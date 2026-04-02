namespace PaymentSystem.Application.Features.Authentication.Commands.LoginUser;

public sealed record LoginUserResult(
    string Token,
    Guid UserId,
    string Email,
    string Name);
