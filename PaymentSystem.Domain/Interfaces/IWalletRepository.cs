using PaymentSystem.Domain.Entities;

namespace PaymentSystem.Domain.Interfaces;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Wallet>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);
    void Update(Wallet wallet);
    void Remove(Wallet wallet);
}
