using PaymentSystem.Domain.Enums;

namespace PaymentSystem.Application.Features.Wallets.Commands.WithdrawMoney;

public sealed record WithdrawMoneyResult(Guid TransactionId, TransactionStatus Status);
