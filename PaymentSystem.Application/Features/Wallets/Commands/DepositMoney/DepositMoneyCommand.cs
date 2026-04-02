using MediatR;

namespace PaymentSystem.Application.Features.Wallets.Commands.DepositMoney;

public sealed record DepositMoneyCommand(
    Guid UserId,
    decimal Amount,
    string? ReferenceId = null) : IRequest<DepositMoneyResult>;
