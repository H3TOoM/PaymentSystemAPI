using MediatR;
using PaymentSystem.Domain.Enums;

namespace PaymentSystem.Application.Features.Transactions.Queries.GetTransactionHistory;

public sealed record GetTransactionHistoryQuery(Guid UserId) : IRequest<IReadOnlyList<TransactionHistoryItem>>;

public sealed record TransactionHistoryItem(
    Guid TransactionId,
    string ReferenceId,
    TransactionType Type,
    TransactionStatus Status,
    decimal Amount,
    DateTime CreatedAtUtc,
    DateTime? ProcessedAtUtc);
