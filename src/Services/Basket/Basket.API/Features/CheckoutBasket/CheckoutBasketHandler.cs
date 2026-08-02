using Basket.API.Data;
using CloudCart.BuildingBlocks.CQRS;
using CloudCart.BuildingBlocks.Events;
using MassTransit;

namespace Basket.API.Features.CheckoutBasket;

public record CheckoutBasketCommand(CheckoutBasketRequest Request) : ICommand<CheckoutBasketResult>;
public record CheckoutBasketResult(bool IsSuccess);
public record CheckoutBasketRequest(
    string UserName,
    Guid CustomerId,
    string FirstName,
    string LastName,
    string EmailAddress,
    string AddressLine,
    string Country,
    string State,
    string ZipCode,
    string CardName,
    string CardNumber,
    string Expiration,
    string Cvv,
    int PaymentMethod);

public class CheckoutBasketHandler(IBasketRepository repository, IPublishEndpoint publishEndpoint)
    : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
{
    public async Task<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
    {
        var cart = await repository.GetBasket(command.Request.UserName, cancellationToken);

        var eventMessage = new BasketCheckoutEvent
        {
            UserName = cart.UserName,
            CustomerId = command.Request.CustomerId,
            FirstName = command.Request.FirstName,
            LastName = command.Request.LastName,
            EmailAddress = command.Request.EmailAddress,
            AddressLine = command.Request.AddressLine,
            Country = command.Request.Country,
            State = command.Request.State,
            ZipCode = command.Request.ZipCode,
            CardName = command.Request.CardName,
            CardNumber = command.Request.CardNumber,
            Expiration = command.Request.Expiration,
            Cvv = command.Request.Cvv,
            PaymentMethod = command.Request.PaymentMethod,
            TotalPrice = cart.TotalPrice,
            Items = cart.Items.Select(i => new BasketCheckoutItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList()
        };

        await publishEndpoint.Publish(eventMessage, cancellationToken);
        await repository.DeleteBasket(cart.UserName, cancellationToken);

        return new CheckoutBasketResult(true);
    }
}