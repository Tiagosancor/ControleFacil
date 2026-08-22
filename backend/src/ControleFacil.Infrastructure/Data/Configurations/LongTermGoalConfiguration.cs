using ControleFacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFacil.Infrastructure.Data.Configurations;

public class LongTermGoalConfiguration : IEntityTypeConfiguration<LongTermGoal>
{
    public void Configure(EntityTypeBuilder<LongTermGoal> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Name).IsRequired().HasMaxLength(200);
        builder.Property(g => g.TargetAmount).HasColumnType("decimal(14,2)");
        builder.Property(g => g.ManualCurrentAmount).HasColumnType("decimal(14,2)");

        builder.HasOne(g => g.InvestmentCategory)
            .WithMany()
            .HasForeignKey(g => g.InvestmentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(g => g.User)
            .WithMany()
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => g.UserId);
    }
}
