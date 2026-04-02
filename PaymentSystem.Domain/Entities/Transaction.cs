using PaymentSystem.Domain.Common;
using PaymentSystem.Domain.Enums;

namespace PaymentSystem.Domain.Entities;

public class Transaction
{
    private readonly List<TransactionLog> _logs = [];

    private Transaction()
    {
    }

    public Transaction(
        Guid id,
        Guid walletId,
        string referenceId,
        TransactionType type,
        decimal amount)
    {
        Id = id.EnsureNotEmpty(nameof(id), "Transaction id cannot be empty.");
        WalletId = walletId.EnsureNotEmpty(nameof(walletId), "Wallet id cannot be empty.");
        ReferenceId = referenceId.EnsureRequired(nameof(referenceId), "Reference id is required.");
        Type = type;
        Amount = amount.EnsurePositive(nameof(amount), "Amount must be greater than zero.");
        Status = TransactionStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid WalletId { get; private set; }
    public string ReferenceId { get; private set; } = string.Empty;
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public string? FailureReason { get; private set; }

    public Wallet? Wallet { get; private set; }
    public IReadOnlyCollection<TransactionLog> Logs => _logs.AsReadOnly();

    public void MarkCompleted()
    {
        (Status == TransactionStatus.Pending).Ensure("Only pending transactions can be completed.");

        Status = TransactionStatus.Completed;
        ProcessedAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        (Status == TransactionStatus.Pending).Ensure("Only pending transactions can be failed.");

        Status = TransactionStatus.Failed;
        FailureReason = reason.EnsureRequired(nameof(reason), "Failure reason is required.");
        ProcessedAtUtc = DateTime.UtcNow;
    }

    public TransactionLog AddLog(Guid logId, string action, string details)
    {
        var log = new TransactionLog(logId, Id, action, details);
        _logs.Add(log);
        return log;
    }
}
