using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Models;
using Ordering.Domain.ValueObjects;

namespace Ordering.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, value => OrderId.Of(value));

        builder.HasOne<OrderItem>().WithOne()
            .HasForeignKey<OrderItem>(oi => oi.OrderId);

        builder.ComplexProperty(o => o.OrderName, nb =>
            nb.Property(n => n.Value).HasColumnName("OrderName").IsRequired());

        builder.ComplexProperty(o => o.ShippingAddress, ab => {
            ab.Property(a => a.FirstName).HasMaxLength(50).IsRequired();
            ab.Property(a => a.LastName).HasMaxLength(50).IsRequired();
            ab.Property(a => a.EmailAddress).HasMaxLength(50);
            ab.Property(a => a.AddressLine).HasMaxLength(180).IsRequired();
            ab.Property(a => a.Country).HasMaxLength(50);
            ab.Property(a => a.State).HasMaxLength(50);
            ab.Property(a => a.ZipCode).HasMaxLength(10).IsRequired();
        });

        builder.ComplexProperty(o => o.BillingAddress, ab => {
            ab.Property(a => a.FirstName).HasMaxLength(50).IsRequired();
            ab.Property(a => a.LastName).HasMaxLength(50).IsRequired();
            ab.Property(a => a.EmailAddress).HasMaxLength(50);
            ab.Property(a => a.AddressLine).HasMaxLength(180).IsRequired();
            ab.Property(a => a.Country).HasMaxLength(50);
            ab.Property(a => a.State).HasMaxLength(50);
            ab.Property(a => a.ZipCode).HasMaxLength(10).IsRequired();
        });

        builder.ComplexProperty(o => o.Payment, pb => {
            pb.Property(p => p.CardName).HasMaxLength(50);
            pb.Property(p => p.CardNumber).HasMaxLength(24).IsRequired();
            pb.Property(p => p.Expiration).HasMaxLength(10);
            pb.Property(p => p.Cvv).HasMaxLength(3);
            pb.Property(p => p.PaymentMethod);
        });

        builder.Property(o => o.Status)
            .HasDefaultValue(OrderStatus.Pending)
            .HasSentinel(OrderStatus.Pending)
            .HasConversion(s => s.ToString(), s => (OrderStatus)Enum.Parse(typeof(OrderStatus), s));

        builder.Property(o => o.CustomerId)
            .HasConversion(id => id.Value, value => CustomerId.Of(value));

        builder.HasMany(o => o.OrderItems)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId);
    }
}