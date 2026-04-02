using PaymentSystem.Domain.Entities;

namespace PaymentSystem.Domain.Interfaces;

public interface ITransactionLogRepository
{
    Task AddAsync(TransactionLog transactionLog, CancellationToken cancellationToken = default);
}
