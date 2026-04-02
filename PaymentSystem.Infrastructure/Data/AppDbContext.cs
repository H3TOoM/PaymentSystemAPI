using Microsoft.EntityFrameworkCore;
using PaymentSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PaymentSystem.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Wallet> Wallets { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<TransactionLog> TransactionLogs { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.PasswordHash);
                entity.HasMany(e => e.Wallets).WithOne(w => w.User).HasForeignKey(w => w.UserId);
                entity.HasMany(e => e.AuditLogs).WithOne(a => a.User).HasForeignKey(a => a.UserId);
            });
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Currency).IsRequired();
                entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
                entity.HasMany(e => e.Transactions).WithOne(t => t.Wallet).HasForeignKey(t => t.WalletId);
            });
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ReferenceId).IsRequired();
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.ReferenceId).IsUnique();
                entity.HasMany(e => e.Logs).WithOne(l => l.Transaction).HasForeignKey(l => l.TransactionId);
            });
            modelBuilder.Entity<TransactionLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired();
            });
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired();
                entity.Property(e => e.TargetType).IsRequired();
                entity.Property(e => e.Details).IsRequired();
            });
        }
    }
}
