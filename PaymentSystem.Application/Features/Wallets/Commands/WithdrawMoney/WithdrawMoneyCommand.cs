using MediatR;

namespace PaymentSystem.Application.Features.Wallets.Commands.WithdrawMoney;

public sealed record WithdrawMoneyCommand(
    Guid UserId,
    decimal Amount,
    string? ReferenceId = null) : IRequest<WithdrawMoneyResult>;
