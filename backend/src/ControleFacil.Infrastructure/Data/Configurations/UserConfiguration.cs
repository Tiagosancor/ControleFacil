using ControleFacil.Domain.Entities;
using ControleFacil.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ControleFacil.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        // HasDefaultValue garante DEFAULT no banco pra não quebrar as linhas já
        // existentes quando a coluna nova (NOT NULL) for adicionada via migration.
        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(UserRole.User);

        builder.Property(u => u.PlanType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(PlanType.Free);
    }
}
