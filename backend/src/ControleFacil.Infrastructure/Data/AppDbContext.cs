using ControleFacil.Domain.Entities;
using ControleFacil.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ControleFacil.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionSeries> TransactionSeries => Set<TransactionSeries>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new BankAccountConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionSeriesConfiguration());
        modelBuilder.ApplyConfiguration(new TransactionConfiguration());
        modelBuilder.ApplyConfiguration(new PasswordResetTokenConfiguration());
    }
}
