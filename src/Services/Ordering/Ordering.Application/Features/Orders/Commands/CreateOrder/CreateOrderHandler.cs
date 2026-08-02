using Ordering.Application.Contracts;
using Ordering.Application.Dtos;

namespace Ordering.Application.Features.Orders.Commands.CreateOrder;

public record CreateOrderCommand(OrderDto Order) : ICommand<CreateOrderResult>;
public record CreateOrderResult(Guid Id);

public class CreateOrderHandler(IOrderRepository repository) : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = CreateNewOrder(command.Order);
        await repository.AddAsync(order, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return new CreateOrderResult(order.Id.Value);
    }

    private static Order CreateNewOrder(OrderDto dto)
    {
        var shippingAddress = Address.Of(
            dto.ShippingAddress.FirstName, dto.ShippingAddress.LastName,
            dto.ShippingAddress.EmailAddress, dto.ShippingAddress.AddressLine,
            dto.ShippingAddress.Country, dto.ShippingAddress.State,
            dto.ShippingAddress.ZipCode);

        var billingAddress = Address.Of(
            dto.BillingAddress.FirstName, dto.BillingAddress.LastName,
            dto.BillingAddress.EmailAddress, dto.BillingAddress.AddressLine,
            dto.BillingAddress.Country, dto.BillingAddress.State,
            dto.BillingAddress.ZipCode);

        var payment = Payment.Of(
            dto.Payment.CardName, dto.Payment.CardNumber,
            dto.Payment.Expiration, dto.Payment.Cvv,
            dto.Payment.PaymentMethod);

        var order = Order.Create(
            OrderId.Of(Guid.NewGuid()),
            CustomerId.Of(dto.CustomerId),
            OrderName.Of(dto.OrderName),
            shippingAddress,
            billingAddress,
            payment);

        foreach (var item in dto.OrderItems)
            order.AddItem(ProductId.Of(item.ProductId), item.Quantity, item.Price);

        return order;
    }
}