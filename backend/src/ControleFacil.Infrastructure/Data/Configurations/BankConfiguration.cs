using ControleFacil.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFacil.Infrastructure.Data.Configurations;

public class BankConfiguration : IEntityTypeConfiguration<Bank>
{
    public void Configure(EntityTypeBuilder<Bank> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Ispb).IsRequired().HasMaxLength(8);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.FullName).HasMaxLength(200);
        builder.Property(b => b.LogoUrl).HasMaxLength(500);

        builder.HasIndex(b => b.Ispb).IsUnique();
        builder.HasIndex(b => b.Name);
    }
}
