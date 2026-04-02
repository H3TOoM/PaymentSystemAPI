using MediatR;
using Microsoft.AspNetCore.Identity;
using PaymentSystem.Application.Common.Extensions;
using PaymentSystem.Domain.Interfaces;

namespace PaymentSystem.Application.Features.Authentication.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<object> _passwordHasher;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IWalletRepository walletRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher<object> passwordHasher)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        (existingUser is null).Ensure("Email is already registered.");

        var passwordHash = _passwordHasher.HashPassword(request, request.Password);
        passwordHash.EnsureNotNull("Failed to hash password.");

        var user = new Domain.Entities.User(
            Guid.NewGuid(),
            request.Name,
            request.Email,
            passwordHash);

        var wallet = user.CreateWallet(Guid.NewGuid(), "USD");

        await _userRepository.AddAsync(user, cancellationToken);
        await _walletRepository.AddAsync(wallet, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterUserResult(user.Id);
    }
}
