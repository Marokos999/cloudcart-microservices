using Basket.API.Data;
using Basket.API.Features.StoreBasket;
using Basket.API.Models;
using FluentAssertions;
using NSubstitute;

namespace Basket.API.UnitTests.Features;

public class StoreBasketHandlerTests
{
    private readonly IBasketRepository _repository;

    public StoreBasketHandlerTests()
    {
        _repository = Substitute.For<IBasketRepository>();
    }

    [Fact]
    public async Task Handle_ShouldStoreBasket_AndReturnUserName()
    {
        // Empty items — avoids gRPC discount call (gRPC client cannot be mocked with NSubstitute)
        var cart = new ShoppingCart("john_doe");
        _repository.StoreBasket(cart, Arg.Any<CancellationToken>()).Returns(cart);

        var discountClient = Substitute.For<DiscountProtoService.DiscountProtoServiceClient>();
        var handler = new StoreBasketHandler(_repository, discountClient);

        var result = await handler.Handle(new StoreBasketCommand(cart), CancellationToken.None);

        result.UserName.Should().Be("john_doe");
        await _repository.Received(1).StoreBasket(cart, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldStoreEmptyBasket()
    {
        var cart = new ShoppingCart("empty_user");
        _repository.StoreBasket(cart, Arg.Any<CancellationToken>()).Returns(cart);

        var discountClient = Substitute.For<DiscountProtoService.DiscountProtoServiceClient>();
        var handler = new StoreBasketHandler(_repository, discountClient);

        var result = await handler.Handle(new StoreBasketCommand(cart), CancellationToken.None);

        result.UserName.Should().Be("empty_user");
    }
}
