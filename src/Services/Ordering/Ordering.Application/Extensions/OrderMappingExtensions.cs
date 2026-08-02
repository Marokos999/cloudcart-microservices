using Ordering.Application.Dtos;
using Ordering.Domain.Models;

namespace Ordering.Application.Extensions;

public static class OrderMappingExtensions
{
    public static OrderDto ToDto(this Order order) => new(
        Id: order.Id.Value,
        CustomerId: order.CustomerId.Value,
        OrderName: order.OrderName.Value,
        ShippingAddress: new AddressDto(
            order.ShippingAddress.FirstName,
            order.ShippingAddress.LastName,
            order.ShippingAddress.EmailAddress,
            order.ShippingAddress.AddressLine,
            order.ShippingAddress.Country,
            order.ShippingAddress.State,
            order.ShippingAddress.ZipCode),
        BillingAddress: new AddressDto(
            order.BillingAddress.FirstName,
            order.BillingAddress.LastName,
            order.BillingAddress.EmailAddress,
            order.BillingAddress.AddressLine,
            order.BillingAddress.Country,
            order.BillingAddress.State,
            order.BillingAddress.ZipCode),
        Payment: new PaymentDto(
            order.Payment.CardName,
            order.Payment.CardNumber,
            order.Payment.Expiration,
            order.Payment.Cvv,
            order.Payment.PaymentMethod),
        Status: order.Status.ToString(),
        OrderItems: order.OrderItems.Select(i => new OrderItemDto(
            i.OrderId.Value,
            i.ProductId.Value,
            i.Quantity,
            i.Price)).ToList(),
        TotalPrice: order.TotalPrice,
        CreatedAt: order.CreatedAt);

    public static IEnumerable<OrderDto> ToDtoList(this IEnumerable<Order> orders) =>
        orders.Select(o => o.ToDto());
}
