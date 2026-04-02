using MediatR;

namespace PaymentSystem.Application.Features.Authentication.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Name,
    string Email,
    string Password) : IRequest<RegisterUserResult>;
