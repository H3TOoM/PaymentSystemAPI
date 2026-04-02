using PaymentSystem.Domain.Enums;

namespace PaymentSystem.Application.Features.Wallets.Commands.DepositMoney;

public sealed record DepositMoneyResult(Guid TransactionId, TransactionStatus Status);
