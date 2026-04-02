using PaymentSystem.Domain.Common;

namespace PaymentSystem.Domain.Entities;

public class TransactionLog
{
    private TransactionLog()
    {
    }

    public TransactionLog(Guid id, Guid transactionId, string action, string details)
    {
        Id = id.EnsureNotEmpty(nameof(id), "Transaction log id cannot be empty.");
        TransactionId = transactionId.EnsureNotEmpty(nameof(transactionId), "Transaction id cannot be empty.");
        Action = action.EnsureRequired(nameof(action), "Action is required.");
        Details = details?.Trim() ?? string.Empty;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid TransactionId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    public Transaction? Transaction { get; private set; }
}
