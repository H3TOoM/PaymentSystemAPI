using Microsoft.EntityFrameworkCore;
using PaymentSystem.Domain.Entities;
using PaymentSystem.Domain.Interfaces;
using PaymentSystem.Infrastructure.Data;

namespace PaymentSystem.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Include(u => u.Wallets)
            .Include(u => u.AuditLogs)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _dbContext.Users.Update(user);
    }

    public void Remove(User user)
    {
        _dbContext.Users.Remove(user);
    }
}
