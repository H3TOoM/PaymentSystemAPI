using MediatR;
using PaymentSystem.Application.Common.Extensions;
using PaymentSystem.Domain.Interfaces;

namespace PaymentSystem.Application.Features.Transactions.Queries.GetTransactionHistory;

public sealed class GetTransactionHistoryQueryHandler
    : IRequestHandler<GetTransactionHistoryQuery, IReadOnlyList<TransactionHistoryItem>>
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionHistoryQueryHandler(
        IUserRepository userRepository,
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<IReadOnlyList<TransactionHistoryItem>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        request.UserId.EnsureNotEmpty("UserId is required.");

        var user = (await _userRepository.GetByIdAsync(request.UserId, cancellationToken))
            .EnsureFound("User does not exist.");

        var wallets = await _walletRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (wallets.Count == 0)
        {
            return [];
        }

        var walletIds = wallets.Select(w => w.Id).ToArray();
        var transactions = await _transactionRepository.GetByWalletIdsAsync(walletIds, cancellationToken);

        var history = transactions
            .Select(t => new TransactionHistoryItem(
                t.Id,
                t.ReferenceId,
                t.Type,
                t.Status,
                t.Amount,
                t.CreatedAtUtc,
                t.ProcessedAtUtc))
            .ToList();

        return history;
    }
}
