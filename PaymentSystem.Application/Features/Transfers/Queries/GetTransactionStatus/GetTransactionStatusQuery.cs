using MediatR;
using PaymentSystem.Domain.Enums;

namespace PaymentSystem.Application.Features.Transfers.Queries.GetTransactionStatus;

public sealed record GetTransactionStatusQuery(string ReferenceId) : IRequest<GetTransactionStatusResult?>;

public sealed record GetTransactionStatusResult(Guid TransactionId, string ReferenceId, TransactionStatus Status);
