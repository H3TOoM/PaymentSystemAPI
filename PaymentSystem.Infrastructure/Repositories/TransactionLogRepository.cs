using PaymentSystem.Domain.Entities;
using PaymentSystem.Domain.Interfaces;
using PaymentSystem.Infrastructure.Data;

namespace PaymentSystem.Infrastructure.Repositories;

public sealed class TransactionLogRepository : ITransactionLogRepository
{
    private readonly AppDbContext _dbContext;

    public TransactionLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(TransactionLog transactionLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.TransactionLogs.AddAsync(transactionLog, cancellationToken);
    }
}
