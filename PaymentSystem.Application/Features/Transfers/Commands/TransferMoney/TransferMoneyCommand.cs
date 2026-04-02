using MediatR;

namespace PaymentSystem.Application.Features.Transfers.Commands.TransferMoney;

public sealed record TransferMoneyCommand(
    Guid SenderUserId,
    Guid ReceiverUserId,
    decimal Amount,
    string ReferenceId) : IRequest<TransferMoneyResult>;
