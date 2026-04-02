using PaymentSystem.Domain.Common;

namespace PaymentSystem.Domain.Entities;

public class AuditLog
{
    private AuditLog()
    {
    }

    public AuditLog(Guid id, Guid userId, string action, string targetType, string details)
    {
        Id = id.EnsureNotEmpty(nameof(id), "Audit log id cannot be empty.");
        UserId = userId.EnsureNotEmpty(nameof(userId), "User id cannot be empty.");
        Action = action.EnsureRequired(nameof(action), "Action is required.");
        TargetType = targetType.EnsureRequired(nameof(targetType), "Target type is required.");
        Details = details?.Trim() ?? string.Empty;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string TargetType { get; private set; } = string.Empty;
    public string Details { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }

    public User? User { get; private set; }
}
