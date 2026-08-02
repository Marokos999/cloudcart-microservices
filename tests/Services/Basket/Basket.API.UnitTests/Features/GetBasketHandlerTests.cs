using Basket.API.Data;
using Basket.API.Features.GetBasket;
using Basket.API.Models;
using FluentAssertions;
using NSubstitute;

namespace Basket.API.UnitTests.Features;

public class GetBasketHandlerTests
{
    private readonly IBasketRepository _repository;
    private readonly GetBasketHandler _handler;

    public GetBasketHandlerTests()
    {
        _repository = Substitute.For<IBasketRepository>();
        _handler = new GetBasketHandler(_repository);
    }

    [Fact]
    public async Task Handle_ShouldReturnCart_WhenCartExists()
    {
        var cart = new ShoppingCart("john_doe")
        {
            Items =
            [
                new ShoppingCartItem { ProductId = "1", ProductName = "PlayStation 5", Price = 499.99m, Quantity = 1 }
            ]
        };
        _repository.GetBasket("john_doe", Arg.Any<CancellationToken>()).Returns(cart);

        var result = await _handler.Handle(new GetBasketQuery("john_doe"), CancellationToken.None);

        result.Cart.UserName.Should().Be("john_doe");
        result.Cart.Items.Should().HaveCount(1);
        result.Cart.TotalPrice.Should().Be(499.99m);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyCart_WhenCartDoesNotExist()
    {
        _repository.GetBasket("unknown", Arg.Any<CancellationToken>())
            .Returns(new ShoppingCart("unknown"));

        var result = await _handler.Handle(new GetBasketQuery("unknown"), CancellationToken.None);

        result.Cart.UserName.Should().Be("unknown");
        result.Cart.Items.Should().BeEmpty();
        result.Cart.TotalPrice.Should().Be(0);
    }

    [Fact]
    public void GetBasketQuery_ShouldContainUserName()
    {
        var query = new GetBasketQuery("jane_doe");
        query.UserName.Should().Be("jane_doe");
    }
}
