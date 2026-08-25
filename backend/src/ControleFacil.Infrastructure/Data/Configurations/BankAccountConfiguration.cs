using ControleFacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFacil.Infrastructure.Data.Configurations;

public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.InitialBalance).HasColumnType("decimal(14,2)");

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(b => b.BankIspb).HasMaxLength(8);
        builder.HasOne(b => b.Bank)
            .WithMany()
            .HasForeignKey(b => b.BankIspb)
            .HasPrincipalKey(bank => bank.Ispb)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(b => b.UserId);
    }
}
