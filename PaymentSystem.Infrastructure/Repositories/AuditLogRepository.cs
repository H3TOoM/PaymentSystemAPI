using PaymentSystem.Domain.Entities;
using PaymentSystem.Domain.Interfaces;
using PaymentSystem.Infrastructure.Data;

namespace PaymentSystem.Infrastructure.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _dbContext;

    public AuditLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
    }
}
