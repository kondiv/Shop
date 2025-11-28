using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Configurations;

internal sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(i => i.Id);

        builder
            .Property(i => i.Id)
            .ValueGeneratedOnAdd();

        builder
            .Property(i => i.Name)
            .HasMaxLength(128);

        builder
            .Property(i => i.Category)
            .HasConversion<string>();

        builder
            .HasOne(i => i.Seller)
            .WithMany()
            .HasForeignKey(i => i.SellerId);
    }
}
