using Inventory.Domain.Models;
using Inventory.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Data.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, value => InventoryItemId.Of(value));

        builder.Property(i => i.ProductId)
            .HasConversion(pid => pid.Value, value => ProductId.Of(value));

        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.ReservedQuantity).IsRequired();

        builder.Ignore(i => i.AvailableQuantity);
    }
}
