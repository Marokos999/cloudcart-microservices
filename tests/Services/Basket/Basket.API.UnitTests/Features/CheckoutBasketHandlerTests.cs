using Basket.API.Data;
using Basket.API.Features.CheckoutBasket;
using Basket.API.Models;
using CloudCart.BuildingBlocks.Events;
using FluentAssertions;
using MassTransit;
using NSubstitute;

namespace Basket.API.UnitTests.Features;

public class CheckoutBasketHandlerTests
{
    private readonly IBasketRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly CheckoutBasketHandler _handler;

    public CheckoutBasketHandlerTests()
    {
        _repository = Substitute.For<IBasketRepository>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _handler = new CheckoutBasketHandler(_repository, _publishEndpoint);
    }

    [Fact]
    public async Task Handle_ShouldPublishEvent_AndDeleteBasket()
    {
        var cart = new ShoppingCart("john_doe")
        {
            Items = [new ShoppingCartItem { ProductId = "1", ProductName = "PlayStation 5", Price = 499.99m, Quantity = 1 }]
        };
        _repository.GetBasket("john_doe", Arg.Any<CancellationToken>()).Returns(cart);
        _repository.DeleteBasket("john_doe", Arg.Any<CancellationToken>()).Returns(true);

        var request = new CheckoutBasketRequest(
            "john_doe", Guid.NewGuid(), "John", "Doe", "john@example.com",
            "123 Main St", "USA", "NY", "10001",
            "John Doe", "4111111111111111", "12/26", "123", 1);

        var result = await _handler.Handle(new CheckoutBasketCommand(request), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _publishEndpoint.Received(1).Publish(Arg.Any<BasketCheckoutEvent>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).DeleteBasket("john_doe", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSetTotalPrice_FromCart()
    {
        var cart = new ShoppingCart("jane_doe")
        {
            Items =
            [
                new ShoppingCartItem { ProductId = "1", ProductName = "Xbox", Price = 499.99m, Quantity = 2 }
            ]
        };
        _repository.GetBasket("jane_doe", Arg.Any<CancellationToken>()).Returns(cart);
        _repository.DeleteBasket("jane_doe", Arg.Any<CancellationToken>()).Returns(true);

        BasketCheckoutEvent? publishedEvent = null;
        await _publishEndpoint.Publish(Arg.Do<BasketCheckoutEvent>(e => publishedEvent = e), Arg.Any<CancellationToken>());

        var request = new CheckoutBasketRequest(
            "jane_doe", Guid.NewGuid(), "Jane", "Doe", "jane@example.com",
            "456 Elm St", "USA", "CA", "90001",
            "Jane Doe", "4111111111111111", "12/26", "321", 1);

        await _handler.Handle(new CheckoutBasketCommand(request), CancellationToken.None);

        publishedEvent?.TotalPrice.Should().Be(999.98m);
    }
}
