using MediatR;
using Microsoft.AspNetCore.Identity;
using PaymentSystem.Application.Common.Extensions;
using PaymentSystem.Application.Services;
using PaymentSystem.Domain.Interfaces;

namespace PaymentSystem.Application.Features.Authentication.Commands.LoginUser;

public sealed class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<object> _passwordHasher;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordHasher<object> passwordHasher)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginUserResult> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        (user is not null).Ensure("Invalid credentials.");
        user!.IsActive.Ensure("Account is inactive.");

        (user.PasswordHash is not null).Ensure("Invalid credentials.");

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            request,
            user.PasswordHash!,
            request.Password);

        (verificationResult == PasswordVerificationResult.Success)
            .Ensure("Invalid credentials.");

        var token = _tokenService.GenerateToken(user.Id, user.Email, user.FullName);

        return new LoginUserResult(token, user.Id, user.Email, user.FullName);
    }
}
