using Microsoft.EntityFrameworkCore;
using PaymentSystem.Domain.Entities;
using PaymentSystem.Domain.Interfaces;
using PaymentSystem.Infrastructure.Data;

namespace PaymentSystem.Infrastructure.Repositories;

public sealed class WalletRepository : IWalletRepository
{
    private readonly AppDbContext _dbContext;

    public WalletRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Wallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wallets
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Wallet>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wallets
            .Where(w => w.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default)
    {
        await _dbContext.Wallets.AddAsync(wallet, cancellationToken);
    }

    public void Update(Wallet wallet)
    {
        _dbContext.Wallets.Update(wallet);
    }

    public void Remove(Wallet wallet)
    {
        _dbContext.Wallets.Remove(wallet);
    }
}
