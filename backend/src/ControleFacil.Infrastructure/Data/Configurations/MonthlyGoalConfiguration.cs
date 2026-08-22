using ControleFacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFacil.Infrastructure.Data.Configurations;

public class MonthlyGoalConfiguration : IEntityTypeConfiguration<MonthlyGoal>
{
    public void Configure(EntityTypeBuilder<MonthlyGoal> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.IncomeGoal).HasColumnType("decimal(14,2)");
        builder.Property(g => g.ExpenseGoal).HasColumnType("decimal(14,2)");

        builder.HasOne(g => g.User)
            .WithMany()
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // No máximo uma meta por usuário por mês.
        builder.HasIndex(g => new { g.UserId, g.Year, g.Month }).IsUnique();
    }
}
