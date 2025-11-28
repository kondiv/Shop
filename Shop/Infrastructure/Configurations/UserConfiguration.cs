using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasIndex(u => u.Login).IsUnique();

        builder
            .Property(u => u.Username)
            .HasMaxLength(64);

        builder
            .Property(u => u.Login)
            .HasMaxLength(64);

        builder
            .Property(u => u.PasswordHash)
            .HasMaxLength(512);

        builder
            .Property(u => u.Role)
            .HasConversion<string>();
    }
}
