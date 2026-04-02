using PaymentSystem.Domain.Common;

namespace PaymentSystem.Domain.Entities;

public class Wallet
{
    private readonly List<Transaction> _transactions = [];

    private Wallet()
    {
    }

    public Wallet(Guid id, Guid userId, string currency)
    {
        Id = id.EnsureNotEmpty(nameof(id), "Wallet id cannot be empty.");
        UserId = userId.EnsureNotEmpty(nameof(userId), "User id cannot be empty.");
        Currency = currency.EnsureRequired(nameof(currency), "Currency is required.").ToUpperInvariant();
        Balance = 0m;
        CreatedAtUtc = DateTime.UtcNow;
        RowVersion = [];
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public User? User { get; private set; }
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    public void Deposit(decimal amount)
    {
        Balance += amount.EnsurePositive(nameof(amount), "Amount must be greater than zero.");
    }

    public void Withdraw(decimal amount)
    {
        var validAmount = amount.EnsurePositive(nameof(amount), "Amount must be greater than zero.");
        (Balance - validAmount >= 0m).Ensure("Wallet balance cannot be negative.");

        Balance -= validAmount;
    }

    public Transaction CreateTransaction(
        Guid transactionId,
        string referenceId,
        Enums.TransactionType type,
        decimal amount)
    {
        EnsureReferenceIdIsUnique(referenceId);

        var transaction = new Transaction(transactionId, Id, referenceId, type, amount);
        _transactions.Add(transaction);
        return transaction;
    }

    private void EnsureReferenceIdIsUnique(string referenceId)
    {
        (!_transactions.Any(t => t.ReferenceId.Equals(referenceId, StringComparison.OrdinalIgnoreCase)))
            .Ensure("Transaction reference id must be unique per wallet.");
    }
}
