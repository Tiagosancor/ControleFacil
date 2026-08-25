using ControleFacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFacil.Infrastructure.Data.Configurations;

public class InvestmentCategoryConfiguration : IEntityTypeConfiguration<InvestmentCategory>
{
    public void Configure(EntityTypeBuilder<InvestmentCategory> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);

        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.AppliedAmount).HasColumnType("decimal(14,2)");
        builder.Property(c => c.InterestRate).HasColumnType("decimal(7,4)");
        builder.Property(c => c.MonthlyContribution).HasColumnType("decimal(14,2)");

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.UserId);
    }
}
