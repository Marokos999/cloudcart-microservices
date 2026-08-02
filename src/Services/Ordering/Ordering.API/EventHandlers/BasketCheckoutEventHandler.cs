using CloudCart.BuildingBlocks.Events;
using MassTransit;
using Ordering.Application.Contracts;
using Ordering.Application.Dtos;
using Ordering.Application.Features.Orders.Commands.CreateOrder;
using MediatR;

namespace Ordering.API.EventHandlers;

public class BasketCheckoutEventHandler(ISender sender) : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        var message = context.Message;
        var orderId = Guid.NewGuid();

        var orderItems = message.Items
            .Where(i => Guid.TryParse(i.ProductId, out _))
            .Select(i => new OrderItemDto(orderId, Guid.Parse(i.ProductId), i.Quantity, i.Price))
            .ToList();

        var order = new OrderDto(
            Id: orderId,
            CustomerId: message.CustomerId,
            OrderName: $"ORD_{message.UserName}_{DateTime.UtcNow:yyyyMMddHHmmss}",
            ShippingAddress: new AddressDto(
                message.FirstName, message.LastName, message.EmailAddress,
                message.AddressLine, message.Country, message.State, message.ZipCode),
            BillingAddress: new AddressDto(
                message.FirstName, message.LastName, message.EmailAddress,
                message.AddressLine, message.Country, message.State, message.ZipCode),
            Payment: new PaymentDto(
                message.CardName, message.CardNumber,
                message.Expiration, message.Cvv, message.PaymentMethod),
            Status: "Pending",
            OrderItems: orderItems,
            TotalPrice: message.TotalPrice,
            CreatedAt: DateTime.UtcNow
        );

        await sender.Send(new CreateOrderCommand(order));
    }
}