using MediatR;
using PaymentSystem.Application.Common.Extensions;
using PaymentSystem.Domain.Interfaces;

namespace PaymentSystem.Application.Features.Wallets.Queries.GetWalletDetails;

public sealed class GetWalletDetailsQueryHandler : IRequestHandler<GetWalletDetailsQuery, GetWalletDetailsResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetWalletDetailsQueryHandler(
        IUserRepository userRepository,
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<GetWalletDetailsResult> Handle(GetWalletDetailsQuery request, CancellationToken cancellationToken)
    {
        request.UserId.EnsureNotEmpty("UserId is required.");

        var user = (await _userRepository.GetByIdAsync(request.UserId, cancellationToken))
            .EnsureFound("User does not exist.");

        var wallet = (await _walletRepository.GetByUserIdAsync(user.Id, cancellationToken)).FirstOrDefault()
            .EnsureFound("Wallet does not exist.");

        var take = request.LastTransactionsCount <= 0 ? 10 : request.LastTransactionsCount;

        var transactions = await _transactionRepository.GetByWalletIdsAsync([wallet.Id], cancellationToken);

        var lastTransactions = transactions
            .Take(take)
            .Select(t => new WalletTransactionSummary(
                t.Id,
                t.ReferenceId,
                t.Type,
                t.Status,
                t.Amount,
                t.CreatedAtUtc))
            .ToList();

        return new GetWalletDetailsResult(
            wallet.Id,
            user.Id,
            wallet.Balance,
            wallet.Currency,
            lastTransactions);
    }
}
