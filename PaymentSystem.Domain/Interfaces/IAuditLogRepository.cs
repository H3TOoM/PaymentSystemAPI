using PaymentSystem.Domain.Entities;

namespace PaymentSystem.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
