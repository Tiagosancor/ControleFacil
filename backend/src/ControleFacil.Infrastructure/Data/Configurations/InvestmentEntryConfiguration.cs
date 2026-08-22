using ControleFacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFacil.Infrastructure.Data.Configurations;

public class InvestmentEntryConfiguration : IEntityTypeConfiguration<InvestmentEntry>
{
    public void Configure(EntityTypeBuilder<InvestmentEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Value).HasColumnType("decimal(14,2)");

        builder.HasOne(e => e.InvestmentCategory)
            .WithMany()
            .HasForeignKey(e => e.InvestmentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // No máximo um valor lançado por categoria de investimento por mês por usuário.
        builder.HasIndex(e => new { e.UserId, e.InvestmentCategoryId, e.Year, e.Month }).IsUnique();
    }
}
