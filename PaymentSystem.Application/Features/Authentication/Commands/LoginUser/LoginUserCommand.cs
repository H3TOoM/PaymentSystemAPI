using MediatR;

namespace PaymentSystem.Application.Features.Authentication.Commands.LoginUser;

public sealed record LoginUserCommand(
    string Email,
    string Password) : IRequest<LoginUserResult>;
