using PaymentSystem.Domain.Entities;

namespace PaymentSystem.Domain.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Transaction?> GetByReferenceIdAsync(string referenceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> GetByWalletIdsAsync(
        IReadOnlyCollection<Guid> walletIds,
        CancellationToken cancellationToken = default);
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    void Update(Transaction transaction);
}
