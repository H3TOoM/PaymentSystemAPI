using PaymentSystem.Domain.Common;

namespace PaymentSystem.Domain.Entities;

public class User
{
    private readonly List<Wallet> _wallets = [];
    private readonly List<AuditLog> _auditLogs = [];

    private User()
    {
    }

    public User(Guid id, string fullName, string email)
    {
        Id = id.EnsureNotEmpty(nameof(id), "User id cannot be empty.");
        FullName = fullName.EnsureRequired(nameof(fullName), "Full name is required.");
        Email = email.EnsureRequired(nameof(email), "Email is required.");
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<Wallet> Wallets => _wallets.AsReadOnly();
    public IReadOnlyCollection<AuditLog> AuditLogs => _auditLogs.AsReadOnly();

    public Wallet CreateWallet(Guid walletId, string currency)
    {
        IsActive.Ensure("Inactive users cannot create wallets.");
        (!_wallets.Any(w => w.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase)))
            .Ensure("A wallet with the same currency already exists for this user.");

        var wallet = new Wallet(walletId, Id, currency);
        _wallets.Add(wallet);
        return wallet;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public AuditLog AddAuditLog(Guid auditLogId, string action, string targetType, string details)
    {
        var auditLog = new AuditLog(auditLogId, Id, action, targetType, details);
        _auditLogs.Add(auditLog);
        return auditLog;
    }
}
