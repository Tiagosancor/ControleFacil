using ControleFacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFacil.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(300);
        builder.Property(t => t.Amount).HasColumnType("decimal(14,2)");
        builder.Property(t => t.PaymentMethod).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.BankAccount)
            .WithMany()
            .HasForeignKey(t => t.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CreditCard)
            .WithMany()
            .HasForeignKey(t => t.CreditCardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Series)
            .WithMany(s => s.Transactions)
            .HasForeignKey(t => t.SeriesId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => new { t.UserId, t.EntryDate });

        // Usado pelo job de alertas de vencimento (Sprint F): busca lançamentos
        // pendentes ainda não avisados com vencimento dentro da janela configurada,
        // cruzando todos os usuários — não é escopado por UserId como o índice acima.
        builder.HasIndex(t => new { t.Status, t.DueAlertSentAt, t.EntryDate });
    }
}
