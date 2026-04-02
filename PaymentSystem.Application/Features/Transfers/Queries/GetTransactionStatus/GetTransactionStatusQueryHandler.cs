using MediatR;
using PaymentSystem.Application.Common.Extensions;
using PaymentSystem.Domain.Interfaces;

namespace PaymentSystem.Application.Features.Transfers.Queries.GetTransactionStatus;

public sealed class GetTransactionStatusQueryHandler
    : IRequestHandler<GetTransactionStatusQuery, GetTransactionStatusResult?>
{
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionStatusQueryHandler(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<GetTransactionStatusResult?> Handle(GetTransactionStatusQuery request, CancellationToken cancellationToken)
    {
        request.ReferenceId.EnsureRequired("ReferenceId is required.");

        var transaction = await _transactionRepository.GetByReferenceIdAsync(request.ReferenceId, cancellationToken);
        if (transaction is null)
        {
            return null;
        }

        return new GetTransactionStatusResult(transaction.Id, transaction.ReferenceId, transaction.Status);
    }
}
