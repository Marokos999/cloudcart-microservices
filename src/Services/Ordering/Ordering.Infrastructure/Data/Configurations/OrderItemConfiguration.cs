using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;

namespace Ordering.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.Id)
            .HasConversion(id => id.Value, value => OrderItemId.Of(value));

        builder.Property(oi => oi.OrderId)
            .HasConversion(id => id.Value, value => OrderId.Of(value));

        builder.Property(oi => oi.ProductId)
            .HasConversion(id => id.Value, value => ProductId.Of(value));

        builder.Property(oi => oi.Quantity).IsRequired();
        builder.Property(oi => oi.Price).HasColumnType("decimal(18,2)").IsRequired();
    }
}