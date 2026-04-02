using PaymentSystem.Domain.Enums;

namespace PaymentSystem.Application.Features.Transfers.Commands.TransferMoney;

public sealed record TransferMoneyResult(Guid TransactionId, TransactionStatus Status);
