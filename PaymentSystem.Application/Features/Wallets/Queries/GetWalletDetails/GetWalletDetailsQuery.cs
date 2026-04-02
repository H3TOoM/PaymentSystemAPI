using MediatR;
using PaymentSystem.Domain.Enums;

namespace PaymentSystem.Application.Features.Wallets.Queries.GetWalletDetails;

public sealed record GetWalletDetailsQuery(Guid UserId, int LastTransactionsCount = 10) : IRequest<GetWalletDetailsResult>;

public sealed record WalletTransactionSummary(
    Guid TransactionId,
    string ReferenceId,
    TransactionType Type,
    TransactionStatus Status,
    decimal Amount,
    DateTime CreatedAtUtc);

public sealed record GetWalletDetailsResult(
    Guid WalletId,
    Guid UserId,
    decimal Balance,
    string Currency,
    IReadOnlyList<WalletTransactionSummary> LastTransactions);
